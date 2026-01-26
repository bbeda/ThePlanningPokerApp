namespace PlanningPoker.Api.Models;

public class VotingResults
{
    public int LowFibonacciAverage { get; set; }
    public int HighFibonacciAverage { get; set; }
    public double ActualAverage { get; set; }
    public Dictionary<int, int> Distribution { get; set; } = new();
    public int MinVote { get; set; }
    public int MaxVote { get; set; }
    public int TotalVotes { get; set; }
}
