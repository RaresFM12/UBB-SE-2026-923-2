namespace UBB_SE_2026_923_2.Models
{
    using System;

    public class Notification
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string ActionButtonText { get; set; } = string.Empty;

        // EF Core navigation property — persisted via shadow FK column "RecipientStaffId".
        public Staff Recipient { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; }

        public Notification()
        {
        }

        public Notification(string title, string message, string actionButtonText = "")
        {
            this.Title = title;
            this.Message = message;
            this.ActionButtonText = actionButtonText;
        }
    }
}
