namespace UBB_SE_2026_923_2.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Web.ViewModels;

    /// <summary>
    /// Thin MVC front end over <see cref="IPrescriptionService"/>. The desktop
    /// uses the same service from <c>BasketPage</c> (prescription-ID textbox +
    /// Add Products button). The web port keeps the single-form flow and adds
    /// high-risk medicine warnings — high-risk meds are part of this assignment
    /// slice and are read-only reference data.
    /// </summary>
    [Authorize(Roles = "Pharmacist,Admin")]
    public class PrescriptionsController : Controller
    {
        private static readonly char[] MedicineSeparators = new[] { ',', ';', '\n', '\r' };

        private readonly IPrescriptionService prescriptionService;
        private readonly IItemsRepository itemsRepository;
        private readonly IEvaluationsRepository evaluationsRepository;
        private readonly IHighRiskMedicineRepository highRiskMedicineRepository;

        public PrescriptionsController(
            IPrescriptionService prescriptionService,
            IItemsRepository itemsRepository,
            IEvaluationsRepository evaluationsRepository,
            IHighRiskMedicineRepository highRiskMedicineRepository)
        {
            this.prescriptionService = prescriptionService;
            this.itemsRepository = itemsRepository;
            this.evaluationsRepository = evaluationsRepository;
            this.highRiskMedicineRepository = highRiskMedicineRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return this.View(new ResolvePrescriptionViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ResolvePrescriptionViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            Dictionary<int, int> resolved;
            try
            {
                // The pharmacist resolves on behalf of a patient whose
                // discount profile isn't modelled in the web session yet; pass
                // an empty dictionary, same shape the service expects.
                resolved = this.prescriptionService.GetItemsFromPrescription(
                    model.PrescriptionId,
                    new Dictionary<int, float>());
            }
            catch (ArgumentException exception)
            {
                this.ModelState.AddModelError(string.Empty, exception.Message);
                return this.View(model);
            }

            var rows = this.BuildResolvedRows(resolved);
            var warnings = this.BuildHighRiskWarnings(model.PrescriptionId);

            var populatedModel = new ResolvePrescriptionViewModel
            {
                PrescriptionId = model.PrescriptionId,
                HasResult = true,
                ResolvedRows = rows,
                HighRiskWarnings = warnings,
            };

            return this.View(populatedModel);
        }

        private IReadOnlyList<ResolvedItemRow> BuildResolvedRows(Dictionary<int, int> resolved)
        {
            var rows = new List<ResolvedItemRow>(resolved.Count);
            foreach (var resolvedEntry in resolved)
            {
                int itemId = resolvedEntry.Key;
                int boxQuantity = resolvedEntry.Value;
                Item item = this.itemsRepository.GetItemById(itemId);
                if (item == null)
                {
                    continue;
                }

                rows.Add(new ResolvedItemRow
                {
                    ItemId = item.Id,
                    ItemName = item.Name,
                    Producer = item.Producer,
                    PillsPerBox = item.NumberOfPills,
                    BoxQuantity = boxQuantity,
                    UnitPrice = item.Price,
                    TotalPrice = item.Price * boxQuantity,
                });
            }

            return rows;
        }

        private IReadOnlyList<string> BuildHighRiskWarnings(string prescriptionId)
        {
            // Match warnings against the original prescription text (not the
            // resolved item names): if a high-risk medicine was prescribed and
            // the service substituted it with a generic, we still want to warn.
            if (!int.TryParse((prescriptionId ?? string.Empty).Trim(), out int evaluationId))
            {
                return new List<string>();
            }

            MedicalEvaluation? evaluation = null;
            foreach (var candidate in this.evaluationsRepository.GetAllEvaluations())
            {
                if (candidate.EvaluationID == evaluationId)
                {
                    evaluation = candidate;
                    break;
                }
            }

            if (evaluation == null || string.IsNullOrWhiteSpace(evaluation.MedicationsList))
            {
                return new List<string>();
            }

            var highRiskReference = this.highRiskMedicineRepository.GetAllHighRiskMedicines();
            var warnings = new List<string>();
            foreach (var medicineName in ParseMedicineNames(evaluation.MedicationsList))
            {
                foreach (var highRiskEntry in highRiskReference)
                {
                    bool namesMatch = string.Equals(
                        highRiskEntry.MedicineName,
                        medicineName,
                        StringComparison.OrdinalIgnoreCase);
                    if (!namesMatch)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(highRiskEntry.WarningMessage))
                    {
                        warnings.Add($"{highRiskEntry.MedicineName}: {highRiskEntry.WarningMessage}");
                    }

                    break;
                }
            }

            return warnings;
        }

        private static IEnumerable<string> ParseMedicineNames(string medicationsList)
        {
            foreach (var part in medicationsList.Split(MedicineSeparators, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = part.Trim();
                if (trimmed.Length > 0)
                {
                    yield return trimmed;
                }
            }
        }
    }
}
