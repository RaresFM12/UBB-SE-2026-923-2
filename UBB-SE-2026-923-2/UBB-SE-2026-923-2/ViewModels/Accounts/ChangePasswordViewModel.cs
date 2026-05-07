namespace UBB_SE_2026_923_2.ViewModels.Accounts
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using UBB_SE_2026_923_2.Command;
    using UBB_SE_2026_923_2.Services;

    public class ChangePasswordViewModel : INotifyPropertyChanged
    {
        private readonly IUserAccountService userAccountService;

        private string oldPassword;
        private string newPassword;
        private string confirmPassword;
        private string errorMessage;

        public ICommand ChangePasswordCommand;

        public ChangePasswordViewModel(IUserAccountService service)
        {
            this.userAccountService = service;
            this.ChangePasswordCommand = new RelayCommand(this.ChangePassword);
        }

        public string OldPassword
        {
            get => this.oldPassword;
            set
            {
                this.oldPassword = value;
                this.OnPropertyChanged();
            }
        }

        public string NewPassword
        {
            get => this.newPassword;
            set
            {
                this.newPassword = value;
                this.OnPropertyChanged();
            }
        }

        public string ConfirmPassword
        {
            get => this.confirmPassword;
            set
            {
                this.confirmPassword = value;
                this.OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => this.errorMessage;
            set
            {
                this.errorMessage = value;
                this.OnPropertyChanged();
            }
        }

        public void ChangePassword()
        {
            try
            {
                this.userAccountService.ChangePassword(this.OldPassword, this.NewPassword, this.ConfirmPassword);
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
