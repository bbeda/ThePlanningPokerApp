using PlanningPoker.Api.DTOs;
using PlanningPoker.Api.Services;

namespace PlanningPoker.Api.Endpoints;

public static class VotingEndpoints
{
    public static void MapVotingEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/sessions/{sessionCode}/voting").WithTags("Voting").WithOpenApi();

        group.MapPost("/start", async (string sessionCode, string userId, ISessionService service) =>
        {
            try
            {
                var round = await service.StartVotingAsync(sessionCode, userId);
                var response = new VotingRoundResponse(
                    round.Id,
                    round.Status.ToString(),
                    round.StartedAt,
                    round.RevealedAt,
                    new List<VoteResponse>(),
                    null
                );
                return Results.Ok(response);
            }
            catch (SessionNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Results.Forbid();
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPost("/votes", async (string sessionCode, string userId, SubmitVoteRequest request, ISessionService service) =>
        {
            try
            {
                var vote = await service.SubmitVoteAsync(sessionCode, userId, request.Value);
                var response = new VoteResponse(vote.UserId, vote.UserName, vote.Value, vote.SubmittedAt);
                return Results.Ok(response);
            }
            catch (SessionNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPost("/reveal", async (string sessionCode, string userId, ISessionService service) =>
        {
            try
            {
                var results = await service.RevealVotesAsync(sessionCode, userId);
                var response = new VotingResultsResponse(
                    results.ActualAverage,
                    results.Majority,
                    results.Optimistic,
                    results.Pessimistic,
                    results.Distribution,
                    results.MinVote,
                    results.MaxVote,
                    results.TotalVotes
                );
                return Results.Ok(response);
            }
            catch (SessionNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Results.Forbid();
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPost("/reset", async (string sessionCode, string userId, ISessionService service) =>
        {
            try
            {
                await service.ResetVotesAsync(sessionCode, userId);
                return Results.NoContent();
            }
            catch (SessionNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Results.Forbid();
            }
        });
    }
}
