namespace PlanningPoker.Api.DTOs;

public record UserResponse(
    string Id,
    string Name,
    bool IsOwner,
    DateTime JoinedAt,
    bool IsConnected
);
