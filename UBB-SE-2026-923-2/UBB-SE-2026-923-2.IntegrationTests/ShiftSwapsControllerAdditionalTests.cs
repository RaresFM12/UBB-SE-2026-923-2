using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class ShiftSwapsControllerAdditionalTests
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
    public async Task CreateShiftSwap_ThenUpdateStatus_ReturnsNoContent()
    {
        var request = new { SwapId = 0, ShiftId = 1, RequesterId = 1, ColleagueId = 2, RequestedAt = DateTime.UtcNow, Status = 0 };
        var createResponse = await client.PostAsJsonAsync("/api/ShiftSwaps", request);
        var id = await createResponse.Content.ReadFromJsonAsync<int>();
        var statusReq = new { Status = "APPROVED" };
        var response = await client.PatchAsJsonAsync($"/api/ShiftSwaps/{id}/status", statusReq);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task UpdateSwapStatus_NonExistent_DoesNotReturnServerError()
    {
        var response = await client.PatchAsJsonAsync("/api/ShiftSwaps/9999/status", new { Status = "REJECTED" });
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task CreateShiftSwap_SameRequesterAndColleague_ReturnsOk()
    {
        var request = new { SwapId = 0, ShiftId = 1, RequesterId = 5, ColleagueId = 5, RequestedAt = DateTime.UtcNow, Status = 0 };
        var response = await client.PostAsJsonAsync("/api/ShiftSwaps", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateShiftSwap_HighIds_ReturnsOk()
    {
        var request = new { SwapId = 0, ShiftId = 99999, RequesterId = 88888, ColleagueId = 77777, RequestedAt = DateTime.UtcNow, Status = 0 };
        var response = await client.PostAsJsonAsync("/api/ShiftSwaps", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
