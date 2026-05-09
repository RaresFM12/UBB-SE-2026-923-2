namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Per-user note attached to the period tracker.
    /// Surrogate primary key Id replaces the old composite (UserId, NoteId) PK.
    /// NoteId is kept as a regular column for legacy projection.
    /// FK column UserId is maintained as an EF Core shadow property.
    /// </summary>
    public class PeriodNote
    {
        // Surrogate primary key — replaces the old composite (UserId, NoteId) PK.
        public int Id { get; set; }

        // Kept as a regular column; used in legacy dictionary projection.
        public int NoteId { get; set; }

        public string NoteBody { get; set; } = string.Empty;

        public bool IsDone { get; set; }

        // EF Core navigation property — persisted via shadow FK column "UserId".
        public User User { get; set; } = null!;
    }
}
