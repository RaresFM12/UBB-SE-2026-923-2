namespace UBB_SE_2026_923_2.Views.Accounts
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Accounts;
    using UBB_SE_2026_923_2.Views;

    public sealed partial class RegisterView : Page
    {
        public static event Action UserRegistered;

        public RegisterView()
        {
            this.InitializeComponent();
            var registerViewModel = new RegisterViewModel(ServiceWrapper.UserAccountService);
            registerViewModel.RegisterSucceded += this.OnRegisterSucceded;
            this.DataContext = registerViewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var registerViewModel = (RegisterViewModel)this.DataContext;
            registerViewModel.Password = PasswordBox.Password;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var vm = (RegisterViewModel)this.DataContext;
            vm.ConfirmPassword = ConfirmPasswordBox.Password;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            (this.Parent as Frame)?.Navigate(typeof(LoginView));
        }

        private void OnRegisterSucceded()
        {
            var currentUser = ServiceWrapper.UserAccountService.CurrentUser;
            var currentUserService = App.Services.GetRequiredService<ICurrentUserService>();
            currentUserService.SetFromUser(currentUser);

            UserRegistered?.Invoke();
            (this.Parent as Frame)?.Navigate(typeof(RoleDashboardPage));
        }
    }
}
