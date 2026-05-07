using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class HangoutsControllerAdditionalTests
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
    public async Task CreateHangout_ZeroMaxParticipants_ReturnsOk()
    {
        var request = new { Title = "Solo", Description = "Just me", Date = DateTime.UtcNow.AddDays(1), MaxParticipants = 0 };
        var response = await client.PostAsJsonAsync("/api/Hangouts", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateHangout_LargeMaxParticipants_ReturnsOk()
    {
        var request = new { Title = "Big", Description = "Many people", Date = DateTime.UtcNow.AddDays(1), MaxParticipants = 1000 };
        var response = await client.PostAsJsonAsync("/api/Hangouts", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateHangout_EmptyTitle_ReturnsOk()
    {
        var request = new { Title = "", Description = "No title", Date = DateTime.UtcNow.AddDays(1), MaxParticipants = 5 };
        var response = await client.PostAsJsonAsync("/api/Hangouts", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateHangout_EmptyDescription_ReturnsOk()
    {
        var request = new { Title = "NoDesc", Description = "", Date = DateTime.UtcNow.AddDays(1), MaxParticipants = 5 };
        var response = await client.PostAsJsonAsync("/api/Hangouts", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateMultipleHangouts_GetAll_ReturnsCorrectCount()
    {
        for (int i = 0; i < 4; i++)
        {
            await client.PostAsJsonAsync("/api/Hangouts", new { Title = $"H{i}", Description = "D", Date = DateTime.UtcNow.AddDays(i + 1), MaxParticipants = 3 });
        }
        var hangouts = await client.GetFromJsonAsync<List<Hangout>>("/api/Hangouts");
        Assert.That(hangouts!.Count, Is.EqualTo(4));
    }

    [Test]
    public async Task CreateHangout_FarFutureDate_ReturnsOk()
    {
        var request = new { Title = "Future", Description = "D", Date = DateTime.UtcNow.AddYears(2), MaxParticipants = 10 };
        var response = await client.PostAsJsonAsync("/api/Hangouts", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateHangout_LongTitle_ReturnsOk()
    {
        var request = new { Title = new string('T', 200), Description = "D", Date = DateTime.UtcNow.AddDays(1), MaxParticipants = 5 };
        var response = await client.PostAsJsonAsync("/api/Hangouts", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateHangout_ThenGetById_TitleMatches()
    {
        var createResponse = await client.PostAsJsonAsync("/api/Hangouts", new { Title = "Verify", Description = "D", Date = DateTime.UtcNow.AddDays(1), MaxParticipants = 5 });
        var id = await createResponse.Content.ReadFromJsonAsync<int>();
        var hangout = await client.GetFromJsonAsync<Hangout>($"/api/Hangouts/{id}");
        Assert.That(hangout!.Title, Is.EqualTo("Verify"));
    }

    [Test]
    public async Task CreateHangout_ThenGetById_MaxParticipantsMatches()
    {
        var createResponse = await client.PostAsJsonAsync("/api/Hangouts", new { Title = "MP", Description = "D", Date = DateTime.UtcNow.AddDays(1), MaxParticipants = 42 });
        var id = await createResponse.Content.ReadFromJsonAsync<int>();
        var hangout = await client.GetFromJsonAsync<Hangout>($"/api/Hangouts/{id}");
        Assert.That(hangout!.MaxParticipants, Is.EqualTo(42));
    }

    [Test]
    public async Task GetHangoutById_AfterMultipleCreates_ReturnsCorrectOne()
    {
        await client.PostAsJsonAsync("/api/Hangouts", new { Title = "First", Description = "D", Date = DateTime.UtcNow.AddDays(1), MaxParticipants = 1 });
        var secondResponse = await client.PostAsJsonAsync("/api/Hangouts", new { Title = "Second", Description = "D", Date = DateTime.UtcNow.AddDays(2), MaxParticipants = 2 });
        var id = await secondResponse.Content.ReadFromJsonAsync<int>();
        var hangout = await client.GetFromJsonAsync<Hangout>($"/api/Hangouts/{id}");
        Assert.That(hangout!.Title, Is.EqualTo("Second"));
    }
}
