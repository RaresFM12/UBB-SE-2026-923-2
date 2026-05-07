using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class ShiftsControllerIntegrationTests
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
    public async Task GetAllShifts_EmptyDatabase_ReturnsEmptyList()
    {
        var response = await client.GetAsync("/api/Shifts");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var shifts = await response.Content.ReadFromJsonAsync<List<Shift>>();
        Assert.That(shifts!.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task CreateShift_ValidRequest_ReturnsNoContent()
    {
        var request = new
        {
            StaffId = 1,
            Location = "Pharmacy A",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(8),
            Status = 0
        };

        var response = await client.PostAsJsonAsync("/api/Shifts", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateShift_ThenGetAll_ReturnsOkStatus()
    {
        var request = new
        {
            StaffId = 5,
            Location = "Ward B",
            StartTime = DateTime.UtcNow.AddDays(3),
            EndTime = DateTime.UtcNow.AddDays(3).AddHours(8),
            Status = 0
        };

        await client.PostAsJsonAsync("/api/Shifts", request);
        var response = await client.GetAsync("/api/Shifts");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task DeleteShift_NonExistentId_DoesNotReturnServerError()
    {
        var response = await client.DeleteAsync("/api/Shifts/9999");
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task UpdateShiftStatus_NonExistentId_DoesNotReturnServerError()
    {
        var request = new { Status = 1 };
        var response = await client.PatchAsJsonAsync("/api/Shifts/9999/status", request);
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }
}
