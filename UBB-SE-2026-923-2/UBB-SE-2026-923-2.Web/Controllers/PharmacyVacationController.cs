using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.Web.Controllers
{
    [Authorize(Roles = "Pharmacist,Admin")]
    public class PharmacyVacationController : Controller
    {
        private readonly IPharmacyVacationService _vacationService;

        public PharmacyVacationController(IPharmacyVacationService vacationService)
        {
            _vacationService = vacationService;
        }

        // Același helper ca la Schedule pentru a găsi ID-ul farmacistului logat
        private int? GetCurrentPharmacistId()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail)) return null;

            var pharmacists = _vacationService.GetPharmacists();
            var matchingPharmacist = pharmacists.FirstOrDefault(p => p.ContactInfo == userEmail);
            return matchingPharmacist?.StaffID;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(DateTime startDate, DateTime endDate)
        {
            var staffId = GetCurrentPharmacistId();
            if (staffId == null)
            {
                ViewBag.ErrorMessage = "Could not find your staff profile.";
                return View();
            }

            try
            {
                // Încercăm să înregistrăm vacanța prin serviciul tău
                _vacationService.RegisterVacation(staffId.Value, startDate, endDate);

                // Dacă merge, afișăm mesaj de succes
                ViewBag.SuccessMessage = "Vacation successfully registered!";
            }
            catch (Exception ex)
            {
                // Dacă serviciul aruncă eroare (ex: Overlap la ture), o prindem și o afișăm pe ecran
                ViewBag.ErrorMessage = ex.Message;
            }

            return View();
        }
    }
}