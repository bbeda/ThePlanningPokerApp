using PlanningPoker.Api.DTOs;
using PlanningPoker.Api.Services;

namespace PlanningPoker.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/sessions/{sessionCode}/users").WithTags("Users").WithOpenApi();

        group.MapPost("/", async (string sessionCode, JoinSessionRequest request, ISessionService service) =>
        {
            try
            {
                var user = await service.JoinSessionAsync(sessionCode, request.UserName, request.BrowserId);
                var response = new UserResponse(user.Id, user.Name, user.IsOwner, user.JoinedAt, user.IsConnected);
                return Results.Created($"/api/sessions/{sessionCode}/users/{user.Id}", response);
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

        group.MapDelete("/{userId}", async (string sessionCode, string userId, ISessionService service) =>
        {
            await service.LeaveSessionAsync(sessionCode, userId);
            return Results.NoContent();
        });
    }
}
