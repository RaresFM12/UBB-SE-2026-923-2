namespace UBB_SE_2026_923_2.Views.Accounts
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Accounts;

    public sealed partial class AdminAccountsManagementView : Page
    {
        private IUserAccountService accountService;

        public AdminAccountsManagementViewModel ViewModel { get; private set; }

        public AdminAccountsManagementView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.accountService = ServiceWrapper.UserAccountService;
            this.ViewModel = new AdminAccountsManagementViewModel(this.accountService);

            this.DataContext = this.ViewModel;
        }

        private void OnSearchClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Search();
        }

        private async void OnPromoteClick(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is UserItemViewModel userItem)
            {
                var dialog = new ContentDialog
                {
                    Title = "Warning",
                    Content = "This action cannot be undone. Proceed?",
                    PrimaryButtonText = "Proceed",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot,
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    this.ViewModel.Promote(userItem);
                }
                else
                {
                    cb.IsChecked = false;
                }
            }
        }

        private async void OnDisableClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is UserItemViewModel userItem)
            {
                var dialog = new ContentDialog
                {
                    Title = "Confirm",
                    Content = "Disable this account?",
                    PrimaryButtonText = "Yes",
                    CloseButtonText = "No",
                    XamlRoot = this.XamlRoot,
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    this.ViewModel.Disable(userItem);
                }
            }
        }
    }
}