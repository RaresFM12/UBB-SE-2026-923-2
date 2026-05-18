using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UBB_SE_2026_923_2.IntegrationTests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string TestSchemeName = "Test";
    private const string TestUserIdentifier = "1";
    private const string TestUserName = "Test User";
    private const string TestUserEmail = "test@test.com";
    private const string AdminRoleName = "Admin";
    private const string DoctorRoleName = "Doctor";

    public TestAuthHandler(
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

        ClaimsIdentity identity = new ClaimsIdentity(claims, TestSchemeName);
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);
        AuthenticationTicket ticket = new AuthenticationTicket(principal, TestSchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
