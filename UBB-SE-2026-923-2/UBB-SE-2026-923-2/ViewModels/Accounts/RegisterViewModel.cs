using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using UBB_SE_2026_923_2.Command;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.ViewModels.Accounts
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private IUserAccountService userAccountService;
        private string email;
        private string password;
        private string username;
        private string phoneNumber;
        private string confirmPassword;
        private string errorMessage;
        private string selectedRole;

        public event Action RegisterSucceded;
        public event PropertyChangedEventHandler PropertyChanged;

        public List<string> RoleOptions { get; } = new List<string> { "Admin", "Client", "Doctor", "Pharmacist" };

        public string SelectedRole
        {
            get => selectedRole;
            set
            {
                selectedRole = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => email;
            set
            {
                email = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => password;
            set
            {
                password = value;
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

        public string Username
        {
            get => username;
            set
            {
                username = value;
                OnPropertyChanged();
            }
        }
        public string PhoneNumber
        {
            get => phoneNumber;
            set
            {
                phoneNumber = value;
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

        public ICommand RegisterCommand { get; }
        public RegisterViewModel(IUserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;
            selectedRole = "Client";
            RegisterCommand = new RelayCommand(Register);
        }
        private void Register()
        {
            try
            {
                userAccountService.Register(
                    Email,
                    Password,
                    ConfirmPassword,
                    Username,
                    PhoneNumber,
                    SelectedRole);

                RegisterSucceded?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
