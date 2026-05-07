using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class SubstancesControllerAdditionalTests
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
    public async Task CreateSubstance_ZeroLethalDose_ReturnsNoContent()
    {
        var request = new { Name = "Safe", LethalDose = 0f, Description = "Harmless" };
        var response = await client.PostAsJsonAsync("/api/Substances", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateSubstance_HighLethalDose_ReturnsNoContent()
    {
        var request = new { Name = "VeryToxic", LethalDose = 99999f, Description = "Extremely dangerous" };
        var response = await client.PostAsJsonAsync("/api/Substances", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateSubstance_EmptyDescription_ReturnsNoContent()
    {
        var request = new { Name = "NoDesc", LethalDose = 100f, Description = "" };
        var response = await client.PostAsJsonAsync("/api/Substances", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateSubstance_LongName_ReturnsNoContent()
    {
        var request = new { Name = new string('Z', 150), LethalDose = 50f, Description = "Long" };
        var response = await client.PostAsJsonAsync("/api/Substances", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateMultipleSubstances_GetAll_ReturnsCorrectCount()
    {
        for (int i = 0; i < 5; i++)
        {
            await client.PostAsJsonAsync("/api/Substances", new { Name = $"Sub{i}", LethalDose = (float)(i * 100), Description = $"D{i}" });
        }
        var substances = await client.GetFromJsonAsync<List<Substance>>("/api/Substances");
        Assert.That(substances!.Count, Is.EqualTo(5));
    }

    [Test]
    public async Task CreateSubstance_ThenUpdate_ReturnsNoContent()
    {
        await client.PostAsJsonAsync("/api/Substances", new { Name = "Updatable", LethalDose = 100f, Description = "Old" });
        var substance = await client.GetFromJsonAsync<Substance>("/api/Substances/Updatable");
        substance!.Description = "New";
        var response = await client.PutAsJsonAsync("/api/Substances/Updatable", substance);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateSubstance_ThenGetByName_HasCorrectLethalDose()
    {
        await client.PostAsJsonAsync("/api/Substances", new { Name = "DoseCheck", LethalDose = 777f, Description = "Test" });
        var substance = await client.GetFromJsonAsync<Substance>("/api/Substances/DoseCheck");
        Assert.That(substance!.LethalDose, Is.EqualTo(777f));
    }

    [Test]
    public async Task GetSubstanceByName_AfterCreate_ReturnsCorrectDescription()
    {
        await client.PostAsJsonAsync("/api/Substances", new { Name = "DescCheck", LethalDose = 50f, Description = "MyDescription" });
        var substance = await client.GetFromJsonAsync<Substance>("/api/Substances/DescCheck");
        Assert.That(substance!.Description, Is.EqualTo("MyDescription"));
    }

    [Test]
    public async Task GetTopSubstances_AfterCreating_ReturnsOk()
    {
        await client.PostAsJsonAsync("/api/Substances", new { Name = "TopTest", LethalDose = 100f, Description = "T" });
        var response = await client.GetAsync("/api/Substances/top");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateSubstance_SpecialCharsInName_ReturnsNoContent()
    {
        var request = new { Name = "Sub-With_Special.Chars", LethalDose = 10f, Description = "D" };
        var response = await client.PostAsJsonAsync("/api/Substances", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
}
