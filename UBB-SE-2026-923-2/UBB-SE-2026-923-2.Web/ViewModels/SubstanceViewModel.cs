namespace UBB_SE_2026_923_2.Web.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class SubstanceViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, float.MaxValue, ErrorMessage = "Lethal dose must be greater than zero.")]
        public float LethalDose { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}
