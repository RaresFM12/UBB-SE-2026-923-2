using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.Command;

namespace UBB_SE_2026_923_2.ViewModels.Accounts
{
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
            userAccountService = service;
            ChangePasswordCommand = new RelayCommand(ChangePassword);
        }

        public string OldPassword
        {
            get => oldPassword;
            set
            {
                oldPassword = value;
                OnPropertyChanged();
            }
        }

        public string NewPassword
        {
            get => newPassword;
            set
            {
                newPassword = value;
                OnPropertyChanged();
            }
        }

        public string ConfirmPassword
        {
            get => confirmPassword;
            set
            {
                confirmPassword = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnPropertyChanged();
            }
        }

        public void ChangePassword()
        {
            try
            {
                userAccountService.ChangePassword(OldPassword, NewPassword, ConfirmPassword);
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
