using System;
using Microsoft.UI.Xaml.Controls;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.ViewModels.Accounts;

namespace UBB_SE_2026_923_2.Views.Accounts
{
    public sealed partial class ChangePasswordView : ContentDialog
    {
        private IUserAccountService accountService;
        public ChangePasswordViewModel ViewModel { get; }

        public ChangePasswordView(IUserAccountService service)
        {
            this.InitializeComponent();

            accountService = service;
            ViewModel = new ChangePasswordViewModel(service);

            this.DataContext = ViewModel;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.OldPassword = OldPasswordBox.Password;
            ViewModel.NewPassword = NewPasswordBox.Password;
            ViewModel.ConfirmPassword = ConfirmPasswordBox.Password;

            ViewModel.ErrorMessage = null;

            ViewModel.ChangePasswordCommand.Execute(null);

            if (!string.IsNullOrEmpty(ViewModel.ErrorMessage))
            {
                args.Cancel = true;
            }
        }
    }
}
