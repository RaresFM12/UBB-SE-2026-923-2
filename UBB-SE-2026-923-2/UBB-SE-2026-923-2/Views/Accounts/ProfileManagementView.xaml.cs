namespace UBB_SE_2026_923_2.Views.Accounts
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Accounts;

    public sealed partial class ProfileManagementView : Page
    {
        private readonly UserAccountService accountService;

        public ProfileManagementViewModel ViewModel { get; }

        public ProfileManagementView()
        {
            this.InitializeComponent();

            this.accountService = ServiceWrapper.UserAccountService;
            this.ViewModel = new ProfileManagementViewModel(this.accountService);

            this.DataContext = this.ViewModel;
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ViewModel.ErrorMessage = null;
                this.ViewModel.SaveChanges();
            }
            catch (Exception ex)
            {
                this.ViewModel.ErrorMessage = ex.Message;
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.CancelChanges();
        }

        private async void OnChangePasswordClick(object sender, RoutedEventArgs e)
        {
            var dialog = new ChangePasswordView(this.accountService);
            dialog.XamlRoot = this.XamlRoot;

            await dialog.ShowAsync();
        }

        private async void OnOrderHistoryClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UBB_SE_2026_923_2.Views.Orders.OrderHistoryPage), new OrderService());
        }
    }
}
