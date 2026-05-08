namespace UBB_SE_2026_923_2.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using UBB_SE_2026_923_2.Configuration;

/// <summary>
/// Lets the EF Core CLI tooling (<c>dotnet ef migrations add</c>,
/// <c>dotnet ef database update</c>) build an <see cref="AppDbContext"/>
/// without booting the WinUI host. The connection string is taken from
/// <see cref="AppSettings.ConnectionString"/>, which falls back to a
/// hard-coded value if <c>appsettings.json</c> is unavailable at design time.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] arguments)
    {
        var databaseContextOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(AppSettings.ConnectionString);

        return new AppDbContext(databaseContextOptionsBuilder.Options);
    }
}