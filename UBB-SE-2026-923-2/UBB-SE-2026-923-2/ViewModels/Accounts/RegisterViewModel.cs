namespace UBB_SE_2026_923_2.ViewModels.Accounts
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using UBB_SE_2026_923_2.Command;
    using UBB_SE_2026_923_2.Services;

    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly IUserAccountService userAccountService;
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
            get => this.selectedRole;
            set
            {
                this.selectedRole = value;
                this.OnPropertyChanged();
            }
        }

        public string Email
        {
            get => this.email;
            set
            {
                this.email = value;
                this.OnPropertyChanged();
            }
        }

        public string Password
        {
            get => this.password;
            set
            {
                this.password = value;
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

        public string Username
        {
            get => this.username;
            set
            {
                this.username = value;
                this.OnPropertyChanged();
            }
        }

        public string PhoneNumber
        {
            get => this.phoneNumber;
            set
            {
                this.phoneNumber = value;
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

        public ICommand RegisterCommand { get; }

        public RegisterViewModel(IUserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;
            this.selectedRole = "Client";
            this.RegisterCommand = new RelayCommand(this.Register);
        }

        private void Register()
        {
            try
            {
                this.userAccountService.Register(
                    this.Email,
                    this.Password,
                    this.ConfirmPassword,
                    this.Username,
                    this.PhoneNumber,
                    this.SelectedRole);

                this.RegisterSucceded?.Invoke();
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
