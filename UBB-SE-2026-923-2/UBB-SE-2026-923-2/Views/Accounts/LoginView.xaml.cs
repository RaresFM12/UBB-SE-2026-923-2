using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.ViewModels.Accounts;

namespace UBB_SE_2026_923_2.Views.Accounts
{

    public sealed partial class LoginView : Page
    {
        public static event Action UserLoggedIn;
        public LoginView()
        {
            this.InitializeComponent();
            var viewModel = new LoginViewModel(ServiceWrapper.UserAccountService);
            viewModel.LoginSucceded += OnLoginSucceeded;
            this.DataContext = viewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var vm = (LoginViewModel)this.DataContext;
            vm.Password = PasswordBox.Password;
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            (this.Parent as Frame)?.Navigate(typeof(RegisterView));
        }
        private void OnLoginSucceeded()
        {
            UserLoggedIn?.Invoke();
            (this.Parent as Frame)?.Navigate(typeof(Views.Accounts.ProfileManagementView));
        }
    }
}
