namespace UBB_SE_2026_923_2.Web.Controllers;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;

[Authorize(Roles = "Admin,Manager")]
public class ShiftManagementController : Controller
{
    private readonly IShiftManagementService shiftService;
    private readonly ISalaryComputationService salaryService;

    public ShiftManagementController(IShiftManagementService shiftService, ISalaryComputationService salaryService)
    {
        this.shiftService = shiftService;
        this.salaryService = salaryService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var shifts = this.salaryService.GetAllShifts();
        return this.View(shifts);
    }

    [HttpGet]
    public IActionResult Create()
    {
        this.ViewBag.StaffList = this.salaryService.GetAllStaff();
        return this.View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(int staffId, DateTime startTime, DateTime endTime, string location)
    {
        var staff = this.salaryService.GetAllStaff().FirstOrDefault(s => s.StaffID == staffId);
        if (staff == null)
        {
            this.ModelState.AddModelError("", "Staff member not found.");
            this.ViewBag.StaffList = this.salaryService.GetAllStaff();
            return this.View();
        }

        bool success = this.shiftService.TryAddShift(staff, startTime, endTime, location);
        if (!success)
        {
            this.ModelState.AddModelError("", "Failed to add shift. Staff might be overlapping shifts.");
            this.ViewBag.StaffList = this.salaryService.GetAllStaff();
            return this.View();
        }

        return this.RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancel(int id)
    {
        this.shiftService.CancelShift(id);
        return this.RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Activate(int id)
    {
        this.shiftService.SetShiftActive(id);
        return this.RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Salary()
    {
        this.ViewBag.StaffList = this.salaryService.GetAllStaff();
        return this.View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ComputeSalary(int staffId, int month, int year)
    {
        this.ViewBag.StaffList = this.salaryService.GetAllStaff();

        var staff = this.salaryService.GetAllStaff().FirstOrDefault(s => s.StaffID == staffId);
        if (staff == null)
        {
            this.ModelState.AddModelError("", "Staff not found.");
            return this.View("Salary");
        }

        var allShifts = this.salaryService.GetAllShifts();
        var monthlyShifts = allShifts.Where(s => s.AppointedStaff.StaffID == staffId
                                              && s.StartTime.Month == month
                                              && s.StartTime.Year == year).ToList();

        double computedSalary = 0;

        if (staff is Doctor doctor)
        {
            computedSalary = await this.salaryService.ComputeSalaryDoctorAsync(doctor, monthlyShifts, month, year);
        }
        else if (staff is Pharmacyst pharmacist)
        {
            computedSalary = await this.salaryService.ComputeSalaryPharmacistAsync(pharmacist, monthlyShifts, month, year);
        }

        this.ViewBag.CalculatedSalary = computedSalary;
        this.ViewBag.SelectedStaffName = $"{staff.FirstName} {staff.LastName}";

        return this.View("Salary");
    }
}