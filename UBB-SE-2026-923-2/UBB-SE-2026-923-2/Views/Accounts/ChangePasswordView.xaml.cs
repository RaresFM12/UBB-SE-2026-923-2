namespace UBB_SE_2026_923_2.Views.Accounts
{
    using System;
    using Microsoft.UI.Xaml.Controls;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Accounts;

    public sealed partial class ChangePasswordView : ContentDialog
    {
        private readonly IUserAccountService accountService;

        public ChangePasswordViewModel ViewModel { get; }

        public ChangePasswordView(IUserAccountService service)
        {
            this.InitializeComponent();

            this.accountService = service;
            this.ViewModel = new ChangePasswordViewModel(service);

            this.DataContext = this.ViewModel;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.ViewModel.OldPassword = OldPasswordBox.Password;
            this.ViewModel.NewPassword = NewPasswordBox.Password;
            this.ViewModel.ConfirmPassword = ConfirmPasswordBox.Password;

            this.ViewModel.ErrorMessage = null;

            this.ViewModel.ChangePasswordCommand.Execute(null);

            if (!string.IsNullOrEmpty(this.ViewModel.ErrorMessage))
            {
                args.Cancel = true;
            }
        }
    }
}
