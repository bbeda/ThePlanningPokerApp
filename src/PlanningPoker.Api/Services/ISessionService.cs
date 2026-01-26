using PlanningPoker.Api.Models;

namespace PlanningPoker.Api.Services;

public interface ISessionService
{
    // Session management
    Task<Session> CreateSessionAsync(string ownerName, string? browserId = null);
    Task<Session?> GetSessionAsync(string sessionCode);
    Task<User> JoinSessionAsync(string sessionCode, string userName, string? browserId = null);
    Task LeaveSessionAsync(string sessionCode, string userId);
    Task DeleteSessionAsync(string sessionCode);

    // Voting operations
    Task<VotingRound> StartVotingAsync(string sessionCode, string userId);
    Task<Vote> SubmitVoteAsync(string sessionCode, string userId, int value);
    Task<VotingResults> RevealVotesAsync(string sessionCode, string userId);
    Task ResetVotesAsync(string sessionCode, string userId);

    // Utilities
    Task UpdateActivityAsync(string sessionCode);
    Task<List<string>> GetInactiveSessionsAsync(int inactiveMinutes);
    Task<List<(string SessionCode, string UserId)>> GetDisconnectedUsersAsync(int disconnectedMinutes);
    Task RemoveUserAsync(string sessionCode, string userId);
}
