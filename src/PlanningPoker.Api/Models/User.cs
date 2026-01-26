namespace PlanningPoker.Api.Models;

public class User
{
    public required string Id { get; init; }
    public required string SessionId { get; init; }
    public required string Name { get; set; }
    public bool IsOwner { get; set; }
    public DateTime JoinedAt { get; init; }
    public DateTime LastSeenAt { get; set; }
    public string? BrowserId { get; init; }
    public bool IsConnected { get; set; } = false;
    public DateTime? DisconnectedAt { get; set; }
}
