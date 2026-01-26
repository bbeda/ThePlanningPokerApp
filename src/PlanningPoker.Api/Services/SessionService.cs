using System.Collections.Concurrent;
using System.Security.Cryptography;
using PlanningPoker.Api.DTOs;
using PlanningPoker.Api.Models;

namespace PlanningPoker.Api.Services;

public class SessionService : ISessionService
{
    private readonly ConcurrentDictionary<string, Session> _sessions = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _sessionLocks = new();
    private readonly ISseNotificationService _sseService;
    private readonly ILogger<SessionService> _logger;

    public SessionService(ISseNotificationService sseService, ILogger<SessionService> logger)
    {
        _sseService = sseService;
        _logger = logger;
    }

    public async Task<Session> CreateSessionAsync(string ownerName, string? browserId = null)
    {
        var sessionCode = GenerateUniqueSessionCode();
        var now = DateTime.UtcNow;

        var owner = new User
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = sessionCode,
            Name = ownerName,
            IsOwner = true,
            JoinedAt = now,
            LastSeenAt = now,
            BrowserId = browserId
        };

        var session = new Session
        {
            Id = sessionCode,
            OwnerId = owner.Id,
            CreatedAt = now,
            LastActivityAt = now
        };

        session.Users.TryAdd(owner.Id, owner);
        _sessions.TryAdd(sessionCode, session);

        _logger.LogInformation("Session {SessionCode} created by {OwnerName} (BrowserId: {BrowserId})",
            sessionCode, ownerName, browserId ?? "none");

        return session;
    }

    public async Task<Session?> GetSessionAsync(string sessionCode)
    {
        _sessions.TryGetValue(sessionCode, out var session);
        if (session != null)
        {
            await UpdateActivityAsync(sessionCode);
        }
        return session;
    }

    public async Task<User> JoinSessionAsync(string sessionCode, string userName, string? browserId = null)
    {
        var session = await GetSessionAsync(sessionCode);
        if (session == null)
        {
            throw new SessionNotFoundException(sessionCode);
        }

        var now = DateTime.UtcNow;

        // Check if browser ID exists (reconnection attempt)
        if (!string.IsNullOrEmpty(browserId))
        {
            var existingUser = session.Users.Values.FirstOrDefault(u =>
                !string.IsNullOrEmpty(u.BrowserId) &&
                u.BrowserId.Equals(browserId, StringComparison.Ordinal));

            if (existingUser != null)
            {
                // Reconnecting - update LastSeenAt and return existing user
                existingUser.LastSeenAt = now;
                _logger.LogInformation("User {UserName} reconnected to session {SessionCode} (BrowserId: {BrowserId})",
                    userName, sessionCode, browserId);
                return existingUser;
            }
        }

        // Check if username is already taken
        if (session.Users.Values.Any(u => u.Name.Equals(userName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException($"Username '{userName}' is already taken in this session");
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = sessionCode,
            Name = userName,
            IsOwner = false,
            JoinedAt = now,
            LastSeenAt = now,
            BrowserId = browserId
        };

        session.Users.TryAdd(user.Id, user);
        await UpdateActivityAsync(sessionCode);

        // Notify other users
        await _sseService.NotifySessionAsync(sessionCode, new SseEvent(
            SseEventTypes.UserJoined,
            new { user.Id, user.Name, user.IsOwner, user.JoinedAt, user.IsConnected }
        ), excludeUserId: user.Id);

        _logger.LogInformation("User {UserName} joined session {SessionCode} (BrowserId: {BrowserId})",
            userName, sessionCode, browserId ?? "none");

        return user;
    }

    public async Task LeaveSessionAsync(string sessionCode, string userId)
    {
        var session = await GetSessionAsync(sessionCode);
        if (session == null) return;

        if (session.Users.TryRemove(userId, out var user))
        {
            await UpdateActivityAsync(sessionCode);

            // Notify other users
            await _sseService.NotifySessionAsync(sessionCode, new SseEvent(
                SseEventTypes.UserLeft,
                new { UserId = userId, UserName = user.Name }
            ));

            _logger.LogInformation("User {UserName} left session {SessionCode}", user.Name, sessionCode);

            // If the owner left, delete the session
            if (user.IsOwner)
            {
                await DeleteSessionAsync(sessionCode);
            }
        }
    }

    public async Task DeleteSessionAsync(string sessionCode)
    {
        if (_sessions.TryRemove(sessionCode, out var session))
        {
            // Notify all users that session is closed
            await _sseService.NotifySessionAsync(sessionCode, new SseEvent(
                SseEventTypes.SessionClosed,
                new { SessionCode = sessionCode }
            ));

            _logger.LogInformation("Session {SessionCode} deleted", sessionCode);
        }
    }

    public async Task<VotingRound> StartVotingAsync(string sessionCode, string userId)
    {
        var session = await GetSessionAsync(sessionCode);
        if (session == null)
        {
            throw new SessionNotFoundException(sessionCode);
        }

        // Check if user is the owner
        if (session.OwnerId != userId)
        {
            throw new UnauthorizedException("Only the session owner can start voting");
        }

        // Create new voting round
        var now = DateTime.UtcNow;
        var round = new VotingRound
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = sessionCode,
            StartedAt = now,
            Status = VotingRoundStatus.InProgress
        };

        session.CurrentRound = round;
        await UpdateActivityAsync(sessionCode);

        // Notify all users
        await _sseService.NotifySessionAsync(sessionCode, new SseEvent(
            SseEventTypes.VotingStarted,
            new { round.Id, round.StartedAt, round.Status }
        ));

        _logger.LogInformation("Voting started in session {SessionCode}", sessionCode);

        return round;
    }

    public async Task<Vote> SubmitVoteAsync(string sessionCode, string userId, int value)
    {
        var session = await GetSessionAsync(sessionCode);
        if (session == null)
        {
            throw new SessionNotFoundException(sessionCode);
        }

        if (session.CurrentRound == null)
        {
            throw new ValidationException("No active voting round");
        }

        if (session.CurrentRound.Status != VotingRoundStatus.InProgress)
        {
            throw new ValidationException("Voting round is not active");
        }

        if (!FibonacciCalculator.IsValidValue(value))
        {
            throw new ValidationException($"Invalid vote value. Must be one of: 1, 2, 3, 5, 8, 13, 21");
        }

        if (!session.Users.TryGetValue(userId, out var user))
        {
            throw new ValidationException("User not found in session");
        }

        var now = DateTime.UtcNow;
        var vote = new Vote
        {
            UserId = userId,
            UserName = user.Name,
            Value = value,
            SubmittedAt = session.CurrentRound.Votes.ContainsKey(userId)
                ? session.CurrentRound.Votes[userId].SubmittedAt
                : now,
            UpdatedAt = now
        };

        session.CurrentRound.Votes.AddOrUpdate(userId, vote, (key, oldValue) => vote);
        await UpdateActivityAsync(sessionCode);

        // Notify other users (without revealing the vote value)
        await _sseService.NotifySessionAsync(sessionCode, new SseEvent(
            SseEventTypes.VoteSubmitted,
            new { UserId = userId, UserName = user.Name, HasVoted = true }
        ));

        _logger.LogInformation("User {UserName} voted in session {SessionCode}", user.Name, sessionCode);

        return vote;
    }

    public async Task<VotingResults> RevealVotesAsync(string sessionCode, string userId)
    {
        var session = await GetSessionAsync(sessionCode);
        if (session == null)
        {
            throw new SessionNotFoundException(sessionCode);
        }

        if (session.OwnerId != userId)
        {
            throw new UnauthorizedException("Only the session owner can reveal votes");
        }

        if (session.CurrentRound == null)
        {
            throw new ValidationException("No active voting round");
        }

        if (session.CurrentRound.Status == VotingRoundStatus.Revealed)
        {
            throw new ValidationException("Votes already revealed");
        }

        if (session.CurrentRound.Votes.IsEmpty)
        {
            throw new ValidationException("No votes to reveal");
        }

        // Calculate results
        var results = FibonacciCalculator.CalculateResults(session.CurrentRound.Votes.Values);

        session.CurrentRound.Status = VotingRoundStatus.Revealed;
        session.CurrentRound.RevealedAt = DateTime.UtcNow;
        session.CurrentRound.Results = results;

        await UpdateActivityAsync(sessionCode);

        // Notify all users with results
        await _sseService.NotifySessionAsync(sessionCode, new SseEvent(
            SseEventTypes.VotesRevealed,
            new
            {
                Results = results,
                Votes = session.CurrentRound.Votes.Values.Select(v => new
                {
                    v.UserId,
                    v.UserName,
                    v.Value,
                    v.SubmittedAt
                }).ToList()
            }
        ));

        _logger.LogInformation("Votes revealed in session {SessionCode}", sessionCode);

        return results;
    }

    public async Task ResetVotesAsync(string sessionCode, string userId)
    {
        var session = await GetSessionAsync(sessionCode);
        if (session == null)
        {
            throw new SessionNotFoundException(sessionCode);
        }

        if (session.OwnerId != userId)
        {
            throw new UnauthorizedException("Only the session owner can reset votes");
        }

        if (session.CurrentRound != null)
        {
            // Add to history if revealed
            if (session.CurrentRound.Status == VotingRoundStatus.Revealed)
            {
                session.RoundHistory.Add(session.CurrentRound);
            }

            session.CurrentRound = null;
        }

        await UpdateActivityAsync(sessionCode);

        // Notify all users
        await _sseService.NotifySessionAsync(sessionCode, new SseEvent(
            SseEventTypes.VotesReset,
            new { ResetAt = DateTime.UtcNow }
        ));

        _logger.LogInformation("Votes reset in session {SessionCode}", sessionCode);
    }

    public async Task UpdateActivityAsync(string sessionCode)
    {
        if (_sessions.TryGetValue(sessionCode, out var session))
        {
            session.LastActivityAt = DateTime.UtcNow;
        }
        await Task.CompletedTask;
    }

    public async Task<List<string>> GetInactiveSessionsAsync(int inactiveMinutes)
    {
        var now = DateTime.UtcNow;
        var inactiveSessions = _sessions
            .Where(kvp =>
            {
                // Check both LastActivityAt and CreatedAt, use whichever is more recent
                // This ensures sessions live for at least {inactiveMinutes} from either creation or last activity
                var mostRecentTime = kvp.Value.LastActivityAt > kvp.Value.CreatedAt
                    ? kvp.Value.LastActivityAt
                    : kvp.Value.CreatedAt;
                return (now - mostRecentTime).TotalMinutes > inactiveMinutes;
            })
            .Select(kvp => kvp.Key)
            .ToList();

        return await Task.FromResult(inactiveSessions);
    }

    public async Task<List<(string SessionCode, string UserId)>> GetDisconnectedUsersAsync(int disconnectedMinutes)
    {
        var disconnectedUsers = new List<(string SessionCode, string UserId)>();
        var cutoffTime = DateTime.UtcNow.AddMinutes(-disconnectedMinutes);

        foreach (var session in _sessions.Values)
        {
            foreach (var user in session.Users.Values)
            {
                if (!user.IsConnected && user.DisconnectedAt.HasValue && user.DisconnectedAt.Value < cutoffTime)
                {
                    disconnectedUsers.Add((session.Id, user.Id));
                }
            }
        }

        return await Task.FromResult(disconnectedUsers);
    }

    public async Task RemoveUserAsync(string sessionCode, string userId)
    {
        var session = await GetSessionAsync(sessionCode);
        if (session == null) return;

        if (session.Users.TryRemove(userId, out var user))
        {
            // Notify other users
            await _sseService.NotifySessionAsync(sessionCode, new SseEvent(
                SseEventTypes.UserLeft,
                new { UserId = userId, UserName = user.Name, Reason = "disconnected_timeout" }
            ));

            _logger.LogInformation("User {UserName} removed from session {SessionCode} due to disconnection timeout", user.Name, sessionCode);

            // If the owner was removed, delete the session
            if (user.IsOwner)
            {
                await DeleteSessionAsync(sessionCode);
            }
        }
    }

    private string GenerateUniqueSessionCode()
    {
        const int codeLength = 8;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        string code;
        do
        {
            var bytes = new byte[codeLength];
            RandomNumberGenerator.Fill(bytes);
            code = new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        }
        while (_sessions.ContainsKey(code));

        return code;
    }
}
