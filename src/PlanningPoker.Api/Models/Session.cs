using System.Collections.Concurrent;

namespace PlanningPoker.Api.Models;

public class Session
{
    public required string Id { get; init; }
    public required string OwnerId { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime LastActivityAt { get; set; }

    public ConcurrentDictionary<string, User> Users { get; init; } = new();
    public VotingRound? CurrentRound { get; set; }
    public List<VotingRound> RoundHistory { get; init; } = new();

    public bool IsActive
    {
        get
        {
            var mostRecentTime = LastActivityAt > CreatedAt ? LastActivityAt : CreatedAt;
            return (DateTime.UtcNow - mostRecentTime).TotalMinutes <= 10;
        }
    }
    public User? Owner => Users.TryGetValue(OwnerId, out var owner) ? owner : null;
}
