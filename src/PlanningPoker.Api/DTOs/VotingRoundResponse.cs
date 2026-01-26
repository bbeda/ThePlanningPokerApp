namespace PlanningPoker.Api.DTOs;

public record VotingRoundResponse(
    string Id,
    string Status,
    DateTime StartedAt,
    DateTime? RevealedAt,
    List<VoteResponse> Votes,
    VotingResultsResponse? Results
);
