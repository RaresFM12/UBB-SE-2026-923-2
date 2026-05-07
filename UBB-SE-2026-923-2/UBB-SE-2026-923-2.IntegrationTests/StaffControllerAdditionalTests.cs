using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class StaffControllerAdditionalTests
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
    public async Task GetAllStaff_EmptyDatabase_ReturnsEmptyArray()
    {
        var content = await client.GetStringAsync("/api/Staff");
        Assert.That(content, Is.EqualTo("[]"));
    }

    [Test]
    public async Task GetDoctors_EmptyDatabase_ReturnsEmptyArray()
    {
        var content = await client.GetStringAsync("/api/Staff/doctors");
        Assert.That(content, Is.EqualTo("[]"));
    }

    [Test]
    public async Task GetPharmacists_EmptyDatabase_ReturnsEmptyArray()
    {
        var content = await client.GetStringAsync("/api/Staff/pharmacists");
        Assert.That(content, Is.EqualTo("[]"));
    }

    [Test]
    public async Task GetStaffById_ZeroId_ReturnsNotFound()
    {
        var response = await client.GetAsync("/api/Staff/0");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task UpdateAvailability_NonExistentStaff_DoesNotReturnServerError()
    {
        var request = new { IsAvailable = true, Status = 0 };
        var response = await client.PatchAsJsonAsync("/api/Staff/9999/availability", request);
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }
}
