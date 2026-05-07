namespace UBB_SE_2026_923_2.ViewModels.PharmacyManagement
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public class NotificationsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<NotificationViewModel> notifications;

        public ObservableCollection<NotificationViewModel> Notifications
        {
            get => this.notifications;
            set
            {
                this.notifications = value;
                this.OnPropertyChanged();
            }
        }

        private readonly IAdminService adminService;

        public NotificationsViewModel(IAdminService adminService)
        {
            this.Notifications = new ObservableCollection<NotificationViewModel>();
            this.adminService = adminService;
        }

        public void PopulateNotifications()
        {
            User currentUser = ServiceWrapper.UserAccountService.CurrentUser;
            if (currentUser == null)
            {
                return;
            }

            List<Notification> notificationData = this.adminService.GetNotificationsForUser(currentUser);

            this.Notifications.Clear();

            foreach (Notification notification in notificationData)
            {
                this.Notifications.Add(new NotificationViewModel(notification.Title, notification.Message, notification.ActionButtonText));
            }
        }

        // --- INotifyPropertyChanged Implementation ---
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}