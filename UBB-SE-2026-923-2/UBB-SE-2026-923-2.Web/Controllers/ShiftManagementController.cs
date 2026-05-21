namespace UBB_SE_2026_923_2.Web.Controllers;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;

[Authorize(Roles = AllowedRoles)]
public class ShiftManagementController : Controller
{
    public const string DefaultDateTimeFormat = "g";

    private const string AllowedRoles = "Admin,Manager";

    private readonly IShiftManagementService shiftManagementService;
    private readonly ISalaryComputationService salaryComputationService;

    public ShiftManagementController(
        IShiftManagementService shiftManagementService,
        ISalaryComputationService salaryComputationService)
    {
        this.shiftManagementService = shiftManagementService;
        this.salaryComputationService = salaryComputationService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var shifts = this.salaryComputationService.GetAllShifts();
        return this.View(shifts);
    }

    [HttpGet]
    public IActionResult Create()
    {
        this.ViewBag.StaffList = this.salaryComputationService.GetAllStaff();
        return this.View();
    }

    [HttpGet]
    public IActionResult Details(int shiftId)
    {
        bool IsMatchingShift(Shift shift) => shift.Id == shiftId;
        var shift = this.salaryComputationService.GetAllShifts().FirstOrDefault(IsMatchingShift);

        if (shift == null)
        {
            return this.NotFound();
        }

        return this.View(shift);
    }

    [HttpGet]
    public IActionResult Edit(int shiftId)
    {
        bool IsMatchingShift(Shift shift) => shift.Id == shiftId;
        var shift = this.salaryComputationService.GetAllShifts().FirstOrDefault(IsMatchingShift);

        if (shift == null)
        {
            return this.NotFound();
        }

        this.ViewBag.StaffList = this.salaryComputationService.GetAllStaff();
        return this.View(shift);
    }

    [HttpGet]
    public IActionResult Delete(int shiftId)
    {
        bool IsMatchingShift(Shift shift) => shift.Id == shiftId;
        var shift = this.salaryComputationService.GetAllShifts().FirstOrDefault(IsMatchingShift);

        if (shift == null)
        {
            return this.NotFound();
        }

        return this.View(shift);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(int staffId, DateTime startTime, DateTime endTime, string location)
    {
        bool IsMatchingStaff(IStaff staffMember) => staffMember.StaffID == staffId;
        var staff = this.salaryComputationService.GetAllStaff().FirstOrDefault(IsMatchingStaff);

        if (staff == null)
        {
            this.ModelState.AddModelError(string.Empty, "Staff member not found.");
            this.ViewBag.StaffList = this.salaryComputationService.GetAllStaff();
            return this.View();
        }

        bool isSuccess = this.shiftManagementService.TryAddShift(staff, startTime, endTime, location);
        if (!isSuccess)
        {
            this.ModelState.AddModelError(string.Empty, "Failed to add shift. Staff might be overlapping shifts.");
            this.ViewBag.StaffList = this.salaryComputationService.GetAllStaff();
            return this.View();
        }

        return this.RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancel(int shiftId)
    {
        this.shiftManagementService.CancelShift(shiftId);
        return this.RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Activate(int shiftId)
    {
        this.shiftManagementService.SetShiftActive(shiftId);
        return this.RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Pharmacist,Doctor")]
    public IActionResult Salary()
    {
        this.ViewBag.StaffList = this.salaryComputationService.GetAllStaff();
        return this.View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager,Pharmacist,Doctor")]
    public async Task<IActionResult> ComputeSalary(int staffId, int month, int year)
    {
        this.ViewBag.StaffList = this.salaryComputationService.GetAllStaff();

        bool IsMatchingStaff(IStaff staffMember) => staffMember.StaffID == staffId;
        var staff = this.salaryComputationService.GetAllStaff().FirstOrDefault(IsMatchingStaff);

        if (staff == null)
        {
            this.ModelState.AddModelError(string.Empty, "Staff not found.");
            return this.View("Salary");
        }

        bool IsStaffShiftInTargetMonth(Shift shift) =>
            shift.AppointedStaff.StaffID == staffId &&
            shift.StartTime.Month == month &&
            shift.StartTime.Year == year;

        var allShifts = this.salaryComputationService.GetAllShifts();
        var monthlyShifts = allShifts.Where(IsStaffShiftInTargetMonth).ToList();

        double computedSalary = 0;

        if (staff is Doctor doctor)
        {
            computedSalary = await this.salaryComputationService.ComputeSalaryDoctorAsync(doctor, monthlyShifts, month, year);
        }
        else if (staff is Pharmacyst pharmacist)
        {
            computedSalary = await this.salaryComputationService.ComputeSalaryPharmacistAsync(pharmacist, monthlyShifts, month, year);
        }

        this.ViewBag.CalculatedSalary = computedSalary;
        this.ViewBag.SelectedStaffName = $"{staff.FirstName} {staff.LastName}";

        return this.View("Salary");
    }
}