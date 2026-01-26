using PlanningPoker.Api.DTOs;

namespace PlanningPoker.Api.Services;

public interface ISseNotificationService
{
    Task RegisterClientAsync(string sessionCode, string userId, HttpResponse response, CancellationToken cancellationToken);
    Task UnregisterClientAsync(string sessionCode, string userId);
    Task NotifySessionAsync(string sessionCode, SseEvent sseEvent, string? excludeUserId = null);
    Task NotifyUserAsync(string sessionCode, string userId, SseEvent sseEvent);
    bool IsUserConnected(string sessionCode, string userId);
}
