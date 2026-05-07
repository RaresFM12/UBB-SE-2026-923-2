using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class AppointmentsControllerAdditionalTests
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
    public async Task CreateAppointment_CancelledStatus_ReturnsNoContent()
    {
        var request = new { PatientId = 1, DoctorId = 1, StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(1).AddHours(1), Status = "Cancelled" };
        var response = await client.PostAsJsonAsync("/api/Appointments", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateMultipleAppointments_GetAll_ReturnsCorrectCount()
    {
        for (int i = 0; i < 5; i++)
        {
            await client.PostAsJsonAsync("/api/Appointments", new { PatientId = i, DoctorId = i, StartTime = DateTime.UtcNow.AddDays(i + 1), EndTime = DateTime.UtcNow.AddDays(i + 1).AddHours(1), Status = "S" });
        }
        var appointments = await client.GetFromJsonAsync<List<Appointment>>("/api/Appointments");
        Assert.That(appointments!.Count, Is.EqualTo(5));
    }

    [Test]
    public async Task CreateAppointment_EmptyStatus_ReturnsNoContent()
    {
        var request = new { PatientId = 1, DoctorId = 1, StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(1).AddHours(1), Status = "" };
        var response = await client.PostAsJsonAsync("/api/Appointments", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateAppointment_SameDayStartEnd_ReturnsNoContent()
    {
        var start = DateTime.UtcNow.AddDays(10);
        var request = new { PatientId = 2, DoctorId = 2, StartTime = start, EndTime = start.AddMinutes(30), Status = "Quick" };
        var response = await client.PostAsJsonAsync("/api/Appointments", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task UpdateAppointmentStatus_AfterCreate_ReturnsNoContent()
    {
        await client.PostAsJsonAsync("/api/Appointments", new { PatientId = 3, DoctorId = 3, StartTime = DateTime.UtcNow.AddDays(6), EndTime = DateTime.UtcNow.AddDays(6).AddHours(1), Status = "Pending" });
        var appointments = await client.GetFromJsonAsync<List<Appointment>>("/api/Appointments");
        var id = appointments!.First().Id;
        var response = await client.PatchAsJsonAsync($"/api/Appointments/{id}/status", new { Status = "Done" });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateAppointment_FarFutureDate_ReturnsNoContent()
    {
        var request = new { PatientId = 4, DoctorId = 4, StartTime = DateTime.UtcNow.AddYears(1), EndTime = DateTime.UtcNow.AddYears(1).AddHours(1), Status = "Future" };
        var response = await client.PostAsJsonAsync("/api/Appointments", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateAppointment_LongStatusString_ReturnsNoContent()
    {
        var request = new { PatientId = 5, DoctorId = 5, StartTime = DateTime.UtcNow.AddDays(7), EndTime = DateTime.UtcNow.AddDays(7).AddHours(2), Status = new string('S', 100) };
        var response = await client.PostAsJsonAsync("/api/Appointments", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task GetAllAppointments_AfterCreatingThree_ReturnsThree()
    {
        await client.PostAsJsonAsync("/api/Appointments", new { PatientId = 1, DoctorId = 1, StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(1).AddHours(1), Status = "A" });
        await client.PostAsJsonAsync("/api/Appointments", new { PatientId = 2, DoctorId = 2, StartTime = DateTime.UtcNow.AddDays(2), EndTime = DateTime.UtcNow.AddDays(2).AddHours(1), Status = "B" });
        await client.PostAsJsonAsync("/api/Appointments", new { PatientId = 3, DoctorId = 3, StartTime = DateTime.UtcNow.AddDays(3), EndTime = DateTime.UtcNow.AddDays(3).AddHours(1), Status = "C" });
        var appointments = await client.GetFromJsonAsync<List<Appointment>>("/api/Appointments");
        Assert.That(appointments!.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task UpdateStatus_NonExistentAppointment_DoesNotReturnServerError()
    {
        var response = await client.PatchAsJsonAsync("/api/Appointments/9999/status", new { Status = "X" });
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task CreateAppointment_DoctorIdZero_ReturnsNoContent()
    {
        var request = new { PatientId = 0, DoctorId = 0, StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(1).AddHours(1), Status = "Zero" };
        var response = await client.PostAsJsonAsync("/api/Appointments", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
}
