using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.WebApi.Controllers;

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
        var appointments = await repository.GetAllAppointmentsAsync();
        return Ok(appointments);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
    {
        await repository.AddAppointmentAsync(
            request.PatientId,
            request.DoctorId,
            request.StartTime,
            request.EndTime,
            request.Status);
        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        await repository.UpdateAppointmentStatusAsync(id, request.Status);
        return NoContent();
    }

    public record CreateAppointmentRequest(
        int PatientId,
        int DoctorId,
        DateTime StartTime,
        DateTime EndTime,
        string Status);

    public record UpdateStatusRequest(string Status);
}
