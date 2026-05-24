using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

namespace UBB_SE_2026_923_2.IntegrationTests
{
    [TestFixture]
    public class PharmacyScheduleControllerTests
    {
        // AICI E SECRETUL: Îi spunem clar să pornească proiectul .Web, nu .WebApi!
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
            // Arrange: Configurăm clientul să nu dea redirect automat
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Act: Accesăm explicit ruta paginii tale
            var response = await client.GetAsync("/PharmacySchedule/Index");

            // Assert: Verificăm dacă primim eroare 302 (Redirect) pentru că nu suntem logați
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Found).Or.EqualTo(HttpStatusCode.Redirect));

            // Assert: Verificăm dacă suntem trimiși spre pagina de Login
            Assert.That(response.Headers.Location?.ToString(), Does.Contain("Login").IgnoreCase);
        }
    }
}