using System.ComponentModel;
using System.Runtime.CompilerServices;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.ViewModels.Accounts
{
    public class ProfileManagementViewModel : INotifyPropertyChanged
    {
        private IUserAccountService userAccountService;

        private string username;
        private string phoneNumber;
        private string errorMessage;

        public ProfileManagementViewModel(IUserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;
            LoadUserData();
        }

        public string Email => userAccountService.CurrentUser?.Email ?? string.Empty;

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

        public void LoadUserData()
        {
            var currentUser = userAccountService.CurrentUser;
            if (currentUser == null)
            {
                return;
            }

            Username = currentUser.Username;
            PhoneNumber = currentUser.PhoneNumber;
        }

        public void SaveChanges()
        {
            userAccountService.UpdateProfile(Username, PhoneNumber);
        }

        public void CancelChanges()
        {
            LoadUserData();
            ErrorMessage = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
