namespace UBB_SE_2026_923_2.Web
{
    using System;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using UBB_SE_2026_923_2.Shared;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC + business logic (services + HTTP-backed repositories) shared with desktop.
            builder.Services.AddControllersWithViews();

            string apiBase = builder.Configuration["WebApiBaseUrl"]
                ?? throw new InvalidOperationException("WebApiBaseUrl not set in configuration.");
            builder.Services.AddBusinessLogic(new Uri(apiBase));

            // Cookie authentication: every controller action is gated with
            // [Authorize] (or [AllowAnonymous] for login/register).
            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login";
                    options.AccessDeniedPath = "/Login/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                });
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Expose the same provider to the Shared business-logic layer so
            // services that still resolve dependencies from the static locator
            // (legacy parameterless constructors) keep working.
            SharedServiceProvider.Services = app.Services;

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
