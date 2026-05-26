namespace UBB_SE_2026_923_2.Web.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.Web.ViewModels;

[Authorize]
public class ShiftManagementController : Controller
{
    public const string DefaultDateTimeFormat = "g";

    private const string AdminManagerRoles = "Admin,Manager";
    private const string SalaryRoles = "Admin,Manager,Pharmacist,Doctor";
    private static readonly IReadOnlyList<string> ShiftLocations = new[]
    {
        "ER",
        "Ward A",
        "Ward B",
        "Cardiology",
        "Surgery",
        "Neurology",
        "Pediatry",
        "Oncology",
        "Pharmacy",
    };

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
    [Authorize(Roles = AdminManagerRoles)]
    public IActionResult Index(DateTime? shiftDate)
    {
        var selectedDate = shiftDate?.Date ?? DateTime.Today;
        ViewBag.SelectedShiftDate = selectedDate.ToString("yyyy-MM-dd");
        var shifts = this.shiftManagementService.GetDailyShifts(selectedDate)
            .OrderBy(shift => shift.StartTime)
            .ToList();
        return this.View(shifts);
    }

    [HttpGet]
    [Authorize(Roles = AdminManagerRoles)]
    public IActionResult Create(string? location = null, string? qualification = null)
    {
        return this.View(this.BuildShiftCreationViewModel(location, qualification));
    }

    [HttpGet]
    [Authorize(Roles = AdminManagerRoles)]
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
    [Authorize(Roles = AdminManagerRoles)]
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
    [Authorize(Roles = AdminManagerRoles)]
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
    [Authorize(Roles = AdminManagerRoles)]
    public IActionResult Create(
        int staffId,
        DateTime? shiftDate,
        TimeSpan? startTime,
        TimeSpan? endTime,
        string location,
        string? qualification)
    {
        var model = this.BuildShiftCreationViewModel(location, qualification);

        if (staffId == 0 || string.IsNullOrWhiteSpace(location) || !shiftDate.HasValue || !startTime.HasValue || !endTime.HasValue)
        {
            this.ModelState.AddModelError(string.Empty, "Please fill all the fields of the form!");
            return this.View(model);
        }

        if (!this.shiftManagementService.ValidateShiftTimes(startTime.Value, endTime.Value))
        {
            this.ModelState.AddModelError(string.Empty, "Error: End hour must be chronologically after the start hour!");
            return this.View(model);
        }

        bool IsMatchingStaff(IStaff staffMember) => staffMember.StaffID == staffId;
        var staff = model.QualifiedStaff.FirstOrDefault(IsMatchingStaff);

        if (staff == null)
        {
            this.ModelState.AddModelError(string.Empty, "Staff member not found.");
            return this.View(model);
        }

        var date = shiftDate.Value.Date;
        var startDateTime = date.Add(startTime.Value);
        var endDateTime = date.Add(endTime.Value);

        bool isSuccess = this.shiftManagementService.TryAddShift(staff, startDateTime, endDateTime, location);
        if (!isSuccess)
        {
            this.ModelState.AddModelError(string.Empty, "Failed to add shift. Staff might be overlapping shifts.");
            return this.View(model);
        }

        this.TempData["ShiftStatusMessage"] = "The shift was scheduled successfuly!";
        return this.RedirectToAction(nameof(Create));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AdminManagerRoles)]
    public IActionResult Cancel(int shiftId, DateTime? shiftDate)
    {
        this.shiftManagementService.CancelShift(shiftId);
        this.TempData["ShiftStatusMessage"] = $"The shift #{shiftId} was cancelled.";
        return this.RedirectToShiftIndex(shiftDate);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AdminManagerRoles)]
    public IActionResult CancelFromCreate(int shiftId)
    {
        this.shiftManagementService.CancelShift(shiftId);
        this.TempData["ShiftStatusMessage"] = $"The shift #{shiftId} was cancelled.";
        return this.RedirectToAction(nameof(Create));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AdminManagerRoles)]
    public IActionResult Activate(int shiftId, DateTime? shiftDate)
    {
        this.shiftManagementService.SetShiftActive(shiftId);
        this.TempData["ShiftStatusMessage"] = $"The shift #{shiftId} was marked as active.";
        return this.RedirectToShiftIndex(shiftDate);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AdminManagerRoles)]
    public IActionResult AutoReassign(int shiftId, DateTime? shiftDate)
    {
        var selectedDate = shiftDate?.Date ?? DateTime.Today;
        bool IsMatchingShift(Shift shift) => shift.Id == shiftId;
        var shift = this.shiftManagementService.GetDailyShifts(selectedDate).FirstOrDefault(IsMatchingShift)
            ?? this.salaryComputationService.GetAllShifts().FirstOrDefault(IsMatchingShift);

        if (shift == null)
        {
            this.TempData["ShiftStatusMessage"] = "Shift not found.";
            return this.RedirectToShiftIndex(shiftDate);
        }

        var replacement = this.shiftManagementService.FindStaffReplacements(shift).FirstOrDefault();
        if (replacement != null && this.shiftManagementService.ReassignShift(shift, replacement))
        {
            this.TempData["ShiftStatusMessage"] = "The automatic searching of a replacement has been triggered.";
        }
        else
        {
            this.TempData["ShiftStatusMessage"] = "No eligible replacement was found.";
        }

        return this.RedirectToShiftIndex(shiftDate);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AdminManagerRoles)]
    public IActionResult ActivateFromCreate(int shiftId)
    {
        this.shiftManagementService.SetShiftActive(shiftId);
        this.TempData["ShiftStatusMessage"] = $"The shift #{shiftId} was marked as active.";
        return this.RedirectToAction(nameof(Create));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AdminManagerRoles)]
    public IActionResult AutoReassignFromCreate(int shiftId)
    {
        bool IsMatchingShift(Shift shift) => shift.Id == shiftId;
        var shift = this.shiftManagementService.GetDailyShifts(DateTime.Today).FirstOrDefault(IsMatchingShift)
            ?? this.shiftManagementService.GetWeeklyShifts(DateTime.Today).FirstOrDefault(IsMatchingShift);

        if (shift != null)
        {
            var replacement = this.shiftManagementService.FindStaffReplacements(shift).FirstOrDefault();
            if (replacement != null && this.shiftManagementService.ReassignShift(shift, replacement))
            {
                this.TempData["ShiftStatusMessage"] = "The automatic searching of a replacement has been triggered.";
            }
            else
            {
                this.TempData["ShiftStatusMessage"] = "No eligible replacement was found.";
            }
        }

        return this.RedirectToAction(nameof(Create));
    }

    [HttpGet]
    [Authorize(Roles = SalaryRoles)]
    public IActionResult Salary()
    {
        this.ViewBag.StaffList = this.salaryComputationService.GetAllStaff();
        return this.View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = SalaryRoles)]
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

    private ShiftCreationViewModel BuildShiftCreationViewModel(string? location, string? qualification)
    {
        var qualifications = string.IsNullOrWhiteSpace(location)
            ? new List<string>()
            : this.shiftManagementService.GetSpecializationsAndCertificationsForLocation(location);

        var staff = !string.IsNullOrWhiteSpace(location) && !string.IsNullOrWhiteSpace(qualification)
            ? this.shiftManagementService.GetFilteredStaff(location, qualification)
            : new List<IStaff>();

        return new ShiftCreationViewModel
        {
            SelectedLocation = location,
            SelectedQualification = qualification,
            Locations = ShiftLocations,
            Qualifications = qualifications,
            QualifiedStaff = staff,
            TodayShifts = this.shiftManagementService.GetDailyShifts(DateTime.Today).OrderBy(shift => shift.StartTime).ToList(),
        };
    }

    private IActionResult RedirectToShiftIndex(DateTime? shiftDate)
    {
        var selectedDate = shiftDate?.Date ?? DateTime.Today;
        return this.RedirectToAction(nameof(Index), new { shiftDate = selectedDate.ToString("yyyy-MM-dd") });
    }
}
