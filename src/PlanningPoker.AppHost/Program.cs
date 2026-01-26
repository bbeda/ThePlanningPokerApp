using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("planningpoker-env");

var api = builder.AddProject<Projects.PlanningPoker_Api>("planningpoker-api")
    .WithExternalHttpEndpoints();

// Development only: Run frontend dev server
// For production, frontend is built and served from API's wwwroot
if (builder.Environment.IsDevelopment())
{
    builder.AddNpmApp("planningpoker-web", "../PlanningPoker.Web", "dev")
        .WithReference(api)
        .WaitFor(api)
        .WithHttpEndpoint(env: "PORT")
        .WithExternalHttpEndpoints()
        .WithEnvironment("VITE_API_URL", api.GetEndpoint("http"));
}

builder.Build().Run();
