namespace UBB_SE_2026_923_2.Views.ProductsCatalogue
{
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.ViewModels.ProductsCatalogue;

    public sealed partial class HomePage : Page
    {
        // 1. Expose the ViewModel to the XAML
        public HomePageViewModel ViewModel { get; } = new HomePageViewModel();

        public HomePage()
        {
            InitializeComponent();

            // 2. Set the DataContext so XAML bindings work automatically
            this.DataContext = this.ViewModel;
        }

        // 3. Catch the User object when the app navigates to this page
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // If the app passes a User object, initialize the ViewModel with it.
            if (e.Parameter is User currentUser)
            {
                this.ViewModel.Initialize(currentUser);
            }
            else
            {
                // If no parameter is passed, treat them as a logged-out guest.
                this.ViewModel.Initialize(null);
            }
        }
    }
}