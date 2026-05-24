namespace UBB_SE_2026_923_2.Web.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Single-page view model for the pharmacist prescription tool. The desktop
    /// equivalent lives on <c>BasketPage</c> as a prescription-ID textbox + button;
    /// the web port keeps the same single-input flow.
    /// </summary>
    public class ResolvePrescriptionViewModel
    {
        [Required(ErrorMessage = "Prescription ID is required.")]
        [Display(Name = "Prescription ID")]
        public string PrescriptionId { get; set; } = string.Empty;

        public bool HasResult { get; set; }

        public IReadOnlyList<ResolvedItemRow> ResolvedRows { get; init; } = new List<ResolvedItemRow>();

        public IReadOnlyList<string> HighRiskWarnings { get; init; } = new List<string>();
    }

    /// <summary>
    /// One row of the resolved-items table: the in-stock item the pharmacist
    /// would dispense for one of the prescribed medicines, plus the quantity
    /// of boxes and the line totals (already discounted by the service).
    /// </summary>
    public class ResolvedItemRow
    {
        public int ItemId { get; init; }

        public string ItemName { get; init; } = string.Empty;

        public string Producer { get; init; } = string.Empty;

        public int PillsPerBox { get; init; }

        public int BoxQuantity { get; init; }

        public float UnitPrice { get; init; }

        public float TotalPrice { get; init; }
    }
}
