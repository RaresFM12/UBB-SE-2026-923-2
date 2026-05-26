using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.Web.ViewModels;

namespace UBB_SE_2026_923_2.Web.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class ShiftSwapController : Controller
    {
        private readonly IShiftSwapService _shiftSwapService;

        public ShiftSwapController(IShiftSwapService shiftSwapService)
        {
            _shiftSwapService = shiftSwapService;
        }

        private int? GetCurrentStaffId()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                return null;
            }

            var doctors = _shiftSwapService.GetAllDoctors();
            var matchingDoctor = doctors.FirstOrDefault(doctor => doctor.Email == userEmail);
            return matchingDoctor?.StaffID;
        }

        private int? ResolveSelectedDoctorId(int? selectedDoctorId, IReadOnlyList<Doctor> doctors)
        {
            if (selectedDoctorId.HasValue && doctors.Any(doctor => doctor.StaffID == selectedDoctorId.Value))
            {
                return selectedDoctorId.Value;
            }

            var currentStaffId = GetCurrentStaffId();
            if (currentStaffId.HasValue && doctors.Any(doctor => doctor.StaffID == currentStaffId.Value))
            {
                return currentStaffId.Value;
            }

            return doctors.FirstOrDefault()?.StaffID;
        }

        public ActionResult Index(int? selectedDoctorId, int? selectedShiftId)
        {
            var doctors = _shiftSwapService.GetAllDoctors();
            var staffId = ResolveSelectedDoctorId(selectedDoctorId, doctors);
            if (staffId == null)
            {
                return View(new ShiftSwapIndexViewModel
                {
                    Doctors = doctors,
                    StatusMessage = "No doctors found in database.",
                });
            }

            var allSwaps = _shiftSwapService.GetAllShiftSwapRequests();

            var shiftSwapIndexViewModel = new ShiftSwapIndexViewModel
            {
                Doctors = doctors,
                FutureShifts = _shiftSwapService.GetFutureShiftsForStaff(staffId.Value),
                SelectedDoctorId = staffId.Value,
                SelectedShiftId = selectedShiftId,
                StatusMessage = TempData["StatusMessage"]?.ToString() ?? string.Empty,
                PendingShiftIds = allSwaps
                    .Where(shift => shift.Requester?.StaffID == staffId.Value && shift.Status == ShiftSwapRequestStatus.PENDING)
                    .Select(shift => shift.Shift?.Id ?? 0)
                    .ToHashSet(),
            };

            if (selectedShiftId.HasValue)
            {
                bool alreadyRequested = allSwaps.Any(s =>
                    s.Shift?.Id == selectedShiftId.Value &&
                    s.Requester?.StaffID == staffId.Value &&
                    s.Status == ShiftSwapRequestStatus.PENDING);

                shiftSwapIndexViewModel.AlreadyRequested = alreadyRequested;

                if (!alreadyRequested)
                {
                    shiftSwapIndexViewModel.EligibleColleagues = _shiftSwapService
                        .GetEligibleSwapColleaguesForShift(staffId.Value, selectedShiftId.Value, out var error);

                    if (!string.IsNullOrEmpty(error))
                    {
                        shiftSwapIndexViewModel.StatusMessage = error;
                    }
                }
            }

            return View(shiftSwapIndexViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestSwap(int? selectedDoctorId, int shiftId, int colleagueId)
        {
            var doctors = _shiftSwapService.GetAllDoctors();
            var staffId = ResolveSelectedDoctorId(selectedDoctorId, doctors);
            if (staffId == null)
            {
                TempData["StatusMessage"] = "Could not find your staff profile.";
                return RedirectToAction(nameof(Index));
            }

            _shiftSwapService.RequestShiftSwap(staffId.Value, shiftId, colleagueId, out var message);
            TempData["StatusMessage"] = message;
            return RedirectToAction(nameof(Index), new { selectedDoctorId = staffId.Value, selectedShiftId = shiftId });
        }

        public ActionResult Incoming(int? selectedDoctorId)
        {
            var doctors = _shiftSwapService.GetAllDoctors();
            var staffId = ResolveSelectedDoctorId(selectedDoctorId, doctors);
            if (staffId == null)
            {
                return View(new IncomingSwapRequestsViewModel
                {
                    Doctors = doctors,
                    StatusMessage = "No doctors found in database.",
                });
            }

            var requests = _shiftSwapService.GetIncomingSwapRequests(staffId.Value);
            return View(new IncomingSwapRequestsViewModel
            {
                Doctors = doctors,
                Requests = requests,
                SelectedDoctorId = staffId.Value,
                StatusMessage = TempData["StatusMessage"]?.ToString() ?? string.Empty,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Accept(int swapId, int? selectedDoctorId)
        {
            var doctors = _shiftSwapService.GetAllDoctors();
            var staffId = ResolveSelectedDoctorId(selectedDoctorId, doctors);
            if (staffId == null || swapId <= 0)
            {
                TempData["StatusMessage"] = "Invalid request.";
                return RedirectToAction(nameof(Incoming), new { selectedDoctorId });
            }

            _shiftSwapService.AcceptSwapRequest(swapId, staffId.Value, out var message);
            TempData["StatusMessage"] = message;
            return RedirectToAction(nameof(Incoming), new { selectedDoctorId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(int swapId, int? selectedDoctorId)
        {
            var doctors = _shiftSwapService.GetAllDoctors();
            var staffId = ResolveSelectedDoctorId(selectedDoctorId, doctors);
            if (staffId == null || swapId <= 0)
            {
                TempData["StatusMessage"] = "Invalid request.";
                return RedirectToAction(nameof(Incoming), new { selectedDoctorId });
            }

            _shiftSwapService.RejectSwapRequest(swapId, staffId.Value, out var message);
            TempData["StatusMessage"] = message;
            return RedirectToAction(nameof(Incoming), new { selectedDoctorId });
        }
    }
}
