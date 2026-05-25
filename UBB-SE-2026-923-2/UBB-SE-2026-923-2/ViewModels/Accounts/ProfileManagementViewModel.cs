namespace UBB_SE_2026_923_2.ViewModels.Accounts
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using UBB_SE_2026_923_2.Services;

    public class ProfileManagementViewModel : INotifyPropertyChanged
    {
        private readonly IUserAccountService userAccountService;

        private string username;
        private string phoneNumber;
        private string errorMessage;

        public ProfileManagementViewModel(IUserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;
            this.LoadUserData();
        }

        public string Email => this.userAccountService.CurrentUser?.Email ?? string.Empty;

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

        public void LoadUserData()
        {
            var currentUser = this.userAccountService.CurrentUser;
            if (currentUser == null)
            {
                return;
            }

            currentUser = this.userAccountService.LoadCurrentUser(currentUser.Id);
            if (currentUser == null)
            {
                return;
            }

            this.OnPropertyChanged(nameof(this.Email));
            this.Username = currentUser.Username;
            this.PhoneNumber = currentUser.PhoneNumber;
        }

        public void SaveChanges()
        {
            this.userAccountService.UpdateProfile(this.Username, this.PhoneNumber);
        }

        public void CancelChanges()
        {
            this.LoadUserData();
            this.ErrorMessage = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
