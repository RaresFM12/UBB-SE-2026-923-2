namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Lightweight doctor projection used by repository contracts that only
    /// need an id + name. Defined as a record so it serializes cleanly across
    /// the Web API (System.Text.Json does not serialize value-tuple element
    /// names).
    /// </summary>
    public sealed record DoctorSummary(int DoctorId, string FirstName, string LastName);
}
