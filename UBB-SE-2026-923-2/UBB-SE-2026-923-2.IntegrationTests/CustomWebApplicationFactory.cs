using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UBB_SE_2026_923_2.Data;

namespace UBB_SE_2026_923_2.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = "IntegrationTestDb_" + Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                         || d.ServiceType == typeof(IDbContextFactory<AppDbContext>))
                .ToList();

            foreach (var descriptorToRemove in descriptorsToRemove)
            {
                services.Remove(descriptorToRemove);
            }

            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseInMemoryDatabase(this.databaseName));
        });

        builder.UseEnvironment("Development");
    }
}
