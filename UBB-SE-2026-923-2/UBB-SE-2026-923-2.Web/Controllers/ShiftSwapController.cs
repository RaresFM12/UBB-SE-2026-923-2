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
            if (string.IsNullOrEmpty(userEmail)) return null;

            var doctors = _shiftSwapService.GetAllDoctors();
            var matchingDoctor = doctors.FirstOrDefault(doctor => doctor.Email == userEmail);
            return matchingDoctor?.StaffID;
        }

        public ActionResult Index(int? selectedShiftId)
        {
            var staffId = GetCurrentStaffId();
            if (staffId == null)
                return View(new ShiftSwapIndexViewModel());

            var allSwaps = _shiftSwapService.GetAllShiftSwapRequests(); // you need to expose this

            var shiftSwapIndexViewModel = new ShiftSwapIndexViewModel
            {
                FutureShifts = _shiftSwapService.GetFutureShiftsForStaff(staffId.Value),
                SelectedShiftId = selectedShiftId,
                StatusMessage = TempData["StatusMessage"]?.ToString() ?? string.Empty,
                PendingShiftIds = allSwaps
                    .Where(shift => shift.Requester?.StaffID == staffId.Value && shift.Status == ShiftSwapRequestStatus.PENDING)
                    .Select(shift => shift.Shift?.Id ?? 0)
                    .ToHashSet()
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
                        shiftSwapIndexViewModel.StatusMessage = error;
                }
            }

            return View(shiftSwapIndexViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestSwap(int shiftId, int colleagueId)
        {
            var staffId = GetCurrentStaffId();
            if (staffId == null)
            {
                TempData["StatusMessage"] = "Could not find your staff profile.";
                return RedirectToAction(nameof(Index));
            }

            _shiftSwapService.RequestShiftSwap(staffId.Value, shiftId, colleagueId, out var message);
            TempData["StatusMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Incoming()
        {
            var staffId = GetCurrentStaffId();
            if (staffId == null)
            {
                ViewBag.StatusMessage = "Could not find your staff profile.";
                return View(new List<ShiftSwapRequest>());
            }

            ViewBag.StatusMessage = TempData["StatusMessage"]?.ToString() ?? string.Empty;
            var requests = _shiftSwapService.GetIncomingSwapRequests(staffId.Value);
            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Accept(int swapId, int? selectedDoctorId)
        {
            var staffId = GetCurrentStaffId();
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
            var staffId = GetCurrentStaffId();
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