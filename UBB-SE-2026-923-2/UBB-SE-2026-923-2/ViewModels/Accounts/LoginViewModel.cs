namespace UBB_SE_2026_923_2.ViewModels.Accounts
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using UBB_SE_2026_923_2.Command;
    using UBB_SE_2026_923_2.Services;

    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IUserAccountService userAccountService;
        private string email;
        private string password;
        private string errorMessage;

        public event Action LoginSucceded;

        public event PropertyChangedEventHandler PropertyChanged;

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

        public string ErrorMessage
        {
            get => this.errorMessage;
            set
            {
                this.errorMessage = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; set; }

        public LoginViewModel(IUserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;

            this.LoginCommand = (ICommand)new RelayCommand(this.Login);
        }

        public void Login()
        {
            try
            {
                this.userAccountService.Login(this.Email, this.Password);
                this.LoginSucceded?.Invoke();
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
