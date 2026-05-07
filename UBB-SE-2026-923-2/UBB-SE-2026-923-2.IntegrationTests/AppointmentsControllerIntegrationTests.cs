using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class AppointmentsControllerIntegrationTests
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
    public async Task GetAllAppointments_EmptyDatabase_ReturnsEmptyList()
    {
        var response = await client.GetAsync("/api/Appointments");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var appointments = await response.Content.ReadFromJsonAsync<List<Appointment>>();
        Assert.That(appointments!.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task CreateAppointment_ValidRequest_ReturnsNoContent()
    {
        var request = new
        {
            PatientId = 1,
            DoctorId = 2,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Status = "Scheduled"
        };

        var response = await client.PostAsJsonAsync("/api/Appointments", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateAndGetAppointments_ReturnsCreatedAppointment()
    {
        var request = new
        {
            PatientId = 5,
            DoctorId = 10,
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(2).AddHours(1),
            Status = "Pending"
        };

        await client.PostAsJsonAsync("/api/Appointments", request);
        var appointments = await client.GetFromJsonAsync<List<Appointment>>("/api/Appointments");
        Assert.That(appointments!.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task CreateMultipleAppointments_GetAll_ReturnsAll()
    {
        var req1 = new { PatientId = 1, DoctorId = 1, StartTime = DateTime.UtcNow.AddDays(3), EndTime = DateTime.UtcNow.AddDays(3).AddHours(1), Status = "S1" };
        var req2 = new { PatientId = 2, DoctorId = 2, StartTime = DateTime.UtcNow.AddDays(4), EndTime = DateTime.UtcNow.AddDays(4).AddHours(1), Status = "S2" };
        await client.PostAsJsonAsync("/api/Appointments", req1);
        await client.PostAsJsonAsync("/api/Appointments", req2);
        var appointments = await client.GetFromJsonAsync<List<Appointment>>("/api/Appointments");
        Assert.That(appointments!.Count, Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public async Task CreateAppointment_ThenUpdateStatus_ReturnsNoContent()
    {
        var request = new
        {
            PatientId = 7,
            DoctorId = 3,
            StartTime = DateTime.UtcNow.AddDays(5),
            EndTime = DateTime.UtcNow.AddDays(5).AddHours(1),
            Status = "Scheduled"
        };
        await client.PostAsJsonAsync("/api/Appointments", request);
        var appointments = await client.GetFromJsonAsync<List<Appointment>>("/api/Appointments");
        var id = appointments!.First().Id;
        var statusReq = new { Status = "Completed" };
        var response = await client.PatchAsJsonAsync($"/api/Appointments/{id}/status", statusReq);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
}
