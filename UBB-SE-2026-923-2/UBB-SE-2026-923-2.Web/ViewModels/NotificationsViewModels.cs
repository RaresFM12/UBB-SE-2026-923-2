namespace UBB_SE_2026_923_2.Web.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;

    public class NotificationsIndexViewModel
    {
        public IReadOnlyList<NotificationItemViewModel> Notifications { get; set; } = new List<NotificationItemViewModel>();
    }

    public class NotificationItemViewModel
    {
        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string ActionButtonText { get; set; } = string.Empty;
    }

    public class NotificationsDropdownViewModel
    {
        public IReadOnlyList<NotificationItemViewModel> Notifications { get; set; } = new List<NotificationItemViewModel>();

        public int UnreadCount => this.Notifications.Count;
    }
}
