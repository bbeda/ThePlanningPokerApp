namespace PlanningPoker.Api.Models;

public class Vote
{
    public required string UserId { get; init; }
    public required string UserName { get; set; }
    public int Value { get; set; }
    public DateTime SubmittedAt { get; init; }
    public DateTime? UpdatedAt { get; set; }
}
