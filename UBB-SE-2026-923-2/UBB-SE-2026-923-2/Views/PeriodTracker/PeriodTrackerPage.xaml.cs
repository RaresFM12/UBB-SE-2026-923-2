namespace UBB_SE_2026_923_2.Views.PeriodTracker
{
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using UBB_SE_2026_923_2.ViewModels.PeriodTracker;

    public sealed partial class PeriodTrackerPage : Page
    {
        public PeriodTrackerViewModel ViewModel { get; private set; }

        public PeriodTrackerPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArguments)
        {
            base.OnNavigatedTo(navigationEventArguments);

            if (navigationEventArguments.Parameter is PeriodTrackerViewModel periodTrackerViewModel)
            {
                this.ViewModel = periodTrackerViewModel;
                this.DataContext = this.ViewModel;
                this.ViewModel.LoadInitialState();
                Bindings.Update();
            }
        }
    }
}