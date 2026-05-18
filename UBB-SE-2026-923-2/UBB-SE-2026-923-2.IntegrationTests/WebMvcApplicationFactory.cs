using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.IntegrationTests;

public class WebMvcApplicationFactory : WebApplicationFactory<UBB_SE_2026_923_2.Web.Program>
{
    private const string WebApiBaseUrl = "https://localhost:7100/";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WebApiBaseUrl"] = WebApiBaseUrl,
            });
        });

        builder.ConfigureServices(services =>
        {
            RemoveService<IAdminService>(services);
            RemoveService<IHangoutService>(services);
            RemoveService<IDoctorAppointmentService>(services);

            services.AddSingleton<IAdminService, FakeAdminService>();
            services.AddSingleton<IHangoutService, FakeHangoutService>();
            services.AddSingleton<IDoctorAppointmentService, FakeDoctorAppointmentService>();

            services.AddAuthentication(defaultScheme: "Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });

        builder.UseEnvironment("Development");
    }

    public static string ExtractAntiForgeryToken(string htmlContent)
    {
        Match match = Regex.Match(
            htmlContent,
            @"<input[^>]+name=""__RequestVerificationToken""[^>]+value=""([^""]+)""");
        return match.Groups[1].Value;
    }

    private static void RemoveService<TService>(IServiceCollection services)
    {
        ServiceDescriptor? descriptor = services.FirstOrDefault(serviceDescriptor => serviceDescriptor.ServiceType == typeof(TService));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }
}
