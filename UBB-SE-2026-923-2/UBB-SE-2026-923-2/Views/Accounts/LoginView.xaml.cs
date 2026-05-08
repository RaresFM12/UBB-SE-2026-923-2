namespace UBB_SE_2026_923_2.Views.Accounts
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Accounts;
    using UBB_SE_2026_923_2.Views;

    public sealed partial class LoginView : Page
    {
        public static event Action UserLoggedIn;

        public LoginView()
        {
            this.InitializeComponent();
            var loginViewModel = new LoginViewModel(ServiceWrapper.UserAccountService);
            loginViewModel.LoginSucceded += this.OnLoginSucceeded;
            this.DataContext = loginViewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var loginViewModel = (LoginViewModel)this.DataContext;
            loginViewModel.Password = PasswordBox.Password;
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            (this.Parent as Frame)?.Navigate(typeof(RegisterView));
        }

        private void OnLoginSucceeded()
        {
            var currentUser = ServiceWrapper.UserAccountService.CurrentUser;
            var currentUserService = App.Services.GetRequiredService<ICurrentUserService>();
            currentUserService.SetFromUser(currentUser);

            UserLoggedIn?.Invoke();
            (this.Parent as Frame)?.Navigate(typeof(RoleDashboardPage));
        }
    }
}
