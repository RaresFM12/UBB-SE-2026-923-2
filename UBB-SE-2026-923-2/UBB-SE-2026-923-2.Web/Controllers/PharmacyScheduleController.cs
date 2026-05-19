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
            var matchingPharmacist = pharmacists.FirstOrDefault(p => p.ContactInfo == userEmail); // Assuming ContactInfo holds email
            return matchingPharmacist?.StaffID;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            var staffId = GetCurrentPharmacistId();
            if (staffId == null)
            {
                ViewBag.StatusMessage = "Could not find your staff profile.";
                return View(new List<Shift>());
            }

            // Default to seeing the schedule for the current month if no dates are provided
            var rangeStart = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var rangeEnd = endDate ?? rangeStart.AddMonths(1).AddDays(-1);

            ViewBag.StartDate = rangeStart.ToString("yyyy-MM-dd");
            ViewBag.EndDate = rangeEnd.ToString("yyyy-MM-dd");

            var shifts = await _scheduleService.GetShiftsAsync(staffId.Value, rangeStart, rangeEnd);

            return View(shifts);
        }
    }
}