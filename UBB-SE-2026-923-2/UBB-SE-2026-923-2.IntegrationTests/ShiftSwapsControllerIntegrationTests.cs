using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class ShiftSwapsControllerIntegrationTests
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
    public async Task GetAllShiftSwaps_EmptyDatabase_ReturnsEmptyList()
    {
        var response = await client.GetAsync("/api/ShiftSwaps");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var swaps = await response.Content.ReadFromJsonAsync<List<ShiftSwapRequest>>();
        Assert.That(swaps!.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetShiftSwapById_NonExistent_ReturnsNotFound()
    {
        var response = await client.GetAsync("/api/ShiftSwaps/9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task CreateShiftSwap_ValidRequest_ReturnsOk()
    {
        var request = new
        {
            SwapId = 0,
            ShiftId = 1,
            RequesterId = 1,
            ColleagueId = 2,
            RequestedAt = DateTime.UtcNow,
            Status = 0
        };
        var response = await client.PostAsJsonAsync("/api/ShiftSwaps", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
