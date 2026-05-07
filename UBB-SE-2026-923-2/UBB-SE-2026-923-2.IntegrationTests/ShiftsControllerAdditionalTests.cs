using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class ShiftsControllerAdditionalTests
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
    public async Task CreateShift_EmptyLocation_ReturnsNoContent()
    {
        var request = new { StaffId = 1, Location = "", StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(1).AddHours(8), Status = 0 };
        var response = await client.PostAsJsonAsync("/api/Shifts", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateShift_LongLocation_ReturnsNoContent()
    {
        var request = new { StaffId = 1, Location = new string('L', 200), StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(1).AddHours(8), Status = 0 };
        var response = await client.PostAsJsonAsync("/api/Shifts", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateShift_FutureDate_ReturnsNoContent()
    {
        var request = new { StaffId = 2, Location = "Ward", StartTime = DateTime.UtcNow.AddMonths(6), EndTime = DateTime.UtcNow.AddMonths(6).AddHours(12), Status = 0 };
        var response = await client.PostAsJsonAsync("/api/Shifts", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateShift_DifferentStatus_ReturnsNoContent()
    {
        var request = new { StaffId = 3, Location = "ICU", StartTime = DateTime.UtcNow.AddDays(2), EndTime = DateTime.UtcNow.AddDays(2).AddHours(8), Status = 1 };
        var response = await client.PostAsJsonAsync("/api/Shifts", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateMultipleShifts_GetAll_ResponseIsOk()
    {
        for (int i = 0; i < 3; i++)
        {
            await client.PostAsJsonAsync("/api/Shifts", new { StaffId = i + 1, Location = $"L{i}", StartTime = DateTime.UtcNow.AddDays(i + 1), EndTime = DateTime.UtcNow.AddDays(i + 1).AddHours(8), Status = 0 });
        }
        var response = await client.GetAsync("/api/Shifts");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task UpdateShiftStaff_NonExistentId_DoesNotReturnServerError()
    {
        var request = new { StaffId = 99 };
        var response = await client.PatchAsJsonAsync("/api/Shifts/9999/staff", request);
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task CreateShift_NullLocation_ReturnsNoContent()
    {
        var request = new { StaffId = 4, Location = (string?)null, StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(1).AddHours(8), Status = 0 };
        var response = await client.PostAsJsonAsync("/api/Shifts", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateShift_ShortDuration_ReturnsNoContent()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var request = new { StaffId = 5, Location = "Quick", StartTime = start, EndTime = start.AddMinutes(15), Status = 0 };
        var response = await client.PostAsJsonAsync("/api/Shifts", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task DeleteShift_AfterCreate_DoesNotReturnServerError()
    {
        await client.PostAsJsonAsync("/api/Shifts", new { StaffId = 6, Location = "Del", StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(1).AddHours(8), Status = 0 });
        var response = await client.DeleteAsync("/api/Shifts/1");
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task GetAllShifts_AfterCreating_ResponseIsOk()
    {
        await client.PostAsJsonAsync("/api/Shifts", new { StaffId = 7, Location = "Content", StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(1).AddHours(8), Status = 0 });
        var response = await client.GetAsync("/api/Shifts");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
