namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Wire-friendly projection of a high-risk medicine reference row.
    /// Mirrors the value-tuple shape exposed by the repository contract,
    /// but as a record so it survives JSON serialization.
    /// </summary>
    public sealed record HighRiskMedicineSummary(string MedicineName, string WarningMessage);
}
