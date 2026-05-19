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
    public class ProductCatalogueIntegrationTests
    {
        private ProductCatalogueWebApplicationFactory _factory;
        private HttpClient _client;

        [SetUp]
        public void Setup()
        {
            _factory = new ProductCatalogueWebApplicationFactory();
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [Test]
        public async Task Index_Get_AnonymousUser_ReturnsSuccess()
        {
            var response = await _client.GetAsync("/ProductCatalogue");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Public users must be able to view the product catalogue grid.");
        }

        [Test]
        public async Task Details_Get_AnonymousUser_ReturnsSuccessOrNotFound()
        {
            var response = await _client.GetAsync("/ProductCatalogue/Details/1");

            bool isPubliclyAccessible = response.StatusCode == HttpStatusCode.OK ||
                                        response.StatusCode == HttpStatusCode.NotFound;

            Assert.IsTrue(isPubliclyAccessible, "Product details pages must remain accessible to unregistered browsing users.");
        }

        [Test]
        public async Task Create_Get_AnonymousUser_RedirectsToLoginSystemGate()
        {
            var response = await _client.GetAsync("/ProductCatalogue/Create");

            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);

            var locationHeader = response.Headers.Location?.ToString() ?? "";
            bool isRedirectedToAuth = locationHeader.Contains("Login", StringComparison.OrdinalIgnoreCase) ||
                                      locationHeader.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase);

            Assert.IsTrue(isRedirectedToAuth, "System must block non-admin users from accessing inventory modification routes.");
        }

        [Test]
        public async Task AddToCart_Post_MissingAntiForgeryToken_BlocksOrRedirectsSecuredPipeline()
        {
            var payload = new Dictionary<string, string>
            {
                { "itemId", "1" },
                { "quantity", "1" }
            };
            var requestContent = new FormUrlEncodedContent(payload);

            var response = await _client.PostAsync("/ProductCatalogue/AddToCart", requestContent);

            bool isThrottledBySecurity = response.StatusCode == HttpStatusCode.BadRequest ||
                                         response.StatusCode == HttpStatusCode.Redirect;

            Assert.IsTrue(isThrottledBySecurity, "The server pipeline must intercept POST requests that lack anti-forgery validation tokens.");
        }
    }
}