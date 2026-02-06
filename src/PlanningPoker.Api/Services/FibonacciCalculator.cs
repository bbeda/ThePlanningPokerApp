using PlanningPoker.Api.Models;

namespace PlanningPoker.Api.Services;

public static class FibonacciCalculator
{
    private static readonly int[] Fibonacci = { 1, 2, 3, 5, 8, 13, 21 };

    public static bool IsValidValue(int value)
    {
        return Array.Exists(Fibonacci, f => f == value);
    }

    public static int CalculateMajority(List<int> votes, double average)
    {
        if (votes.Count == 0) return 1;

        var groups = votes.GroupBy(v => v)
                         .Select(g => new { Value = g.Key, Count = g.Count() })
                         .ToList();

        int maxCount = groups.Max(g => g.Count);
        var modes = groups.Where(g => g.Count == maxCount).Select(g => g.Value).ToList();

        if (modes.Count == 1) return modes[0];

        // Tie-break: pick the mode closest to the average
        return modes.OrderBy(v => Math.Abs(v - average)).First();
    }

    public static int CalculatePercentileFibonacci(List<int> votes, double percentile, bool roundDown)
    {
        if (votes.Count == 0) return 1;

        var sorted = votes.OrderBy(v => v).ToList();
        int n = sorted.Count;

        // Linear interpolation for percentile
        double rank = (percentile / 100.0) * (n - 1);
        int lower = (int)Math.Floor(rank);
        int upper = (int)Math.Ceiling(rank);
        double fraction = rank - lower;

        double value;
        if (lower == upper)
        {
            value = sorted[lower];
        }
        else
        {
            value = sorted[lower] + fraction * (sorted[upper] - sorted[lower]);
        }

        if (roundDown)
        {
            // Optimistic: nearest Fibonacci at or below the percentile value
            return Fibonacci.Where(f => f <= value).DefaultIfEmpty(Fibonacci[0]).Max();
        }
        else
        {
            // Pessimistic: nearest Fibonacci at or above the percentile value
            return Fibonacci.Where(f => f >= value).DefaultIfEmpty(Fibonacci[^1]).Min();
        }
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
                Majority = 1,
                Optimistic = 1,
                Pessimistic = 1,
                ActualAverage = 0,
                Distribution = new Dictionary<int, int>(),
                MinVote = 0,
                MaxVote = 0,
                TotalVotes = 0
            };
        }

        double average = voteValues.Average();
        var distribution = CalculateDistribution(voteValues);

        return new VotingResults
        {
            Majority = CalculateMajority(voteValues, average),
            Optimistic = CalculatePercentileFibonacci(voteValues, 25, roundDown: true),
            Pessimistic = CalculatePercentileFibonacci(voteValues, 75, roundDown: false),
            ActualAverage = average,
            Distribution = distribution,
            MinVote = voteValues.Min(),
            MaxVote = voteValues.Max(),
            TotalVotes = voteValues.Count
        };
    }
}
