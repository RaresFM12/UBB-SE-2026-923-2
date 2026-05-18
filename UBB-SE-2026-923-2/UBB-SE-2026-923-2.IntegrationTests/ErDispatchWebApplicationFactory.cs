using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UBB_SE_2026_923_2.Data;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.IntegrationTests;

/// <summary>
/// Boots the <c>.Web</c> MVC app for controller integration tests.
/// Two production seams are replaced:
/// <list type="bullet">
/// <item>the HTTP-backed <see cref="IERDispatchRepository"/> is swapped for an
/// in-memory EF Core repository, so the controller -> service -> repository
/// path runs without a live WebApi;</item>
/// <item>cookie auth is replaced with <see cref="TestAuthHandler"/> so the
/// <c>[Authorize(Roles = "Admin")]</c> gate is satisfied.</item>
/// </list>
/// </summary>
public class ErDispatchWebApplicationFactory : WebApplicationFactory<UBB_SE_2026_923_2.Web.Program>
{
    private readonly string databaseName = "ErDispatchWebTestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IERDispatchRepository>();
            services.RemoveAll<IDbContextFactory<AppDbContext>>();

            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseInMemoryDatabase(this.databaseName));
            services.AddSingleton<IERDispatchRepository, ERDispatchRepository>();

            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });
        });
    }
}
