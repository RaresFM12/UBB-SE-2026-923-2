namespace UBB_SE_2026_923_2.Web.Controllers
{
    using System;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    [Authorize(Roles = "Admin")]
    public class FatigueAuditController : Controller
    {
        private readonly IFatigueAuditService fatigueAuditService;

        public FatigueAuditController(IFatigueAuditService fatigueAuditService)
        {
            this.fatigueAuditService = fatigueAuditService;
        }

        [HttpGet]
        public IActionResult Index(DateTime? weekStart)
        {
            var effectiveWeekStart = weekStart ?? DateTime.Today;
            var result = this.fatigueAuditService.RunAutoAudit(effectiveWeekStart);
            return this.View(result);
        }

        [HttpGet]
        public IActionResult Details(int id, DateTime? weekStart)
        {
            var effectiveWeekStart = weekStart ?? DateTime.Today;
            var result = this.fatigueAuditService.RunAutoAudit(effectiveWeekStart);
            var violation = result.Violations.FirstOrDefault(violationNew => violationNew.ShiftId == id);
            if (violation == null)
            {
                return this.NotFound();
            }

            var suggestion = result.Suggestions.FirstOrDefault(suggestionNew => suggestionNew.ShiftId == id);
            this.ViewBag.Suggestion = suggestion;
            this.ViewBag.WeekStart = effectiveWeekStart;
            return this.View(violation);
        }

        [HttpGet]
        public IActionResult Reassign(int shiftId, DateTime? weekStart)
        {
            var effectiveWeekStart = weekStart ?? DateTime.Today;
            var result = this.fatigueAuditService.RunAutoAudit(effectiveWeekStart);
            var suggestion = result.Suggestions.FirstOrDefault(suggestionNew => suggestionNew.ShiftId == shiftId);
            if (suggestion == null)
            {
                return this.NotFound();
            }

            return this.View(suggestion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reassign(int shiftId, int newStaffId)
        {
            var success = this.fatigueAuditService.ReassignShift(shiftId, newStaffId);
            if (!success)
            {
                this.TempData["Error"] = "Reassignment failed. Please verify the shift and staff IDs.";
                return this.RedirectToAction(nameof(this.Index));
            }

            this.TempData["Success"] = "Shift reassigned successfully.";
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Publish(DateTime? weekStart)
        {
            var effectiveWeekStart = weekStart ?? DateTime.Today;
            var result = this.fatigueAuditService.RunAutoAudit(effectiveWeekStart);
            if (!result.CanPublish)
            {
                this.TempData["Error"] = "Roster cannot be published while violations exist.";
                return this.RedirectToAction(nameof(this.Index), new { weekStart = effectiveWeekStart.ToString("yyyy-MM-dd") });
            }

            this.TempData["Success"] = $"The roster for the week of {effectiveWeekStart:dd MMM yyyy} has been published successfully.";
            return this.RedirectToAction(nameof(this.Index), new { weekStart = effectiveWeekStart.ToString("yyyy-MM-dd") });
        }
    }
}
