namespace UBB_SE_2026_923_2.IntegrationTests
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Testing;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;

    [TestFixture]
    public class UserAccountsControllerIntegrationTests
    {
        private WebMvcApplicationFactory factory;
        private HttpClient client;

        [SetUp]
        public void Setup()
        {
            this.factory = new WebMvcApplicationFactory();
            this.factory.UsersRepository.Seed(new User
            {
                Id = 1,
                Email = "admin@example.com",
                PhoneNumber = "0700000000",
                PasswordHash = "hash",
                Username = "admin",
                IsAdmin = true,
                Role = "Admin",
            });

            this.client = this.factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });
        }

        [TearDown]
        public void TearDown()
        {
            this.client.Dispose();
            this.factory.Dispose();
        }

        [Test]
        public async Task Index_Admin_ReturnsOk()
        {
            var response = await this.client.GetAsync("/UserAccounts");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Does.Contain("User accounts"));
        }

        [Test]
        public async Task Create_PostValid_AddsUser()
        {
            var form = new Dictionary<string, string>
            {
                ["Email"] = "newuser@example.com",
                ["Password"] = "Pass123!",
                ["ConfirmPassword"] = "Pass123!",
                ["Username"] = "newuser",
                ["PhoneNumber"] = "0700000001",
                ["Role"] = "Client",
            };

            var response = await this.client.PostAsync(
                "/UserAccounts/Create",
                new FormUrlEncodedContent(form));

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));

            var createdUser = this.factory.UsersRepository.GetUserByEmail("newuser@example.com");
            Assert.That(createdUser, Is.Not.Null);
        }
    }
}
