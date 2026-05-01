using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using UBB_SE_2026_923_2.Command;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.ViewModels.Accounts
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private IUserAccountService userAccountService;
        private string email;
        private string password;
        private string errorMessage;

        public event Action LoginSucceded;
        public event PropertyChangedEventHandler PropertyChanged;
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
        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; set; }

        public LoginViewModel(IUserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;

            LoginCommand = (ICommand)new RelayCommand(Login);
        }

        public void Login()
        {
            try
            {
                userAccountService.Login(Email, Password);
                LoginSucceded?.Invoke();
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
