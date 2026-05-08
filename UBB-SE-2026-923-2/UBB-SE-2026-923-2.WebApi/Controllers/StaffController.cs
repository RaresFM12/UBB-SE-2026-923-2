namespace UBB_SE_2026_923_2.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

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
        var staff = this.staffRepository.LoadAllStaff().Cast<Staff>().ToList();
        return this.Ok(staff);
    }

    [HttpGet("{staffId:int}")]
    public ActionResult<Staff> GetById(int staffId)
    {
        var staff = this.staffRepository.GetStaffById(staffId) as Staff;
        if (staff is null)
        {
            return this.NotFound();
        }

        return this.Ok(staff);
    }

    [HttpGet("doctors")]
    public async Task<ActionResult<IReadOnlyList<DoctorSummary>>> GetDoctors()
    {
        var doctors = await this.staffRepository.GetAllDoctorsAsync();
        var summaries = doctors
            .Select(doctor => new DoctorSummary(doctor.DoctorId, doctor.FirstName, doctor.LastName))
            .ToList();
        return this.Ok(summaries);
    }

    [HttpGet("pharmacists")]
    public ActionResult<List<Pharmacyst>> GetPharmacists()
    {
        return this.Ok(this.pharmacyStaffRepository.GetPharmacists());
    }

    [HttpPatch("{staffId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int staffId, [FromBody] UpdateStatusRequest request)
    {
        await this.staffRepository.UpdateStatusAsync(staffId, request.Status);
        return this.NoContent();
    }

    [HttpPatch("{staffId:int}/availability")]
    public IActionResult UpdateAvailability(int staffId, [FromBody] UpdateAvailabilityRequest request)
    {
        this.shiftManagementStaffRepository.UpdateStaffAvailability(staffId, request.IsAvailable, request.Status);
        return this.NoContent();
    }

    public record UpdateStatusRequest(string Status);

    public record UpdateAvailabilityRequest(bool IsAvailable, DoctorStatus Status);
}