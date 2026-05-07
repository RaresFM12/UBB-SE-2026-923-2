namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Wire-friendly projection of a hangout participant. The repository
    /// contract returns a value tuple; this record carries the same data
    /// over JSON (System.Text.Json does not preserve tuple element names).
    /// </summary>
    public sealed record HangoutParticipantSummary(int HangoutId, int StaffId);
}
