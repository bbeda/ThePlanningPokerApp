namespace PlanningPoker.Api.DTOs;

public record JoinSessionRequest(string SessionCode, string UserName, string? BrowserId = null);
