namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Per-user note attached to the period tracker. Composite key (UserId, NoteId).
    /// </summary>
    public class PeriodNote
    {
        public int UserId { get; set; }

        public int NoteId { get; set; }

        public string NoteBody { get; set; } = string.Empty;

        public bool IsDone { get; set; }

        public User? User { get; set; }
    }
}
