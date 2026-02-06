using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using PlanningPoker.Api.DTOs;

namespace PlanningPoker.Api.Services;

public class SseNotificationService : ISseNotificationService
{
    private record SseClient(string UserId, StreamWriter Writer);

    // sessionCode → connectionId → SseClient
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, SseClient>> _connections = new();
    private readonly ILogger<SseNotificationService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public SseNotificationService(ILogger<SseNotificationService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task RegisterClientAsync(string sessionCode, string userId, HttpResponse response, CancellationToken cancellationToken)
    {
        var connectionId = Guid.NewGuid().ToString();

        try
        {
            // Set SSE headers
            response.Headers.Append("Content-Type", "text/event-stream");
            response.Headers.Append("Cache-Control", "no-cache");
            response.Headers.Append("Connection", "keep-alive");
            response.Headers.Append("X-Accel-Buffering", "no");

            await response.Body.FlushAsync(cancellationToken);

            var streamWriter = new StreamWriter(response.Body, Encoding.UTF8, leaveOpen: true);

            // Add connection to the dictionary
            var sessionConnections = _connections.GetOrAdd(sessionCode, _ => new ConcurrentDictionary<string, SseClient>());
            sessionConnections.TryAdd(connectionId, new SseClient(userId, streamWriter));

            _logger.LogInformation("SSE client registered: Session={SessionCode}, User={UserId}, Connection={ConnectionId}",
                sessionCode, userId, connectionId);

            // Update user connection status
            await UpdateUserConnectionStatusAsync(sessionCode, userId, true);

            // Send a comment to keep the connection alive
            await streamWriter.WriteAsync(": connected\n\n");
            await streamWriter.FlushAsync();

            // Keep the connection open until cancelled
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(30000, cancellationToken); // Send keep-alive every 30 seconds
                try
                {
                    await streamWriter.WriteAsync(": keep-alive\n\n");
                    await streamWriter.FlushAsync();
                }
                catch
                {
                    break; // Connection lost
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SSE connection: Session={SessionCode}, User={UserId}, Connection={ConnectionId}",
                sessionCode, userId, connectionId);
        }
        finally
        {
            await UnregisterConnectionAsync(sessionCode, connectionId);
        }
    }

    private async Task UnregisterConnectionAsync(string sessionCode, string connectionId)
    {
        if (_connections.TryGetValue(sessionCode, out var sessionConnections))
        {
            if (sessionConnections.TryRemove(connectionId, out var client))
            {
                try
                {
                    client.Writer.Dispose();
                }
                catch { }

                _logger.LogInformation("SSE client unregistered: Session={SessionCode}, User={UserId}, Connection={ConnectionId}",
                    sessionCode, client.UserId, connectionId);

                // Only mark user as disconnected if they have no remaining connections
                bool hasOtherConnections = sessionConnections.Values.Any(c => c.UserId == client.UserId);
                if (!hasOtherConnections)
                {
                    await UpdateUserConnectionStatusAsync(sessionCode, client.UserId, false);
                }
            }

            // Remove session if no more connections
            if (sessionConnections.IsEmpty)
            {
                _connections.TryRemove(sessionCode, out _);
            }
        }
    }

    public async Task UnregisterClientAsync(string sessionCode, string userId)
    {
        if (_connections.TryGetValue(sessionCode, out var sessionConnections))
        {
            var userConnections = sessionConnections.Where(kvp => kvp.Value.UserId == userId).ToList();
            foreach (var kvp in userConnections)
            {
                await UnregisterConnectionAsync(sessionCode, kvp.Key);
            }
        }
    }

    public bool IsUserConnected(string sessionCode, string userId)
    {
        if (_connections.TryGetValue(sessionCode, out var sessionConnections))
        {
            return sessionConnections.Values.Any(c => c.UserId == userId);
        }
        return false;
    }

    private async Task UpdateUserConnectionStatusAsync(string sessionCode, string userId, bool isConnected)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();

            var session = await sessionService.GetSessionAsync(sessionCode);
            if (session != null && session.Users.TryGetValue(userId, out var user))
            {
                user.IsConnected = isConnected;
                user.DisconnectedAt = isConnected ? null : DateTime.UtcNow;

                // Notify other users about the connection status change
                var eventType = isConnected ? SseEventTypes.UserConnected : SseEventTypes.UserDisconnected;
                await NotifySessionAsync(sessionCode, new SseEvent(
                    eventType,
                    new { userId, user.Name, isConnected }
                ), excludeUserId: userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user connection status: Session={SessionCode}, User={UserId}", sessionCode, userId);
        }
    }

    public async Task NotifySessionAsync(string sessionCode, SseEvent sseEvent, string? excludeUserId = null)
    {
        if (!_connections.TryGetValue(sessionCode, out var sessionConnections))
        {
            return; // No connections for this session
        }

        var sseMessage = FormatSseMessage(sseEvent);
        var tasks = new List<Task>();

        foreach (var kvp in sessionConnections)
        {
            if (excludeUserId != null && kvp.Value.UserId == excludeUserId)
            {
                continue; // Skip excluded user
            }

            tasks.Add(SendMessageAsync(sessionCode, kvp.Key, kvp.Value.Writer, sseMessage));
        }

        await Task.WhenAll(tasks);
    }

    public async Task NotifyUserAsync(string sessionCode, string userId, SseEvent sseEvent)
    {
        if (!_connections.TryGetValue(sessionCode, out var sessionConnections))
        {
            return;
        }

        var sseMessage = FormatSseMessage(sseEvent);
        var tasks = sessionConnections
            .Where(kvp => kvp.Value.UserId == userId)
            .Select(kvp => SendMessageAsync(sessionCode, kvp.Key, kvp.Value.Writer, sseMessage))
            .ToList();

        await Task.WhenAll(tasks);
    }

    private string FormatSseMessage(SseEvent sseEvent)
    {
        var sb = new StringBuilder();

        // Event type
        sb.Append($"event: {sseEvent.Type}\n");

        // Data (JSON serialized)
        var jsonData = JsonSerializer.Serialize(sseEvent.Data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        sb.Append($"data: {jsonData}\n");

        // Optional event ID
        if (sseEvent.EventId != null)
        {
            sb.Append($"id: {sseEvent.EventId}\n");
        }

        // Optional retry
        if (sseEvent.Retry.HasValue)
        {
            sb.Append($"retry: {sseEvent.Retry.Value}\n");
        }

        // End of message
        sb.Append('\n');

        return sb.ToString();
    }

    private async Task SendMessageAsync(string sessionCode, string connectionId, StreamWriter streamWriter, string message)
    {
        try
        {
            await streamWriter.WriteAsync(message);
            await streamWriter.FlushAsync();
            _logger.LogDebug("SSE message sent: Session={SessionCode}, Connection={ConnectionId}", sessionCode, connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send SSE message: Session={SessionCode}, Connection={ConnectionId}", sessionCode, connectionId);
            // Remove the disconnected client
            await UnregisterConnectionAsync(sessionCode, connectionId);
        }
    }
}
