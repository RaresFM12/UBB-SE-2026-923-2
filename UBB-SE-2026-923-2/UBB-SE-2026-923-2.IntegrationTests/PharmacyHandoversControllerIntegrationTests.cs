using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class PharmacyHandoversControllerIntegrationTests
{
    private CustomWebApplicationFactory factory;
    private HttpClient client;

    [SetUp]
    public void Setup()
    {
        factory = new CustomWebApplicationFactory();
        client = factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        client.Dispose();
        factory.Dispose();
    }

    [Test]
    public async Task GetAllHandovers_EmptyDatabase_ReturnsOk()
    {
        var response = await client.GetAsync("/api/PharmacyHandovers");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetAllHandovers_EmptyDatabase_ReturnsEmptyList()
    {
        var response = await client.GetAsync("/api/PharmacyHandovers");
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Is.EqualTo("[]"));
    }
}
