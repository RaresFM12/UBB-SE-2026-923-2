using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

namespace UBB_SE_2026_923_2.IntegrationTests // Dacă ai pus testul în proiectul .Tests, poți lăsa namespace-ul așa, NUnit îl va găsi oricum
{
    [TestFixture]
    public class PharmacyVacationControllerTests
    {
        // Țintim direct proiectul .Web pentru a avea acces la interfața grafică
        private WebApplicationFactory<UBB_SE_2026_923_2.Web.Program> _factory;

        [SetUp]
        public void SetUp()
        {
            _factory = new WebApplicationFactory<UBB_SE_2026_923_2.Web.Program>();
        }

        [TearDown]
        public void TearDown()
        {
            _factory.Dispose();
        }

        [Test]
        public async Task Index_AccesatFaraAutentificare_ReturneazaRedirectCatreLogin()
        {
            // Arrange: Oprim redirectul automat pentru a putea citi codul HTTP 302
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Act: Încercăm să accesăm pagina de Request Vacation fără să fim logați
            var response = await client.GetAsync("/PharmacyVacation/Index");

            // Assert: Ne asigurăm că serverul ne respinge și ne redirecționează
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Found).Or.EqualTo(HttpStatusCode.Redirect));

            // Assert: Ne asigurăm că destinația redirecționării este pagina de Login
            Assert.That(response.Headers.Location?.ToString(), Does.Contain("Login").IgnoreCase);
        }
    }
}