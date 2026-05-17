using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace UBB_SE_2026_923_2.IntegrationTests
{
    [TestFixture]
    public class PeriodTrackerIntegrationTests
    {
        private PeriodTrackerWebApplicationFactory _factory;
        private HttpClient _client;

        [SetUp]
        public void Setup()
        {
            _factory = new PeriodTrackerWebApplicationFactory();
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false // Crucial for verifying 302 redirect security gates
            });
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [Test]
        public async Task Index_Get_AnonymousUser_RedirectsToLoginSystemGate()
        {
            // Act: Try to request the secure dashboard landing view route without cookies
            var response = await _client.GetAsync("/PeriodTracker");

            // Assert: Must verify the [Authorize] routing policy intercepts the request with an HTTP 302
            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);

            var locationHeader = response.Headers.Location?.ToString() ?? "";
            Assert.IsTrue(locationHeader.Contains("/Login", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public async Task Details_Get_AnonymousUser_RedirectsToLoginSystemGate()
        {
            // Act: Secure report details endpoint audit
            var response = await _client.GetAsync("/PeriodTracker/Details");

            // Assert: Ensure unauthorized access to raw cycle logs is strictly blocked
            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        }

        [Test]
        public async Task Edit_Get_AnonymousUser_RedirectsToLoginSystemGate()
        {
            // Act: Audit access to settings mutation route
            var response = await _client.GetAsync("/PeriodTracker/Edit");

            // Assert: Ensure configuration modification is protected
            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        }

        [Test]
        public async Task Calculate_Post_MissingAntiforgeryToken_BlocksOrRedirectsSecuredPipeline()
        {
            // Arrange: Preparing raw model data parameters without an anti-forgery validation payload
            var badPayload = new Dictionary<string, string>
            {
                { "startPeriodDate", DateTime.Today.ToString("yyyy-MM-dd") },
                { "cycleDays", "28" },
                { "periodLasts", "5" },
                { "pmsOption", "0" }
            };
            var requestContent = new FormUrlEncodedContent(badPayload);

            // Act: Submit data modification attempt directly through the HTTP POST route
            var response = await _client.PostAsync("/PeriodTracker/Create", requestContent);

            // Assert: The ASP.NET Core pipeline must catch the unverified cross-site scripting profile
            bool isThrottledBySecurity = response.StatusCode == HttpStatusCode.BadRequest ||
                                         response.StatusCode == HttpStatusCode.Redirect;

            Assert.IsTrue(isThrottledBySecurity, "The server pipeline must intercept POST requests that lack anti-forgery validation tokens.");
        }

        [Test]
        public async Task CreateNote_Post_MissingAntiforgeryToken_BlocksOrRedirectsSecuredPipeline()
        {
            // Arrange: Attempt to add an unauthorized clinical annotation log item 
            var payload = new Dictionary<string, string> { { "noteBody", "Unauthorized Script Note Injection" } };
            var requestContent = new FormUrlEncodedContent(payload);

            // Act
            var response = await _client.PostAsync("/PeriodTracker/CreateNote", requestContent);

            // Assert
            bool isThrottledBySecurity = response.StatusCode == HttpStatusCode.BadRequest ||
                                         // If the app handles forgery via redirect loops
                                         response.StatusCode == HttpStatusCode.Redirect;

            Assert.IsTrue(isThrottledBySecurity);
        }
    }
}