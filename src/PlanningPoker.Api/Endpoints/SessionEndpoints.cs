using PlanningPoker.Api.DTOs;
using PlanningPoker.Api.Services;

namespace PlanningPoker.Api.Endpoints;

public static class SessionEndpoints
{
    public static void MapSessionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/sessions").WithTags("Sessions").WithOpenApi();

        group.MapPost("/", async (CreateSessionRequest request, ISessionService service) =>
        {
            var session = await service.CreateSessionAsync(request.OwnerName, request.BrowserId);

            var response = new SessionResponse(
                session.Id,
                session.OwnerId,
                session.Owner?.Name ?? "",
                session.Users.Values.Select(u => new UserResponse(u.Id, u.Name, u.IsOwner, u.JoinedAt, u.IsConnected)).ToList(),
                null,
                session.CreatedAt
            );

            return Results.Created($"/api/sessions/{session.Id}", response);
        });

        group.MapGet("/{code}", async (string code, ISessionService service) =>
        {
            var session = await service.GetSessionAsync(code);

            if (session == null)
            {
                return Results.NotFound(new { error = $"Session '{code}' not found" });
            }

            VotingRoundResponse? currentRoundResponse = null;
            if (session.CurrentRound != null)
            {
                var isRevealed = session.CurrentRound.IsRevealed;
                currentRoundResponse = new VotingRoundResponse(
                    session.CurrentRound.Id,
                    session.CurrentRound.Status.ToString(),
                    session.CurrentRound.StartedAt,
                    session.CurrentRound.RevealedAt,
                    session.CurrentRound.Votes.Values.Select(v => new VoteResponse(
                        v.UserId,
                        v.UserName,
                        isRevealed ? v.Value : null,
                        v.SubmittedAt
                    )).ToList(),
                    session.CurrentRound.Results != null ? new VotingResultsResponse(
                        session.CurrentRound.Results.ActualAverage,
                        session.CurrentRound.Results.Majority,
                        session.CurrentRound.Results.Optimistic,
                        session.CurrentRound.Results.Pessimistic,
                        session.CurrentRound.Results.Distribution,
                        session.CurrentRound.Results.MinVote,
                        session.CurrentRound.Results.MaxVote,
                        session.CurrentRound.Results.TotalVotes
                    ) : null
                );
            }

            var response = new SessionResponse(
                session.Id,
                session.OwnerId,
                session.Owner?.Name ?? "",
                session.Users.Values.Select(u => new UserResponse(u.Id, u.Name, u.IsOwner, u.JoinedAt, u.IsConnected)).ToList(),
                currentRoundResponse,
                session.CreatedAt
            );

            return Results.Ok(response);
        });

        group.MapDelete("/{code}", async (string code, string userId, ISessionService service) =>
        {
            var session = await service.GetSessionAsync(code);

            if (session == null)
            {
                return Results.NotFound(new { error = $"Session '{code}' not found" });
            }

            if (session.OwnerId != userId)
            {
                return Results.Forbid();
            }

            await service.DeleteSessionAsync(code);
            return Results.NoContent();
        });
    }
}
