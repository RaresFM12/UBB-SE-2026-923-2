using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StaffController : ControllerBase
{
    private readonly IStaffRepository staffRepository;
    private readonly IShiftManagementStaffRepository shiftManagementStaffRepository;
    private readonly IPharmacyStaffRepository pharmacyStaffRepository;

    public StaffController(
        IStaffRepository staffRepository,
        IShiftManagementStaffRepository shiftManagementStaffRepository,
        IPharmacyStaffRepository pharmacyStaffRepository)
    {
        this.staffRepository = staffRepository;
        this.shiftManagementStaffRepository = shiftManagementStaffRepository;
        this.pharmacyStaffRepository = pharmacyStaffRepository;
    }

    [HttpGet]
    public ActionResult<List<Staff>> GetAll()
    {
        // The repository contract is List<IStaff>, but the underlying instances are
        // always concrete Staff (Doctor or Pharmacyst). Cast back to the base class
        // so System.Text.Json's JsonDerivedType polymorphism kicks in and emits the
        // correct $type discriminator for each element.
        var staff = staffRepository.LoadAllStaff().Cast<Staff>().ToList();
        return Ok(staff);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Staff> GetById(int id)
    {
        var staff = staffRepository.GetStaffById(id) as Staff;
        if (staff is null)
        {
            return NotFound();
        }

        return Ok(staff);
    }

    [HttpGet("doctors")]
    public async Task<ActionResult<IReadOnlyList<DoctorSummary>>> GetDoctors()
    {
        var doctors = await staffRepository.GetAllDoctorsAsync();
        var summaries = doctors
            .Select(d => new DoctorSummary(d.DoctorId, d.FirstName, d.LastName))
            .ToList();
        return Ok(summaries);
    }

    [HttpGet("pharmacists")]
    public ActionResult<List<Pharmacyst>> GetPharmacists()
    {
        return Ok(pharmacyStaffRepository.GetPharmacists());
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        await staffRepository.UpdateStatusAsync(id, request.Status);
        return NoContent();
    }

    [HttpPatch("{id:int}/availability")]
    public IActionResult UpdateAvailability(int id, [FromBody] UpdateAvailabilityRequest request)
    {
        shiftManagementStaffRepository.UpdateStaffAvailability(id, request.IsAvailable, request.Status);
        return NoContent();
    }

    public record UpdateStatusRequest(string Status);

    public record UpdateAvailabilityRequest(bool IsAvailable, DoctorStatus Status);
}
