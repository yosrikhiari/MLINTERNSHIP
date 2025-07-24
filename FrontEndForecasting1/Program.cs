using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace FrontEndForecasting1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Configure services
                builder.Services.AddControllersWithViews(options =>
                {
                    // Add global anti-forgery validation
                    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                });

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

                // Configure routes
                app.MapControllerRoute(
                    name: "forecast",
                    pattern: "Forecast/{action=Index}/{id?}",
                    defaults: new { controller = "Forecast" });

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

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