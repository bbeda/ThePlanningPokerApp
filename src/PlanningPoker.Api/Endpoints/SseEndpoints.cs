using PlanningPoker.Api.Services;

namespace PlanningPoker.Api.Endpoints;

public static class SseEndpoints
{
    public static void MapSseEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/sessions/{sessionCode}/events", async (
            string sessionCode,
            string userId,
            HttpContext context,
            ISseNotificationService sseService,
            ISessionService sessionService,
            CancellationToken cancellationToken) =>
        {
            // Validate session exists
            var session = await sessionService.GetSessionAsync(sessionCode);
            if (session == null)
            {
                return Results.NotFound(new { error = $"Session '{sessionCode}' not found" });
            }

            // Validate user exists in session
            if (!session.Users.ContainsKey(userId))
            {
                return Results.BadRequest(new { error = "User not found in session" });
            }

            // Register the SSE client
            await sseService.RegisterClientAsync(sessionCode, userId, context.Response, cancellationToken);

            return Results.Empty;
        })
        .WithTags("SSE")
        .WithOpenApi()
        .ExcludeFromDescription(); // Don't show in Swagger (SSE is special)
    }
}
