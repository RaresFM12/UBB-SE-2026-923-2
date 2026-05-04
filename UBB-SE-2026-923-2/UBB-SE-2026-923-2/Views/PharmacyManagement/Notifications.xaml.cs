using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UBB_SE_2026_923_2.Views.ProductsCatalogue;
using UBB_SE_2026_923_2.Services;

using UBB_SE_2026_923_2.ViewModels.PharmacyManagement;

namespace UBB_SE_2026_923_2.Views.PharmacyManagement
{
    public sealed partial class Notifications : Page
    {
        private NotificationsViewModel ViewModel { get; } = new NotificationsViewModel(new AdminService());

        public Notifications()
        {
            ViewModel.PopulateNotifications();

            InitializeComponent();
        }

        private void OnNotificationButtonClicked(object sender, RoutedEventArgs e)
        {
            string buttonContent = (string)((Button)sender).Content;
            if (buttonContent == "Go to products")
            {
                Frame.Navigate(typeof(HomePage));
            }

            if (buttonContent == "Go fix it")
            {
                Frame.Navigate(typeof(EditPage));
            }
        }
    }
}