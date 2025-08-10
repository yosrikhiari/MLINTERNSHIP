using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var app = builder.Build();

// Health endpoint
app.MapHealthChecks("/health");

// Root endpoint supports GET and HEAD (for wget --spider health checks)
app.MapMethods("/", new[] { "GET", "HEAD" }, () => Results.Ok("MLINTERNSHIP model service is running"));

// Minimal Prometheus-compatible metrics endpoint
app.MapGet("/metrics", () => Results.Text(
    "# HELP app_up 1\n# TYPE app_up gauge\napp_up 1\n",
    "text/plain"));

app.Run();