namespace UBB_SE_2026_923_2.IntegrationTests.Fakes
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Http;

    public sealed class FakeAntiforgery : IAntiforgery
    {
        private static readonly AntiforgeryTokenSet Tokens = new(
            "fake-request-token",
            "fake-cookie-token",
            "__RequestVerificationToken",
            "__RequestVerificationToken");

        public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext)
        {
            return Tokens;
        }

        public AntiforgeryTokenSet GetTokens(HttpContext httpContext)
        {
            return Tokens;
        }

        public Task<bool> IsRequestValidAsync(HttpContext httpContext)
        {
            return Task.FromResult(true);
        }

        public Task ValidateRequestAsync(HttpContext httpContext)
        {
            return Task.CompletedTask;
        }

        public void SetCookieTokenAndHeader(HttpContext httpContext)
        {
        }
    }
}
