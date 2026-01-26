using System.Collections.Concurrent;

namespace PlanningPoker.Api.Models;

public class VotingRound
{
    public required string Id { get; init; }
    public required string SessionId { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? RevealedAt { get; set; }
    public VotingRoundStatus Status { get; set; }

    public ConcurrentDictionary<string, Vote> Votes { get; init; } = new();

    public VotingResults? Results { get; set; }

    public bool IsRevealed => Status == VotingRoundStatus.Revealed;
    public int VoteCount => Votes.Count;
}
