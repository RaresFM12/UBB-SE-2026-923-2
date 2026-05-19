using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.Web.Models;

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

        public async Task<IActionResult> Index(int? selectedDoctorId, DateTime? startDate, DateTime? endDate)
        {
            var doctors = _shiftSwapService.GetAllDoctors();
            ViewBag.Doctors = doctors;

            int? effectiveDoctorId = selectedDoctorId;
            if (!effectiveDoctorId.HasValue && !User.IsInRole("Admin"))
                effectiveDoctorId = GetCurrentDoctorStaffId();

            ViewBag.SelectedDoctorId = effectiveDoctorId;

            var rangeStart = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var rangeEnd = endDate ?? rangeStart.AddMonths(1).AddDays(-1);

            ViewBag.StartDate = rangeStart.ToString("yyyy-MM-dd");
            ViewBag.EndDate = rangeEnd.ToString("yyyy-MM-dd");

            if (!effectiveDoctorId.HasValue)
                return View(new DoctorScheduleViewModel());

            var shifts = _shiftSwapService.GetFutureShiftsForStaff(effectiveDoctorId.Value);
            var appointments = await _appointmentService.GetAppointmentsInRangeAsync(effectiveDoctorId.Value, rangeStart, rangeEnd);

            return View(new DoctorScheduleViewModel
            {
                Shifts = shifts.Where(s => s.StartTime >= rangeStart && s.StartTime <= rangeEnd).ToList(),
                Appointments = appointments.ToList()
            });
        }
    }
}