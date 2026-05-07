using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class HangoutsControllerIntegrationTests
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
    public async Task GetAllHangouts_EmptyDatabase_ReturnsEmptyList()
    {
        var response = await client.GetAsync("/api/Hangouts");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var hangouts = await response.Content.ReadFromJsonAsync<List<Hangout>>();
        Assert.That(hangouts!.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task CreateHangout_ValidRequest_ReturnsId()
    {
        var request = new
        {
            Title = "Coffee Break",
            Description = "Quick coffee",
            Date = DateTime.UtcNow.AddDays(1),
            MaxParticipants = 5
        };
        var response = await client.PostAsJsonAsync("/api/Hangouts", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateHangout_ThenGetAll_ReturnsNonEmpty()
    {
        var request = new
        {
            Title = "Lunch",
            Description = "Team lunch",
            Date = DateTime.UtcNow.AddDays(2),
            MaxParticipants = 10
        };
        await client.PostAsJsonAsync("/api/Hangouts", request);
        var hangouts = await client.GetFromJsonAsync<List<Hangout>>("/api/Hangouts");
        Assert.That(hangouts!.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetHangoutById_NonExistent_ReturnsNotFound()
    {
        var response = await client.GetAsync("/api/Hangouts/9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task CreateHangout_ThenGetById_ReturnsOk()
    {
        var request = new
        {
            Title = "Dinner",
            Description = "Team dinner",
            Date = DateTime.UtcNow.AddDays(5),
            MaxParticipants = 8
        };
        var createResponse = await client.PostAsJsonAsync("/api/Hangouts", request);
        var id = await createResponse.Content.ReadFromJsonAsync<int>();
        var response = await client.GetAsync($"/api/Hangouts/{id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
