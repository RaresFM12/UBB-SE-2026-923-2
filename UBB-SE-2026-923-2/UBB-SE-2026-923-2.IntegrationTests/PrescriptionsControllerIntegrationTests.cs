using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using UBB_SE_2026_923_2.Data;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

/// <summary>
/// Happy-path controller integration tests for the Prescriptions web slice:
/// the assignment requires at least <c>Index</c> and the form submission
/// (here <c>Index [POST]</c> instead of <c>Create [POST]</c>, because the
/// prescription tool resolves rather than creates).
/// </summary>
[TestFixture]
public class PrescriptionsControllerIntegrationTests
{
    private PrescriptionsWebApplicationFactory factory = null!;
    private HttpClient client = null!;

    [SetUp]
    public void Setup()
    {
        factory = new PrescriptionsWebApplicationFactory();
        client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }

    [TearDown]
    public void TearDown()
    {
        client.Dispose();
        factory.Dispose();
    }

    [Test]
    public async Task Index_WhenAuthenticatedPharmacistOrAdmin_ReturnsForm()
    {
        var response = await client.GetAsync("/Prescriptions");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var html = await response.Content.ReadAsStringAsync();
        Assert.That(html, Does.Contain("Resolve Prescription"));
    }

    [Test]
    public async Task IndexPost_WhenKnownPrescriptionPosted_RendersResolvedItem()
    {
        int evaluationId = SeedPrescriptionWithAspirin();

        var token = await GetAntiForgeryTokenAsync("/Prescriptions");
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["PrescriptionId"] = evaluationId.ToString(),
            ["__RequestVerificationToken"] = token,
        });

        var response = await client.PostAsync("/Prescriptions", form);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var html = await response.Content.ReadAsStringAsync();
        Assert.That(html, Does.Contain("Aspirin"));
    }

    private int SeedPrescriptionWithAspirin()
    {
        var databaseContextFactory = factory.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var databaseContext = databaseContextFactory.CreateDbContext();

        var evaluatingDoctor = new Doctor
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.test",
            ContactInfo = "555-0100",
            Specialization = "General",
            LicenseNumber = "DOC-001",
            Role = "Doctor",
            DoctorStatus = DoctorStatus.AVAILABLE,
        };
        databaseContext.Doctors.Add(evaluatingDoctor);
        databaseContext.SaveChanges();

        var evaluation = new MedicalEvaluation
        {
            PatientId = "P-1",
            Symptoms = "Headache",
            MedicationsList = "Aspirin",
            Notes = string.Empty,
            EvaluationDate = DateTime.UtcNow,
            Evaluator = evaluatingDoctor,
        };
        databaseContext.MedicalEvaluations.Add(evaluation);

        var aspirin = new Item("Aspirin", "Bayer", "pain", price: 10f, numberOfPills: 30, quantity: 50);
        databaseContext.Items.Add(aspirin);

        databaseContext.SaveChanges();
        return evaluation.EvaluationID;
    }

    private async Task<string> GetAntiForgeryTokenAsync(string url)
    {
        var pageResponse = await client.GetAsync(url);
        Assert.That(pageResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var html = await pageResponse.Content.ReadAsStringAsync();
        var match = Regex.Match(
            html,
            @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");

        Assert.That(match.Success, Is.True, $"Anti-forgery token not found on {url}");
        return match.Groups[1].Value;
    }
}
