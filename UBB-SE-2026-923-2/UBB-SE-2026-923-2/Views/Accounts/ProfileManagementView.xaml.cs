using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.ViewModels.Accounts;

namespace UBB_SE_2026_923_2.Views.Accounts
{
    public sealed partial class ProfileManagementView : Page
    {
        private UserAccountService accountService;
        public ProfileManagementViewModel ViewModel { get; }

        public ProfileManagementView()
        {
            this.InitializeComponent();

            accountService = ServiceWrapper.UserAccountService;
            ViewModel = new ProfileManagementViewModel(accountService);

            this.DataContext = ViewModel;
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.ErrorMessage = null;
                ViewModel.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewModel.ErrorMessage = ex.Message;
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            ViewModel.CancelChanges();
        }

        private async void OnChangePasswordClick(object sender, RoutedEventArgs e)
        {
            var dialog = new ChangePasswordView(accountService);
            dialog.XamlRoot = this.XamlRoot;

            await dialog.ShowAsync();
        }

        private async void OnOrderHistoryClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UBB_SE_2026_923_2.Views.Orders.OrderHistoryPage), new OrderService());
        }
    }
}
