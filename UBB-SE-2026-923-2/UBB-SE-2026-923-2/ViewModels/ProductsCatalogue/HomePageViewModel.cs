namespace UBB_SE_2026_923_2.ViewModels.ProductsCatalogue
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using UBB_SE_2026_923_2.Models;

    public class HomePageViewModel : INotifyPropertyChanged
    {
        private User currentUser;

        public User CurrentUser
        {
            get => this.currentUser;
            private set
            {
                this.currentUser = value;
                this.OnPropertyChanged();
            }
        }

        public void Initialize(User user)
        {
            this.CurrentUser = user;

            this.OnPropertyChanged(nameof(this.IsAdminDashboardVisible));
            this.OnPropertyChanged(nameof(this.IsMyAccountVisible));
            this.OnPropertyChanged(nameof(this.IsLoginVisible));
            this.OnPropertyChanged(nameof(this.IsRegisterVisible));
        }

        public bool IsAdminDashboardVisible => this.CurrentUser != null && this.CurrentUser.IsAdmin;

        public bool IsMyAccountVisible => this.CurrentUser == null;

        public bool IsLoginVisible => this.CurrentUser != null;

        public bool IsRegisterVisible => this.CurrentUser != null;

        public string HandleNavigationRequest(string requestedDestination)
        {
            if (this.CurrentUser == null)
            {
                bool isAllowed = requestedDestination == "Products" ||
                                 requestedDestination == "Home" ||
                                 requestedDestination == "Login" ||
                                 requestedDestination == "Register" ||
                                 requestedDestination == "ProductDetails";

                if (!isAllowed)
                {
                    return "LoginView";
                }
            }

            return requestedDestination;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}