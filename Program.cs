using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;  // Add this for forwarded headers
using Microsoft.Extensions.Hosting;
using System;

namespace Imbali
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add session services
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
                options.Cookie.HttpOnly = true; // Secure cookie
                options.Cookie.IsEssential = true; // Required for session to work without user consent
            });

            // Configure forwarded headers to support HTTPS redirection behind Azure proxy
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            var app = builder.Build();

            // Use forwarded headers middleware before anything that depends on the headers
            app.UseForwardedHeaders();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Enable session middleware
            app.UseSession();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Configure the app to listen on the port Azure assigns (from environment variable "PORT")
            var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
            app.Urls.Clear();
            app.Urls.Add($"http://*:{port}");

            app.Run();
        }
    }
}
