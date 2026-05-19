namespace UBB_SE_2026_923_2.Web.Controllers
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    [Authorize(Roles = AllowedRoles)]
    public class MedicalEvaluationsController : Controller
    {
        public const string DefaultDateTimeFormat = "g";
        private const string AllowedRoles = "Admin,Doctor";
        private const string ErrorMessageKey = "ErrorMessage";
        private const string WarningMessageKey = "WarningMessage";

        private readonly IMedicalEvaluationService medicalEvaluationService;
        private readonly IWellnessItemsService wellnessItemsService;

        public MedicalEvaluationsController(
            IMedicalEvaluationService medicalEvaluationService,
            IWellnessItemsService wellnessItemsService)
        {
            this.medicalEvaluationService = medicalEvaluationService;
            this.wellnessItemsService = wellnessItemsService;
        }

        [HttpGet]
        public IActionResult Index(string doctorId)
        {
            this.ViewBag.DoctorList = this.medicalEvaluationService.GetAllDoctors();

            if (string.IsNullOrEmpty(doctorId))
            {
                return this.View(Enumerable.Empty<MedicalEvaluation>());
            }

            bool isFatigued = this.medicalEvaluationService.IsDoctorFatigued(doctorId);
            if (isFatigued)
            {
                this.ViewData[WarningMessageKey] = "WARNING: This doctor has exceeded the fatigue threshold.";
            }

            var evaluations = this.medicalEvaluationService.GetEvaluationsByDoctor(doctorId);
            this.ViewBag.SelectedDoctorId = doctorId;

            return this.View(evaluations);
        }

        [HttpGet]
        public IActionResult Details(int evaluationId)
        {
            var evaluation = this.medicalEvaluationService.GetEvaluationById(evaluationId);
            if (evaluation == null)
            {
                return this.NotFound();
            }

            return this.View(evaluation);
        }

        [HttpGet]
        public IActionResult Create()
        {
            this.ViewBag.DoctorList = this.medicalEvaluationService.GetAllDoctors();
            this.ViewBag.WellnessItems = this.wellnessItemsService.GetWellnessItems();
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MedicalEvaluation newEvaluation)
        {
            this.ViewBag.DoctorList = this.medicalEvaluationService.GetAllDoctors();
            this.ViewBag.WellnessItems = this.wellnessItemsService.GetWellnessItems();

            if (!this.ModelState.IsValid)
            {
                return this.View(newEvaluation);
            }

            string? medicineConflict = this.medicalEvaluationService.CheckMedicineConflict(newEvaluation.PatientId, newEvaluation.MedicationsList);
            if (!string.IsNullOrEmpty(medicineConflict))
            {
                this.ModelState.AddModelError(string.Empty, medicineConflict);
                return this.View(newEvaluation);
            }

            this.medicalEvaluationService.SaveEvaluation(newEvaluation);

            string targetDoctorId = newEvaluation.Evaluator?.StaffID.ToString() ?? string.Empty;
            return this.RedirectToAction(nameof(Index), new { doctorId = targetDoctorId });
        }

        [HttpGet]
        public IActionResult Edit(int evaluationId)
        {
            var evaluation = this.medicalEvaluationService.GetEvaluationById(evaluationId);
            if (evaluation == null)
            {
                return this.NotFound();
            }

            this.ViewBag.WellnessItems = this.wellnessItemsService.GetWellnessItems();
            return this.View(evaluation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int evaluationId, MedicalEvaluation updatedEvaluation)
        {
            if (evaluationId != updatedEvaluation.EvaluationID)
            {
                return this.BadRequest();
            }

            this.ViewBag.WellnessItems = this.wellnessItemsService.GetWellnessItems();

            if (!this.ModelState.IsValid)
            {
                return this.View(updatedEvaluation);
            }

            string? medicineConflict = this.medicalEvaluationService.CheckMedicineConflict(updatedEvaluation.PatientId, updatedEvaluation.MedicationsList);
            if (!string.IsNullOrEmpty(medicineConflict))
            {
                this.ModelState.AddModelError(string.Empty, medicineConflict);
                return this.View(updatedEvaluation);
            }

            this.medicalEvaluationService.UpdateEvaluation(updatedEvaluation);
            return this.RedirectToAction(nameof(Details), new { evaluationId = updatedEvaluation.EvaluationID });
        }

        [HttpGet]
        public IActionResult Delete(int evaluationId)
        {
            var evaluation = this.medicalEvaluationService.GetEvaluationById(evaluationId);
            if (evaluation == null)
            {
                return this.NotFound();
            }

            return this.View(evaluation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int evaluationId)
        {
            var evaluation = this.medicalEvaluationService.GetEvaluationById(evaluationId);
            if (evaluation == null)
            {
                return this.NotFound();
            }

            string targetDoctorId = evaluation.Evaluator?.StaffID.ToString() ?? string.Empty;
            this.medicalEvaluationService.DeleteEvaluation(evaluationId);

            return this.RedirectToAction(nameof(Index), new { doctorId = targetDoctorId });
        }
    }
}
