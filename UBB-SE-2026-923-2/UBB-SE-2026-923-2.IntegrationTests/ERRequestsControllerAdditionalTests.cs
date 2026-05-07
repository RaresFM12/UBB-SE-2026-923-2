using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class ERRequestsControllerAdditionalTests
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
    public async Task CreateERRequest_EmptySpecialization_ReturnsOk()
    {
        var request = new { Specialization = "", Location = "L", Status = "P" };
        var response = await client.PostAsJsonAsync("/api/ERRequests", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateERRequest_EmptyLocation_ReturnsOk()
    {
        var request = new { Specialization = "S", Location = "", Status = "P" };
        var response = await client.PostAsJsonAsync("/api/ERRequests", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateERRequest_LongSpecialization_ReturnsOk()
    {
        var request = new { Specialization = new string('S', 200), Location = "L", Status = "P" };
        var response = await client.PostAsJsonAsync("/api/ERRequests", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateMultipleERRequests_GetAll_ReturnsCorrectCount()
    {
        for (int i = 0; i < 5; i++)
        {
            await client.PostAsJsonAsync("/api/ERRequests", new { Specialization = $"Spec{i}", Location = $"L{i}", Status = "P" });
        }
        var requests = await client.GetFromJsonAsync<List<ERRequest>>("/api/ERRequests");
        Assert.That(requests!.Count, Is.EqualTo(5));
    }

    [Test]
    public async Task CreateERRequest_ThenUpdateStatus_WithNullDoctor_ReturnsNoContent()
    {
        var createResponse = await client.PostAsJsonAsync("/api/ERRequests", new { Specialization = "A", Location = "B", Status = "New" });
        var id = await createResponse.Content.ReadFromJsonAsync<int>();
        var statusReq = new { Status = "Updated", AssignedDoctorId = (int?)null, AssignedDoctorName = (string?)null };
        var response = await client.PatchAsJsonAsync($"/api/ERRequests/{id}/status", statusReq);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateERRequest_ThenGetById_SpecializationMatches()
    {
        var createResponse = await client.PostAsJsonAsync("/api/ERRequests", new { Specialization = "Cardio", Location = "Floor3", Status = "Active" });
        var id = await createResponse.Content.ReadFromJsonAsync<int>();
        var er = await client.GetFromJsonAsync<ERRequest>($"/api/ERRequests/{id}");
        Assert.That(er!.Specialization, Is.EqualTo("Cardio"));
    }

    [Test]
    public async Task CreateERRequest_ThenGetById_LocationMatches()
    {
        var createResponse = await client.PostAsJsonAsync("/api/ERRequests", new { Specialization = "Neuro", Location = "WingA", Status = "Active" });
        var id = await createResponse.Content.ReadFromJsonAsync<int>();
        var er = await client.GetFromJsonAsync<ERRequest>($"/api/ERRequests/{id}");
        Assert.That(er!.Location, Is.EqualTo("WingA"));
    }

    [Test]
    public async Task UpdateStatus_NonExistentERRequest_DoesNotReturnServerError()
    {
        var response = await client.PatchAsJsonAsync("/api/ERRequests/9999/status", new { Status = "X", AssignedDoctorId = (int?)null, AssignedDoctorName = (string?)null });
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task CreateTenERRequests_GetAll_ReturnsTen()
    {
        for (int i = 0; i < 10; i++)
        {
            await client.PostAsJsonAsync("/api/ERRequests", new { Specialization = $"S{i}", Location = $"L{i}", Status = "P" });
        }
        var requests = await client.GetFromJsonAsync<List<ERRequest>>("/api/ERRequests");
        Assert.That(requests!.Count, Is.EqualTo(10));
    }

    [Test]
    public async Task CreateERRequest_EmptyStatus_ReturnsOk()
    {
        var request = new { Specialization = "S", Location = "L", Status = "" };
        var response = await client.PostAsJsonAsync("/api/ERRequests", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
