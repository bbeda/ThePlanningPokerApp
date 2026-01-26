namespace PlanningPoker.Api.DTOs;

public record VoteResponse(
    string UserId,
    string UserName,
    int? Value,
    DateTime SubmittedAt
);
