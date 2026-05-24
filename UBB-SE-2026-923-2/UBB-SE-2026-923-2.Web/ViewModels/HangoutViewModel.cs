namespace UBB_SE_2026_923_2.Web.ViewModels
{
    public class HangoutViewModel
    {
        public int HangoutId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string FormattedDate { get; set; } = string.Empty;

        public int ParticipantCount { get; set; }

        public int MaxParticipants { get; set; }

        public bool IsFull { get; set; }
    }
}
