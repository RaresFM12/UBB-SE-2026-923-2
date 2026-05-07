namespace UBB_SE_2026_923_2.Views.Doctor
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using UBB_SE_2026_923_2.Configuration;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Doctor;
    using UBB_SE_2026_923_2.Views.Shell;

    public sealed partial class DoctorSchedulePage : Page
    {
        private readonly DoctorScheduleViewModel viewModel;
        private readonly DialogPresenter dialogPresenter;
        private bool initialized;

        public DoctorSchedulePage()
        {
            InitializeComponent();

            this.viewModel = App.Services.GetRequiredService<DoctorScheduleViewModel>();
            this.dialogPresenter = App.Services.GetRequiredService<DialogPresenter>();
            this.DataContext = this.viewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.dialogPresenter.SetXamlRoot(this.XamlRoot);

            if (this.initialized)
            {
                return;
            }

            this.initialized = true;

            await this.viewModel.InitializeAsync();
        }

        private void DateCalendar_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs eventArgs)
        {
            if (sender.SelectedDates == null || sender.SelectedDates.Count == 0)
            {
                return;
            }

            var picked = sender.SelectedDates[0].Date;

            if (picked < AppSettings.SqlMinimumDate)
            {
                return;
            }

            this.viewModel.SelectedDate = picked;
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is AppointmentItemViewModel item)
            {
                this.Frame?.Navigate(typeof(AppointmentDetailsPage), item.ToAppointment());
            }
        }
    }
}
