using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.Web.Controllers
{
    [Authorize(Roles = "Pharmacist,Admin")]
    public class PharmacyScheduleController : Controller
    {
        private readonly IPharmacyScheduleService _scheduleService;

        public PharmacyScheduleController(IPharmacyScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        private int? GetCurrentPharmacistId()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail)) return null;
            var pharmacists = _scheduleService.GetPharmacists();
            var matchingPharmacist = pharmacists.FirstOrDefault(p => p.Email == userEmail);
            return matchingPharmacist?.StaffID;
        }

        public async Task<IActionResult> Index(
            int? selectedPharmacistId,
            DateTime? selectedDate,
            string mode = "Daily",
            string nav = null)
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

            var baseDate = selectedDate ?? DateTime.Today;

            if (nav == "prev")
                baseDate = mode == "Weekly" ? baseDate.AddDays(-7) : baseDate.AddDays(-1);
            else if (nav == "next")
                baseDate = mode == "Weekly" ? baseDate.AddDays(7) : baseDate.AddDays(1);
            else if (nav == "today")
                baseDate = DateTime.Today;

            DateTime rangeStart, rangeEnd;
            if (mode == "Weekly")
            {
                int diff = (7 + (baseDate.DayOfWeek - DayOfWeek.Monday)) % 7;
                rangeStart = baseDate.AddDays(-diff).Date;
                rangeEnd = rangeStart.AddDays(6);
            }
            else
            {
                rangeStart = baseDate.Date;
                rangeEnd = baseDate.Date;
            }

            ViewBag.SelectedDate = baseDate.ToString("yyyy-MM-dd");
            ViewBag.SelectedDateText = mode == "Weekly"
                ? $"{rangeStart:dd MMM} - {rangeEnd:dd MMM yyyy}"
                : baseDate.ToString("dd MMM yyyy");
            ViewBag.Mode = mode;
            ViewBag.PreviousButtonText = mode == "Weekly" ? "Previous Week" : "Previous";
            ViewBag.NextButtonText = mode == "Weekly" ? "Next Week" : "Next";

            if (effectiveStaffId == null)
                return View(new List<Shift>());

            var shifts = await _scheduleService.GetShiftsAsync(effectiveStaffId.Value, rangeStart, rangeEnd.AddDays(1).AddSeconds(-1));
            return View(shifts);
        }
    }
}