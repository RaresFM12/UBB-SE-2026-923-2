using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class StaffControllerIntegrationTests
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
    public async Task GetAllStaff_EmptyDatabase_ReturnsOk()
    {
        var response = await client.GetAsync("/api/Staff");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetStaffById_NonExistent_ReturnsNotFound()
    {
        var response = await client.GetAsync("/api/Staff/9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetDoctors_EmptyDatabase_ReturnsOk()
    {
        var response = await client.GetAsync("/api/Staff/doctors");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetPharmacists_EmptyDatabase_ReturnsOk()
    {
        var response = await client.GetAsync("/api/Staff/pharmacists");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task UpdateStaffStatus_NonExistent_DoesNotReturnServerError()
    {
        var request = new { Status = "Active" };
        var response = await client.PatchAsJsonAsync("/api/Staff/9999/status", request);
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }
}
