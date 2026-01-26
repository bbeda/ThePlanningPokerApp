namespace PlanningPoker.Api.DTOs;

public record CreateSessionRequest(string OwnerName, string? BrowserId = null);
