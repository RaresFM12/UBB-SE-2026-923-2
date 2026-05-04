using System.ComponentModel;
using System.Runtime.CompilerServices;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.ViewModels.ProductsCatalogue
{
    public class HomePageViewModel : INotifyPropertyChanged
    {
        private User currentUser;
        public User CurrentUser
        {
            get => currentUser;
            private set
            {
                currentUser = value;
                OnPropertyChanged();
            }
        }

        public void Initialize(User user)
        {
            CurrentUser = user;

            OnPropertyChanged(nameof(IsAdminDashboardVisible));
            OnPropertyChanged(nameof(IsMyAccountVisible));
            OnPropertyChanged(nameof(IsLoginVisible));
            OnPropertyChanged(nameof(IsRegisterVisible));
        }

        public bool IsAdminDashboardVisible => CurrentUser != null && CurrentUser.IsAdmin;

        public bool IsMyAccountVisible => CurrentUser == null;
        public bool IsLoginVisible => CurrentUser != null;
        public bool IsRegisterVisible => CurrentUser != null;

        public string HandleNavigationRequest(string requestedDestination)
        {
            if (CurrentUser == null)
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
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}