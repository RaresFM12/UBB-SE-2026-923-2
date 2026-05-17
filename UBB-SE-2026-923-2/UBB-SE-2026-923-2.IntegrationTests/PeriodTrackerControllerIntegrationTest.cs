using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.IntegrationTests
{
    [TestFixture]
    public class PeriodTrackerIntegrationTests
    {
        private WebApplicationFactory<UBB_SE_2026_923_2.Web.Program> _anonymousFactory;
        private PeriodTrackerWebApplicationFactory _authenticatedFactory;
        private HttpClient _anonymousClient;
        private HttpClient _authenticatedClient;

        [SetUp]
        public void Setup()
        {
            _anonymousFactory = new WebApplicationFactory<UBB_SE_2026_923_2.Web.Program>();
            _anonymousClient = _anonymousFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false 
            });

            _authenticatedFactory = new PeriodTrackerWebApplicationFactory();
            _authenticatedClient = _authenticatedFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [TearDown]
        public void TearDown()
        {
            _anonymousClient?.Dispose();
            _anonymousFactory?.Dispose();
            _authenticatedClient?.Dispose();
            _authenticatedFactory?.Dispose();
        }

        [Test]
        public async Task Index_Get_AnonymousUser_RedirectsToLoginSystemGate()
        {
            var response = await _anonymousClient.GetAsync("/PeriodTracker");

            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);

            var locationHeader = response.Headers.Location?.ToString() ?? "";
            Assert.IsTrue(locationHeader.Contains("/Login", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public async Task Details_Get_AnonymousUser_RedirectsToLoginSystemGate()
        {
            var response = await _anonymousClient.GetAsync("/PeriodTracker/Details");

            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        }

        [Test]
        public async Task Edit_Get_AnonymousUser_RedirectsToLoginSystemGate()
        {
            var response = await _anonymousClient.GetAsync("/PeriodTracker/Edit");

            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        }

        [Test]
        public async Task Calculate_Post_MissingAntiforgeryToken_BlocksOrRedirectsSecuredPipeline()
        {
            var badPayload = new Dictionary<string, string>
            {
                { "startPeriodDate", DateTime.Today.ToString("yyyy-MM-dd") },
                { "cycleDays", "28" },
                { "periodLasts", "5" },
                { "pmsOption", "0" }
            };
            var requestContent = new FormUrlEncodedContent(badPayload);

            var response = await _anonymousClient.PostAsync("/PeriodTracker/Create", requestContent);

            bool isThrottledBySecurity = response.StatusCode == HttpStatusCode.BadRequest ||
                                         response.StatusCode == HttpStatusCode.Redirect;

            Assert.IsTrue(isThrottledBySecurity, "The server pipeline must intercept POST requests that lack anti-forgery validation tokens.");
        }

        [Test]
        public async Task CreateNote_Post_MissingAntiforgeryToken_BlocksOrRedirectsSecuredPipeline()
        {
            var payload = new Dictionary<string, string> { { "noteBody", "Unauthorized Script Note Injection" } };
            var requestContent = new FormUrlEncodedContent(payload);

            var response = await _anonymousClient.PostAsync("/PeriodTracker/CreateNote", requestContent);

            bool isThrottledBySecurity = response.StatusCode == HttpStatusCode.BadRequest ||
                                         response.StatusCode == HttpStatusCode.Redirect;

            Assert.IsTrue(isThrottledBySecurity);
        }

        [Test]
        public async Task Index_Get_AuthenticatedClient_ReturnsOk()
        {
            var response = await _authenticatedClient.GetAsync("/PeriodTracker");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var html = await response.Content.ReadAsStringAsync();
            Assert.That(html, Does.Contain("Period Tracker Center"));
        }
    }
}