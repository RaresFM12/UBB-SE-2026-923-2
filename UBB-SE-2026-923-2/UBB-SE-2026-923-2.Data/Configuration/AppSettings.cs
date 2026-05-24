namespace UBB_SE_2026_923_2.Configuration;

using System;
using Microsoft.Extensions.Configuration;

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
    private const string WebApiBaseUrlKey = "WebApiBaseUrl";
    private const string WebApiAccessKeyKey = "WebApiAccessKey";

    private const string FallbackConnectionString =
        @"Data Source=.\SQLEXPRESS;Initial Catalog=HospitalDatabase;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";

    private const string FallbackWebApiBaseUrl = "https://localhost:5100/";
    private const string FallbackWebApiAccessKey = "UBB-SE-2026-923-2-ApiKey";

    private static readonly Lazy<IConfigurationRoot> ConfigurationLazy = new(BuildConfiguration);

    private static readonly Lazy<string> ConnectionStringLazy = new(LoadConnectionString);

    private static readonly Lazy<string> WebApiBaseUrlLazy = new(LoadWebApiBaseUrl);

    private static readonly Lazy<string> WebApiAccessKeyLazy = new(LoadWebApiAccessKey);

    public static IConfigurationRoot Configuration => ConfigurationLazy.Value;

    public static string ConnectionString => ConnectionStringLazy.Value;

    public static string WebApiBaseUrl => WebApiBaseUrlLazy.Value;

    public static string WebApiAccessKey => WebApiAccessKeyLazy.Value;

    public static readonly DateTime SqlMinimumDate = new DateTime(1753, 1, 1);

    private static IConfigurationRoot BuildConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

        return configurationBuilder.Build();
    }

    private static string LoadConnectionString()
    {
        try
        {
            var retrievedConfigurationValue = Configuration.GetConnectionString(ConnectionStringKey);
            if (!string.IsNullOrWhiteSpace(retrievedConfigurationValue))
            {
                return retrievedConfigurationValue;
            }
        }
        catch
        {
            // Swallow: fall through to fallback so design-time tools never crash on config.
        }

        return FallbackConnectionString;
    }

    private static string LoadWebApiBaseUrl()
    {
        try
        {
            var retrievedConfigurationValue = Configuration[WebApiBaseUrlKey];
            if (!string.IsNullOrWhiteSpace(retrievedConfigurationValue))
            {
                return retrievedConfigurationValue;
            }
        }
        catch
        {
            // Swallow: fall through to fallback so design-time tools never crash on config.
        }

        return FallbackWebApiBaseUrl;
    }

    private static string LoadWebApiAccessKey()
    {
        try
        {
            var retrievedConfigurationValue = Configuration[WebApiAccessKeyKey];
            if (!string.IsNullOrWhiteSpace(retrievedConfigurationValue))
            {
                return retrievedConfigurationValue;
            }
        }
        catch
        {
            // Swallow: fall through to fallback so design-time tools never crash on config.
        }

        return FallbackWebApiAccessKey;
    }
}