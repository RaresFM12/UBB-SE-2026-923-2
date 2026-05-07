using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class SubstancesControllerIntegrationTests
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
    public async Task GetAllSubstances_EmptyDatabase_ReturnsEmptyList()
    {
        var response = await client.GetAsync("/api/Substances");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var substances = await response.Content.ReadFromJsonAsync<List<Substance>>();
        Assert.That(substances!.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task CreateSubstance_ValidRequest_ReturnsNoContent()
    {
        var request = new
        {
            Name = "Acetylsalicylic Acid",
            LethalDose = 500.0f,
            Description = "Common pain reliever substance"
        };

        var response = await client.PostAsJsonAsync("/api/Substances", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateAndGetSubstance_ReturnsCreatedSubstance()
    {
        var request = new
        {
            Name = "Ibuprofen",
            LethalDose = 600.0f,
            Description = "NSAID"
        };

        await client.PostAsJsonAsync("/api/Substances", request);
        var substances = await client.GetFromJsonAsync<List<Substance>>("/api/Substances");
        Assert.That(substances!.Any(s => s.Name == "Ibuprofen"), Is.True);
    }

    [Test]
    public async Task GetSubstanceByName_NonExistent_ReturnsNotFound()
    {
        var response = await client.GetAsync("/api/Substances/NonExistentSubstance");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetSubstanceExists_NonExistent_ReturnsFalse()
    {
        var result = await client.GetFromJsonAsync<bool>("/api/Substances/NonExistentSubstance/exists");
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetTopSubstances_EmptyDatabase_ReturnsOk()
    {
        var response = await client.GetAsync("/api/Substances/top");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateSubstance_ThenGetByName_ReturnsOk()
    {
        var request = new { Name = "Paracetamol", LethalDose = 400.0f, Description = "Fever reducer" };
        await client.PostAsJsonAsync("/api/Substances", request);
        var response = await client.GetAsync("/api/Substances/Paracetamol");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateSubstance_ThenExists_ReturnsTrue()
    {
        var request = new { Name = "Codeine", LethalDose = 800.0f, Description = "Opioid" };
        await client.PostAsJsonAsync("/api/Substances", request);
        var result = await client.GetFromJsonAsync<bool>("/api/Substances/Codeine/exists");
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CreateMultipleSubstances_GetAll_ReturnsAll()
    {
        await client.PostAsJsonAsync("/api/Substances", new { Name = "SubA", LethalDose = 100.0f, Description = "A" });
        await client.PostAsJsonAsync("/api/Substances", new { Name = "SubB", LethalDose = 200.0f, Description = "B" });
        var substances = await client.GetFromJsonAsync<List<Substance>>("/api/Substances");
        Assert.That(substances!.Count, Is.GreaterThanOrEqualTo(2));
    }
}
