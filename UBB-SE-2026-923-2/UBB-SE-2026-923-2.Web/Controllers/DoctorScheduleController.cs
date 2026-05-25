using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.Web.ViewModels;

namespace UBB_SE_2026_923_2.Web.Controllers
{
    [Authorize(Roles = "Doctor,Admin")]
    public class DoctorScheduleController : Controller
    {
        private readonly IShiftSwapService _shiftSwapService;
        private readonly IDoctorAppointmentService _appointmentService;

        public DoctorScheduleController(IShiftSwapService shiftSwapService, IDoctorAppointmentService appointmentService)
        {
            _shiftSwapService = shiftSwapService;
            _appointmentService = appointmentService;
        }

        private int? GetCurrentDoctorStaffId()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail)) return null;
            var doctors = _shiftSwapService.GetAllDoctors();
            var matchingDoctor = doctors.FirstOrDefault(d => d.Email == userEmail);
            return matchingDoctor?.StaffID;
        }

        public async Task<IActionResult> Index(
            int? selectedDoctorId,
            DateTime? selectedDate,
            string mode = "Daily",
            string nav = null)
        {
            string errorMessage = null;
            List<Doctor> doctors = new();

            try
            {
                doctors = _shiftSwapService.GetAllDoctors();
            }
            catch (Exception exception)
            {
                errorMessage = $"Failed to load doctors: {exception.Message}";
            }

            ViewBag.Doctors = doctors;

            int? effectiveDoctorId = selectedDoctorId;
            if (!effectiveDoctorId.HasValue && !User.IsInRole("Admin"))
                effectiveDoctorId = GetCurrentDoctorStaffId();

            if (errorMessage == null && doctors.Count == 0)
                errorMessage = "No doctors available.";

            ViewBag.SelectedDoctorId = effectiveDoctorId;

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
                ? $"Week of {rangeStart:dd MMM yyyy}"
                : baseDate.ToString("dddd, dd MMM yyyy");
            ViewBag.Mode = mode;
            ViewBag.PreviousButtonText = mode == "Weekly" ? "Previous Week" : "Previous";
            ViewBag.NextButtonText = mode == "Weekly" ? "Next Week" : "Next";

            var filteredShifts = new List<Shift>();
            var appointments = new List<Appointment>();

            if (effectiveDoctorId.HasValue && errorMessage == null)
            {
                try
                {
                    var shifts = _shiftSwapService.GetFutureShiftsForStaff(effectiveDoctorId.Value);
                    var appointmentsResult = await _appointmentService.GetAppointmentsInRangeAsync(
                        effectiveDoctorId.Value, rangeStart, rangeEnd.AddDays(1).AddSeconds(-1));

                    appointments = appointmentsResult.ToList();
                    filteredShifts = shifts
                        .Where(s => s.StartTime.Date >= rangeStart && s.StartTime.Date <= rangeEnd)
                        .ToList();
                }
                catch (Exception exception)
                {
                    errorMessage = $"Failed to load schedule: {exception.Message}";
                }
            }

            ViewBag.ErrorMessage = errorMessage;
            ViewBag.IsEmpty = string.IsNullOrEmpty(errorMessage) && !filteredShifts.Any() && !appointments.Any();

            return View(new DoctorScheduleViewModel
            {
                Shifts = filteredShifts,
                Appointments = appointments
            });
        }
    }
}
