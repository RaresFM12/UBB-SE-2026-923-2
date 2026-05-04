using System;

namespace UBB_SE_2026_923_2.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string ActionButtonText { get; set; }

        public Notification()
        {
        }

        public Notification(string title, string message, string actionButtonText = "")
        {
            Title = title;
            Message = message;
            ActionButtonText = actionButtonText;
        }
    }
}
