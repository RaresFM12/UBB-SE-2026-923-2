using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class ERRequestsControllerIntegrationTests
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
    public async Task GetAllERRequests_EmptyDatabase_ReturnsEmptyList()
    {
        var response = await client.GetAsync("/api/ERRequests");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var requests = await response.Content.ReadFromJsonAsync<List<ERRequest>>();
        Assert.That(requests!.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task CreateERRequest_ValidRequest_ReturnsId()
    {
        var request = new
        {
            Specialization = "Cardiology",
            Location = "Building A",
            Status = "Pending"
        };

        var response = await client.PostAsJsonAsync("/api/ERRequests", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateAndGetERRequest_ReturnsCreatedRequest()
    {
        var request = new
        {
            Specialization = "Neurology",
            Location = "Building B",
            Status = "Active"
        };

        await client.PostAsJsonAsync("/api/ERRequests", request);
        var requests = await client.GetFromJsonAsync<List<ERRequest>>("/api/ERRequests");
        Assert.That(requests!.Any(repository => repository.Specialization == "Neurology"), Is.True);
    }

    [Test]
    public async Task GetERRequestById_NonExistentId_ReturnsNotFound()
    {
        var response = await client.GetAsync("/api/ERRequests/9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task CreateERRequest_ThenGetById_ReturnsOk()
    {
        var request = new { Specialization = "Ortho", Location = "C", Status = "New" };
        var createResponse = await client.PostAsJsonAsync("/api/ERRequests", request);
        var id = await createResponse.Content.ReadFromJsonAsync<int>();
        var response = await client.GetAsync($"/api/ERRequests/{id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateERRequest_ThenUpdateStatus_ReturnsNoContent()
    {
        var request = new { Specialization = "Derm", Location = "D", Status = "Pending" };
        var createResponse = await client.PostAsJsonAsync("/api/ERRequests", request);
        var id = await createResponse.Content.ReadFromJsonAsync<int>();
        var statusReq = new { Status = "Assigned", AssignedDoctorId = 1, AssignedDoctorName = "Dr. Smith" };
        var response = await client.PatchAsJsonAsync($"/api/ERRequests/{id}/status", statusReq);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateMultipleERRequests_GetAll_ReturnsAll()
    {
        await client.PostAsJsonAsync("/api/ERRequests", new { Specialization = "X", Location = "L1", Status = "P" });
        await client.PostAsJsonAsync("/api/ERRequests", new { Specialization = "Y", Location = "L2", Status = "P" });
        var requests = await client.GetFromJsonAsync<List<ERRequest>>("/api/ERRequests");
        Assert.That(requests!.Count, Is.GreaterThanOrEqualTo(2));
    }
}
