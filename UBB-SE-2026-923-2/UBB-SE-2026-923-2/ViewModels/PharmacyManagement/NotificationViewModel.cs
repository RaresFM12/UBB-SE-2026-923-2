namespace UBB_SE_2026_923_2.ViewModels.PharmacyManagement
{
    public class NotificationViewModel
    {
        public string NotificationTitle { get; set; }

        public string NotificationBody { get; set; }

        public string NotificationButtonText { get; set; }

        public NotificationViewModel(string notificationTitle, string notificationBody, string notificationButtonText)
        {
            this.NotificationTitle = notificationTitle;
            this.NotificationBody = notificationBody;
            this.NotificationButtonText = notificationButtonText;
        }
    }
}
