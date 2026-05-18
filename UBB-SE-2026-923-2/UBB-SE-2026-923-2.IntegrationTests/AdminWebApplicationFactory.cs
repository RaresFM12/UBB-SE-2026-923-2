using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.IntegrationTests;

public class AdminWebApplicationFactory : WebApplicationFactory<UBB_SE_2026_923_2.Web.Program>
{
    private const string WebApiBaseUrl = "https://localhost:7100/";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        void AddTestConfiguration(WebHostBuilderContext context, IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WebApiBaseUrl"] = WebApiBaseUrl,
            });
        }

        void RegisterTestServices(IServiceCollection services)
        {
            RemoveService<IAdminService>(services);

            services.AddSingleton<IAdminService, FakeAdminService>();

            services.AddAuthentication(defaultScheme: MvcTestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, MvcTestAuthHandler>(MvcTestAuthHandler.SchemeName, options => { });
        }

        builder.ConfigureAppConfiguration(AddTestConfiguration);
        builder.ConfigureServices(RegisterTestServices);
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

public class MvcTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "MvcTest";

    private const string TestUserIdentifier = "1";
    private const string TestUserName = "Test User";
    private const string TestUserEmail = "test@test.com";
    private const string AdminRoleName = "Admin";
    private const string DoctorRoleName = "Doctor";

    public MvcTestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Claim[] claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserIdentifier),
            new Claim(ClaimTypes.Name, TestUserName),
            new Claim(ClaimTypes.Email, TestUserEmail),
            new Claim(ClaimTypes.Role, AdminRoleName),
            new Claim(ClaimTypes.Role, DoctorRoleName),
        };

        ClaimsIdentity identity = new ClaimsIdentity(claims, SchemeName);
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);
        AuthenticationTicket ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class FakeAdminService : IAdminService
{
    public List<Item> GetAllItems() => new List<Item>();

    public List<Substance> GetAllSubstances() => new List<Substance>();

    public List<Item> SearchItemsByName(string query) => new List<Item>();

    public Item GetItemById(int itemId) => null!;

    public Substance GetSubstanceByName(string name) => null!;

    public bool SubstanceExists(string name) => false;

    public void AddItem(Item newItem) { }

    public void AddItemWithQuantity(Item newItem) { }

    public void RemoveItemById(int itemId) { }

    public void UpdateItemById(int itemId, Item updatedItem) { }

    public void AddSubstance(Substance newSubstance) { }

    public void RemoveSubstanceByName(Substance substance) { }

    public void UpdateSubstanceByName(string name, Substance substance) { }

    public void ValidateItemForAdd(Item item) { }

    public List<Item> GetExpiredItems() => new List<Item>();

    public Notification SendNewStockNotification(Item item) => throw new NotImplementedException();

    public Notification SendAboutToExpireNotification() => throw new NotImplementedException();

    public List<Notification> GetNotificationsForUser(User user) => throw new NotImplementedException();

    public List<Tuple<int, string, int>> GetTop30Items() => new List<Tuple<int, string, int>>();

    public Dictionary<string, int> GetTop30Substances() => new Dictionary<string, int>();
}
