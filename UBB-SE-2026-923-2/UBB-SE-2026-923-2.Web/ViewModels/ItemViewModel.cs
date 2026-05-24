namespace UBB_SE_2026_923_2.Web.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class ItemViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Producer { get; set; } = string.Empty;

        [Required]
        [Range(0.01, float.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public float Price { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number of pills must be at least 1.")]
        public int NumberOfPills { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        public string Label { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100.")]
        public float DiscountPercentage { get; set; }

        // One entry per line in the format "SubstanceName:Concentration"
        // Example: "Ibuprofen:200.5"
        [Required(ErrorMessage = "At least one active substance is required.")]
        public string SubstancesText { get; set; } = string.Empty;

        // One entry per line in the format "YYYY-MM-DD:NumberOfPacks"
        // Example: "2026-12-31:10"
        public string BatchesText { get; set; } = string.Empty;
    }
}
