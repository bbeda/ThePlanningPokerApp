using PlanningPoker.Api.Endpoints;
using PlanningPoker.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults (Aspire) - commented out for now as ServiceDefaults needs implementation
// builder.AddServiceDefaults();

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register application services
builder.Services.AddSingleton<ISessionService, SessionService>();
builder.Services.AddSingleton<ISseNotificationService, SseNotificationService>();
builder.Services.AddHostedService<SessionCleanupService>();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseStaticFiles(); // Serve Vue build output from wwwroot

// Map API endpoints
app.MapSessionEndpoints();
app.MapUserEndpoints();
app.MapVotingEndpoints();
app.MapSseEndpoints();

// Map health checks
app.MapHealthChecks("/health");

// Fallback to index.html for SPA routing
app.MapFallbackToFile("index.html");

app.Run();
