namespace PlanningPoker.Api.DTOs;

public record SessionResponse(
    string SessionCode,
    string OwnerId,
    string OwnerName,
    List<UserResponse> Users,
    VotingRoundResponse? CurrentRound,
    DateTime CreatedAt
);
