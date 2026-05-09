namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Many-to-many link between <see cref="Hangout"/> and <see cref="Staff"/>.
    /// Surrogate primary key Id replaces the old composite (HangoutId, StaffId) PK.
    /// FK columns are maintained as EF Core shadow properties.
    /// </summary>
    public class HangoutParticipant
    {
        // Surrogate primary key — replaces the old composite (HangoutId, StaffId) PK.
        public int Id { get; set; }

        // EF Core navigation properties — persisted via shadow FK columns
        // "HangoutId" and "StaffId".
        public Hangout Hangout { get; set; } = null!;

        public Staff Staff { get; set; } = null!;
    }
}
