namespace PlanningPoker.Api.DTOs;

public record VotingResultsResponse(
    double ActualAverage,
    int Majority,
    int Optimistic,
    int Pessimistic,
    Dictionary<int, int> Distribution,
    int MinVote,
    int MaxVote,
    int TotalVotes
);
