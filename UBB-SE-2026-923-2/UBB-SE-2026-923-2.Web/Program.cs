namespace UBB_SE_2026_923_2.Web
{
    using System;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using UBB_SE_2026_923_2.Shared;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Web.ViewModels;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC + business logic (services + HTTP-backed repositories) shared with desktop.
            builder.Services.AddControllersWithViews();

            string apiBase = builder.Configuration["WebApiBaseUrl"]
                ?? throw new InvalidOperationException("WebApiBaseUrl not set in configuration.");
            string apiKey = builder.Configuration["WebApiAccessKey"]
                ?? throw new InvalidOperationException("WebApiAccessKey not set in configuration.");
            builder.Services.AddBusinessLogic(new Uri(apiBase), apiKey);

            // Cookie authentication and a fallback policy so all MVC endpoints
            // require authentication unless explicitly marked [AllowAnonymous].
            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login";
                    options.AccessDeniedPath = "/Login/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                });
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            var app = builder.Build();


            
            // -----------------------------------------------------------

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

            // --- UPDATE THIS INLINE LAMBDA MIDDLEWARE IN PROGRAM.CS ---
            app.Use(async (context, next) =>
            {
                // 1. Rebuild the shared singleton tracking instances natively
                ServiceWrapper.Initialize();

                // 2. If the user is authenticated via cookie, restore their context into the legacy engine
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    var nameIdentifierClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(nameIdentifierClaim, out int loggedInUserId))
                    {
                        // Pull the user repository directly from the active request container
                        var userRepository = context.RequestServices.GetRequiredService<Repositories.IUsersRepository>();
                        var databaseUserRecord = userRepository.GetUserById(loggedInUserId);

                        if (databaseUserRecord != null)
                        {
                            BasketStore.Restore(databaseUserRecord);

                            // Inject the active identity context into the private property storage
                            var serviceInstance = ServiceWrapper.UserAccountService;
                            var currentUserProperty = typeof(UserAccountService)
                                .GetProperty("CurrentUser", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                            currentUserProperty?.SetValue(serviceInstance, databaseUserRecord);
                        }
                    }
                }
                await next();
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
