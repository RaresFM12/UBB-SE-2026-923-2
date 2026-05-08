namespace UBB_SE_2026_923_2.Views.Accounts
{
    using System;
    using Microsoft.UI.Xaml.Controls;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Accounts;

    public sealed partial class ChangePasswordView : ContentDialog
    {
        private readonly IUserAccountService accountService;

        public ChangePasswordViewModel ChangePasswordViewModel { get; }

        public ChangePasswordView(IUserAccountService service)
        {
            this.InitializeComponent();

            this.accountService = service;
            this.ChangePasswordViewModel = new ChangePasswordViewModel(service);

            this.DataContext = this.ChangePasswordViewModel;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.ChangePasswordViewModel.OldPassword = OldPasswordBox.Password;
            this.ChangePasswordViewModel.NewPassword = NewPasswordBox.Password;
            this.ChangePasswordViewModel.ConfirmPassword = ConfirmPasswordBox.Password;

            this.ChangePasswordViewModel.ErrorMessage = null;

            this.ChangePasswordViewModel.ChangePasswordCommand.Execute(null);

            if (!string.IsNullOrEmpty(this.ChangePasswordViewModel.ErrorMessage))
            {
                args.Cancel = true;
            }
        }
    }
}
