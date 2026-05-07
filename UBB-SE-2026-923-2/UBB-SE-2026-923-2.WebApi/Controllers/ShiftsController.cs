namespace UBB_SE_2026_923_2.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

[ApiController]
[Route("api/[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftRepository repository;

    public ShiftsController(IShiftRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<Shift>> GetAll()
    {
        return this.Ok(this.repository.GetAllShifts());
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateShiftRequest request)
    {
        var shift = new Shift
        {
            StaffId = request.StaffId,
            Location = request.Location ?? string.Empty,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = request.Status,
        };
        this.repository.AddShift(shift);
        return this.NoContent();
    }

    [HttpPatch("{id:int}/status")]
    public IActionResult UpdateStatus(int id, [FromBody] UpdateShiftStatusRequest request)
    {
        this.repository.UpdateShiftStatus(id, request.Status);
        return this.NoContent();
    }

    [HttpPatch("{id:int}/staff")]
    public IActionResult UpdateStaff(int id, [FromBody] UpdateShiftStaffRequest request)
    {
        this.repository.UpdateShiftStaffId(id, request.StaffId);
        return this.NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        this.repository.DeleteShift(id);
        return this.NoContent();
    }

    public record CreateShiftRequest(
        int StaffId,
        string? Location,
        DateTime StartTime,
        DateTime EndTime,
        ShiftStatus Status);

    public record UpdateShiftStatusRequest(ShiftStatus Status);

    public record UpdateShiftStaffRequest(int StaffId);
}
