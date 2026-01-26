using PlanningPoker.Api.Models;

namespace PlanningPoker.Api.Services;

public static class FibonacciCalculator
{
    private static readonly int[] Fibonacci = { 1, 2, 3, 5, 8, 13, 21 };

    public static bool IsValidValue(int value)
    {
        return Array.Exists(Fibonacci, f => f == value);
    }

    public static (int low, int high) CalculateAverages(IEnumerable<int> votes)
    {
        var voteList = votes.ToList();
        if (voteList.Count == 0) return (1, 1);

        double average = voteList.Average();

        int low = Fibonacci.Where(f => f <= average).DefaultIfEmpty(1).Max();
        int high = Fibonacci.Where(f => f >= average).DefaultIfEmpty(21).Min();

        return (low, high);
    }

    public static Dictionary<int, int> CalculateDistribution(IEnumerable<int> votes)
    {
        return votes.GroupBy(v => v)
                   .ToDictionary(g => g.Key, g => g.Count());
    }

    public static VotingResults CalculateResults(IEnumerable<Vote> votes)
    {
        var voteValues = votes.Select(v => v.Value).ToList();

        if (voteValues.Count == 0)
        {
            return new VotingResults
            {
                LowFibonacciAverage = 1,
                HighFibonacciAverage = 1,
                ActualAverage = 0,
                Distribution = new Dictionary<int, int>(),
                MinVote = 0,
                MaxVote = 0,
                TotalVotes = 0
            };
        }

        var (low, high) = CalculateAverages(voteValues);
        var distribution = CalculateDistribution(voteValues);

        return new VotingResults
        {
            LowFibonacciAverage = low,
            HighFibonacciAverage = high,
            ActualAverage = voteValues.Average(),
            Distribution = distribution,
            MinVote = voteValues.Min(),
            MaxVote = voteValues.Max(),
            TotalVotes = voteValues.Count
        };
    }
}
