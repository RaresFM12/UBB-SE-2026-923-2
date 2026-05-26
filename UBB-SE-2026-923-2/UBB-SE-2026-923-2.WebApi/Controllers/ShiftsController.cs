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

    [HttpGet("{id:int}")]
    public ActionResult<Shift> GetById(int id)
    {
        var shift = this.repository.GetAllShifts().FirstOrDefault(shiftNew => shiftNew.Id == id);
        if (shift is null)
            return this.NotFound();
        return this.Ok(shift);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateShiftRequest request)
    {
        var shift = new Shift
        {
            Staff = new Staff { StaffID = request.StaffId },
            Location = request.Location ?? string.Empty,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = request.Status,
        };
        this.repository.AddShift(shift);
        return this.NoContent();
    }

    [HttpPatch("{shiftId:int}/status")]
    public IActionResult UpdateStatus(int shiftId, [FromBody] UpdateShiftStatusRequest request)
    {
        this.repository.UpdateShiftStatus(shiftId, request.Status);
        return this.NoContent();
    }

    [HttpPatch("{shiftId:int}/staff")]
    public IActionResult UpdateStaff(int shiftId, [FromBody] UpdateShiftStaffRequest request)
    {
        this.repository.UpdateShiftStaffId(shiftId, request.StaffId);
        return this.NoContent();
    }

    [HttpDelete("{shiftId:int}")]
    public IActionResult Delete(int shiftId)
    {
        this.repository.DeleteShift(shiftId);
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