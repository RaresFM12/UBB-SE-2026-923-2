using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class EvaluationsControllerIntegrationTests
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
    public async Task GetAllEvaluations_EmptyDatabase_ReturnsEmptyList()
    {
        var response = await client.GetAsync("/api/Evaluations");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var evaluations = await response.Content.ReadFromJsonAsync<List<MedicalEvaluation>>();
        Assert.That(evaluations!.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task CreateEvaluation_ValidRequest_ReturnsNoContent()
    {
        var request = new
        {
            DoctorId = 1,
            PatientId = 1,
            Diagnosis = "Headache",
            Notes = "Take rest",
            Medications = "Aspirin",
            AssumedRisk = false
        };

        var response = await client.PostAsJsonAsync("/api/Evaluations", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateAndGetEvaluation_ReturnsCreatedEvaluation()
    {
        var request = new
        {
            DoctorId = 2,
            PatientId = 3,
            Diagnosis = "Flu",
            Notes = "Rest and fluids",
            Medications = "Paracetamol",
            AssumedRisk = false
        };

        await client.PostAsJsonAsync("/api/Evaluations", request);
        var evaluations = await client.GetFromJsonAsync<List<MedicalEvaluation>>("/api/Evaluations");
        Assert.That(evaluations!.Any(e => e.Notes == "Rest and fluids"), Is.True);
    }

    [Test]
    public async Task DeleteEvaluation_NonExistentId_DoesNotReturnServerError()
    {
        var response = await client.DeleteAsync("/api/Evaluations/9999");
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task UpdateEvaluation_NonExistentId_DoesNotReturnServerError()
    {
        var request = new { Diagnosis = "Updated", Notes = "Updated", Medications = "Updated" };
        var response = await client.PutAsJsonAsync("/api/Evaluations/9999", request);
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task CreateMultipleEvaluations_GetAll_ReturnsAll()
    {
        var req1 = new { DoctorId = 1, PatientId = 1, Diagnosis = "A", Notes = "N1", Medications = "M1", AssumedRisk = false };
        var req2 = new { DoctorId = 2, PatientId = 2, Diagnosis = "B", Notes = "N2", Medications = "M2", AssumedRisk = true };
        await client.PostAsJsonAsync("/api/Evaluations", req1);
        await client.PostAsJsonAsync("/api/Evaluations", req2);
        var evaluations = await client.GetFromJsonAsync<List<MedicalEvaluation>>("/api/Evaluations");
        Assert.That(evaluations!.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task CreateEvaluation_WithAssumedRisk_ReturnsNoContent()
    {
        var request = new
        {
            DoctorId = 3,
            PatientId = 4,
            Diagnosis = "HighRisk",
            Notes = "Careful",
            Medications = "DrugX",
            AssumedRisk = true
        };
        var response = await client.PostAsJsonAsync("/api/Evaluations", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateEvaluation_EmptyMedications_ReturnsNoContent()
    {
        var request = new
        {
            DoctorId = 1,
            PatientId = 1,
            Diagnosis = "None",
            Notes = "Healthy",
            Medications = "",
            AssumedRisk = false
        };
        var response = await client.PostAsJsonAsync("/api/Evaluations", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
}
