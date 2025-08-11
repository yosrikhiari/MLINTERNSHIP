using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using FrontEndForecasting.Services;

namespace FrontEndForecasting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Only load configuration from appsettings.json (and environment-specific) with no other fallbacks
                builder.Configuration.Sources.Clear();
                builder.Configuration
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

                // Configure services
                builder.Services.AddControllersWithViews(options =>
                {
                    // Add global anti-forgery validation
                    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                });

                // Add caching services (Redis-backed)
                builder.Services.AddMemoryCache(); // optional local fallback for other features

                // Add export services
                builder.Services.AddScoped<IExportService, ExportService>();

                // Add performance monitoring services
                builder.Services.AddScoped<IPerformanceMonitoringService, PerformanceMonitoringService>();

                // Add health checks
                builder.Services.AddHealthChecks();

                builder.Services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(30);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                });

                // Configure file upload limits
                builder.Services.Configure<FormOptions>(options =>
                {
                    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
                    options.ValueLengthLimit = int.MaxValue;
                    options.ValueCountLimit = int.MaxValue;
                });

                var redisConfig = builder.Configuration["Redis:Configuration"]
                                ?? builder.Configuration["REDIS__CONFIGURATION"]
                                ?? "redis:6379,abortConnect=false";
                builder.Services.AddStackExchangeRedisCache(o =>
                {
                    o.Configuration = redisConfig;
                    o.InstanceName = "forecast:";
                });

                builder.Services.AddScoped<ICacheService, RedisCacheService>();

                // Configure Kestrel server limits
                builder.Services.Configure<KestrelServerOptions>(options =>
                {
                    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
                });

                var app = builder.Build();

                // Configure middleware pipeline
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Home/Error");
                    app.UseHsts();
                }
                else
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseRouting();
                app.UseSession();
                app.UseAuthorization();

                // Add health checks endpoint
                app.MapHealthChecks("/health");

                // Configure routes
                app.MapControllerRoute(
                    name: "forecast",
                    pattern: "Forecast/{action=Index}/{id?}",
                    defaults: new { controller = "Forecast" });

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=forecast}/{action=Index}/{id?}");

                app.Run();
            }
            catch (Exception ex)
            {
                // Your existing error handling is perfect
                Console.WriteLine($"Critical error during application startup:");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }
    }
}