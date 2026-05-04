namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Many-to-many link between <see cref="Hangout"/> and <see cref="Staff"/>.
    /// Composite key (HangoutId, StaffId). Replaces the legacy in-memory
    /// <see cref="Hangout.ParticipantList"/>.
    /// </summary>
    public class HangoutParticipant
    {
        public int HangoutId { get; set; }
        public int StaffId { get; set; }

        public Hangout? Hangout { get; set; }
        public Staff? Staff { get; set; }
    }
}
