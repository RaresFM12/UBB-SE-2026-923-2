using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class HangoutParticipantsControllerIntegrationTests
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
    public async Task GetAllParticipants_EmptyDatabase_ReturnsOk()
    {
        var response = await client.GetAsync("/api/HangoutParticipants");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateParticipant_ValidRequest_ReturnsNoContent()
    {
        var request = new { HangoutId = 1, StaffId = 1 };
        var response = await client.PostAsJsonAsync("/api/HangoutParticipants", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task GetAllParticipants_EmptyDatabase_ReturnsEmptyArray()
    {
        var content = await client.GetStringAsync("/api/HangoutParticipants");
        Assert.That(content, Is.EqualTo("[]"));
    }
}
