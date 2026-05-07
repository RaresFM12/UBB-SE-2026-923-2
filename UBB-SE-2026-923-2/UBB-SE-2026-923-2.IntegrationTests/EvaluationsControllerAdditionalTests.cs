using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class EvaluationsControllerAdditionalTests
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
    public async Task CreateEvaluation_LongNotes_ReturnsNoContent()
    {
        var request = new { DoctorId = 1, PatientId = 1, Diagnosis = "D", Notes = new string('N', 1000), Medications = "M", AssumedRisk = false };
        var response = await client.PostAsJsonAsync("/api/Evaluations", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateEvaluation_MultipleMedications_ReturnsNoContent()
    {
        var request = new { DoctorId = 1, PatientId = 2, Diagnosis = "D", Notes = "N", Medications = "Aspirin, Ibuprofen, Paracetamol", AssumedRisk = false };
        var response = await client.PostAsJsonAsync("/api/Evaluations", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateEvaluation_ThenUpdate_NotesChange()
    {
        await client.PostAsJsonAsync("/api/Evaluations", new { DoctorId = 1, PatientId = 3, Diagnosis = "D", Notes = "Original", Medications = "M", AssumedRisk = false });
        var evaluations = await client.GetFromJsonAsync<List<MedicalEvaluation>>("/api/Evaluations");
        var id = evaluations!.First().EvaluationID;
        var updateReq = new { Diagnosis = "Updated", Notes = "Changed", Medications = "NewMed" };
        var response = await client.PutAsJsonAsync($"/api/Evaluations/{id}", updateReq);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateEvaluation_ThenDelete_ReducesCount()
    {
        await client.PostAsJsonAsync("/api/Evaluations", new { DoctorId = 1, PatientId = 4, Diagnosis = "D", Notes = "N", Medications = "M", AssumedRisk = false });
        await client.PostAsJsonAsync("/api/Evaluations", new { DoctorId = 2, PatientId = 5, Diagnosis = "D2", Notes = "N2", Medications = "M2", AssumedRisk = false });
        var evaluations = await client.GetFromJsonAsync<List<MedicalEvaluation>>("/api/Evaluations");
        var id = evaluations!.First().EvaluationID;
        await client.DeleteAsync($"/api/Evaluations/{id}");
        var remaining = await client.GetFromJsonAsync<List<MedicalEvaluation>>("/api/Evaluations");
        Assert.That(remaining!.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task CreateEvaluation_DifferentDoctors_AllPersist()
    {
        await client.PostAsJsonAsync("/api/Evaluations", new { DoctorId = 10, PatientId = 1, Diagnosis = "A", Notes = "N", Medications = "M", AssumedRisk = false });
        await client.PostAsJsonAsync("/api/Evaluations", new { DoctorId = 20, PatientId = 2, Diagnosis = "B", Notes = "N", Medications = "M", AssumedRisk = false });
        await client.PostAsJsonAsync("/api/Evaluations", new { DoctorId = 30, PatientId = 3, Diagnosis = "C", Notes = "N", Medications = "M", AssumedRisk = true });
        var evaluations = await client.GetFromJsonAsync<List<MedicalEvaluation>>("/api/Evaluations");
        Assert.That(evaluations!.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task CreateEvaluation_EmptyDiagnosis_ReturnsNoContent()
    {
        var request = new { DoctorId = 1, PatientId = 1, Diagnosis = "", Notes = "N", Medications = "M", AssumedRisk = false };
        var response = await client.PostAsJsonAsync("/api/Evaluations", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateEvaluation_EmptyNotes_ReturnsNoContent()
    {
        var request = new { DoctorId = 1, PatientId = 1, Diagnosis = "D", Notes = "", Medications = "M", AssumedRisk = false };
        var response = await client.PostAsJsonAsync("/api/Evaluations", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task DeleteEvaluation_ThenGetAll_DoesNotContainDeleted()
    {
        await client.PostAsJsonAsync("/api/Evaluations", new { DoctorId = 5, PatientId = 5, Diagnosis = "Only", Notes = "N", Medications = "M", AssumedRisk = false });
        var evaluations = await client.GetFromJsonAsync<List<MedicalEvaluation>>("/api/Evaluations");
        var id = evaluations!.First().EvaluationID;
        await client.DeleteAsync($"/api/Evaluations/{id}");
        var remaining = await client.GetFromJsonAsync<List<MedicalEvaluation>>("/api/Evaluations");
        Assert.That(remaining!.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task UpdateEvaluation_ChangesMedications_Persists()
    {
        await client.PostAsJsonAsync("/api/Evaluations", new { DoctorId = 1, PatientId = 1, Diagnosis = "D", Notes = "N", Medications = "OldMed", AssumedRisk = false });
        var evaluations = await client.GetFromJsonAsync<List<MedicalEvaluation>>("/api/Evaluations");
        var id = evaluations!.First().EvaluationID;
        await client.PutAsJsonAsync($"/api/Evaluations/{id}", new { Diagnosis = "D", Notes = "N", Medications = "NewMed" });
        var updated = await client.GetFromJsonAsync<List<MedicalEvaluation>>("/api/Evaluations");
        Assert.That(updated!.First().MedicationsList, Is.EqualTo("NewMed"));
    }

    [Test]
    public async Task CreateTenEvaluations_GetAll_ReturnsTen()
    {
        for (int i = 0; i < 10; i++)
        {
            await client.PostAsJsonAsync("/api/Evaluations", new { DoctorId = i, PatientId = i, Diagnosis = $"D{i}", Notes = "N", Medications = "M", AssumedRisk = false });
        }
        var evaluations = await client.GetFromJsonAsync<List<MedicalEvaluation>>("/api/Evaluations");
        Assert.That(evaluations!.Count, Is.EqualTo(10));
    }
}
