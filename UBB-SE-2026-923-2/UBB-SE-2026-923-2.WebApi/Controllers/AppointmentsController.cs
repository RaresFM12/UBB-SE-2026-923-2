namespace UBB_SE_2026_923_2.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentRepository repository;

    public AppointmentsController(IAppointmentRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Appointment>>> GetAll()
    {
        var appointments = await this.repository.GetAllAppointmentsAsync();
        return this.Ok(appointments);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
    {
        await this.repository.AddAppointmentAsync(
            request.PatientId,
            request.DoctorId,
            request.StartTime,
            request.EndTime,
            request.Status);
        return this.NoContent();
    }

    [HttpPatch("{appointmentId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int appointmentId, [FromBody] UpdateStatusRequest request)
    {
        await this.repository.UpdateAppointmentStatusAsync(appointmentId, request.Status);
        return this.NoContent();
    }

    public record CreateAppointmentRequest(
        int PatientId,
        int DoctorId,
        DateTime StartTime,
        DateTime EndTime,
        string Status);

    public record UpdateStatusRequest(string Status);
}