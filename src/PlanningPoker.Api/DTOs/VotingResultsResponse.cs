namespace PlanningPoker.Api.DTOs;

public record VotingResultsResponse(
    int LowFibonacciAverage,
    int HighFibonacciAverage,
    double ActualAverage,
    Dictionary<int, int> Distribution,
    int MinVote,
    int MaxVote,
    int TotalVotes
);
