namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Reference data: a medicine flagged as high risk. Read-only via the
    /// repository; populated through migration seed data or a separate admin
    /// flow rather than user input.
    /// </summary>
    public class HighRiskMedicine
    {
        public string MedicineName { get; set; } = string.Empty;

        public string WarningMessage { get; set; } = string.Empty;
    }
}
