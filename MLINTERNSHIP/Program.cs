using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var app = builder.Build();

// Add Prometheus metrics endpoint
app.UseMetricServer();
app.UseHttpMetrics();

// Health endpoint
app.MapHealthChecks("/health");

// Root endpoint supports GET and HEAD (for wget --spider health checks)
app.MapMethods("/", new[] { "GET", "HEAD" }, () => Results.Ok("MLINTERNSHIP model service is running"));

app.Run();