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
/// Boots the <c>.Web</c> MVC app for the Prescriptions controller integration
/// tests. The HTTP-backed repositories the controller depends on are swapped
/// for in-memory EF Core repositories, so the controller -> service ->
/// repository path runs without a live WebApi; cookie auth is replaced with
/// <see cref="TestAuthHandler"/> so the
/// <c>[Authorize(Roles = "Pharmacist,Admin")]</c> gate is satisfied.
/// </summary>
public class PrescriptionsWebApplicationFactory : WebApplicationFactory<UBB_SE_2026_923_2.Web.Program>
{
    private readonly string databaseName = "PrescriptionsWebTestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IItemsRepository>();
            services.RemoveAll<IEvaluationsRepository>();
            services.RemoveAll<IHighRiskMedicineRepository>();
            services.RemoveAll<IDbContextFactory<AppDbContext>>();

            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseInMemoryDatabase(this.databaseName));

            services.AddSingleton<IItemsRepository, SQLItemsRepository>();
            services.AddSingleton<IEvaluationsRepository, EvaluationsRepository>();
            services.AddSingleton<IHighRiskMedicineRepository, HighRiskMedicineRepository>();

            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });
        });
    }
}
