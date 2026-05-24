using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.Web.Controllers
{
    // Restricting to Pharmacists and Admins as per standard pharmacy scheduling logic
    [Authorize(Roles = "Pharmacist,Admin")]
    public class PharmacyScheduleController : Controller
    {
        private readonly IPharmacyScheduleService _scheduleService;

        public PharmacyScheduleController(IPharmacyScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        // Helper to get the logged-in Pharmacist's ID, matching Paul's style
        private int? GetCurrentPharmacistId()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail)) return null;

            var pharmacists = _scheduleService.GetPharmacists();
            var matchingPharmacist = pharmacists.FirstOrDefault(p => p.Email == userEmail); // ← was ContactInfo
            return matchingPharmacist?.StaffID;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? selectedPharmacistId)
        {
            var pharmacists = _scheduleService.GetPharmacists();
            ViewBag.Pharmacists = pharmacists;
            ViewBag.IsAdmin = User.IsInRole("Admin") || User.IsInRole("Pharmacist");

            int? effectiveStaffId;

            if (User.IsInRole("Admin") || User.IsInRole("Pharmacist"))
                effectiveStaffId = selectedPharmacistId ?? GetCurrentPharmacistId();
            else
                effectiveStaffId = GetCurrentPharmacistId();

            ViewBag.SelectedPharmacistId = effectiveStaffId;

            if (effectiveStaffId == null)
            {
                ViewBag.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                ViewBag.EndDate = DateTime.Now.AddMonths(1).ToString("yyyy-MM-dd");
                return View(new List<Shift>());
            }

            var rangeStart = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var rangeEnd = endDate ?? rangeStart.AddMonths(1).AddDays(-1);

            ViewBag.StartDate = rangeStart.ToString("yyyy-MM-dd");
            ViewBag.EndDate = rangeEnd.ToString("yyyy-MM-dd");

            var shifts = await _scheduleService.GetShiftsAsync(effectiveStaffId.Value, rangeStart, rangeEnd);
            return View(shifts);
        }
    }
}