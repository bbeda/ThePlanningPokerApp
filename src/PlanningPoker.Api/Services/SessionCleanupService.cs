namespace PlanningPoker.Api.Services;

public class SessionCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SessionCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromSeconds(30);
    private readonly int _inactiveSessionMinutes = 10;
    private readonly int _disconnectedUserMinutes = 2;

    public SessionCleanupService(IServiceProvider serviceProvider, ILogger<SessionCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SessionCleanupService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_cleanupInterval, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();

                // Clean up disconnected users (2 minute timeout)
                var disconnectedUsers = await sessionService.GetDisconnectedUsersAsync(_disconnectedUserMinutes);

                if (disconnectedUsers.Any())
                {
                    _logger.LogInformation("Found {Count} disconnected users to remove", disconnectedUsers.Count);

                    foreach (var (sessionCode, userId) in disconnectedUsers)
                    {
                        try
                        {
                            await sessionService.RemoveUserAsync(sessionCode, userId);
                            _logger.LogInformation("Removed disconnected user {UserId} from session {SessionCode}", userId, sessionCode);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error removing disconnected user {UserId} from session {SessionCode}", userId, sessionCode);
                        }
                    }
                }

                // Clean up inactive sessions (10 minute timeout)
                var inactiveSessions = await sessionService.GetInactiveSessionsAsync(_inactiveSessionMinutes);

                if (inactiveSessions.Any())
                {
                    _logger.LogInformation("Found {Count} inactive sessions to clean up", inactiveSessions.Count);

                    foreach (var sessionCode in inactiveSessions)
                    {
                        try
                        {
                            await sessionService.DeleteSessionAsync(sessionCode);
                            _logger.LogInformation("Cleaned up inactive session: {SessionCode}", sessionCode);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error cleaning up session {SessionCode}", sessionCode);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during session cleanup");
            }
        }

        _logger.LogInformation("SessionCleanupService stopped");
    }
}
