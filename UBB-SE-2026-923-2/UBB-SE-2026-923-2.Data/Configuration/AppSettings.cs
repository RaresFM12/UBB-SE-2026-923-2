using System;
using Microsoft.Extensions.Configuration;

namespace UBB_SE_2026_923_2.Configuration;

/// <summary>
/// Central configuration access for the app.
/// <para>
/// The connection string is loaded from <c>appsettings.json</c> (key
/// <c>ConnectionStrings:AppDatabase</c>) at first use. If the file is
/// missing or unreadable, the hard-coded fallback below is used so the
/// design-time EF Core tooling and local debug runs always have a value.
/// </para>
/// </summary>
public static class AppSettings
{
    private const string ConnectionStringKey = "AppDatabase";

    private const string FallbackConnectionString =
        @"Data Source=localhost;Initial Catalog=HospitalDatabase;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";

    private static readonly Lazy<IConfigurationRoot> ConfigurationLazy = new(BuildConfiguration);

    private static readonly Lazy<string> ConnectionStringLazy = new(LoadConnectionString);

    public static IConfigurationRoot Configuration => ConfigurationLazy.Value;

    public static string ConnectionString => ConnectionStringLazy.Value;

    public static readonly DateTime SqlMinimumDate = new DateTime(1753, 1, 1);

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

        return builder.Build();
    }

    private static string LoadConnectionString()
    {
        try
        {
            var fromConfig = Configuration.GetConnectionString(ConnectionStringKey);
            if (!string.IsNullOrWhiteSpace(fromConfig))
            {
                return fromConfig;
            }
        }
        catch
        {
            // Swallow: fall through to fallback so design-time tools never crash on config.
        }

        return FallbackConnectionString;
    }
}
