namespace UBB_SE_2026_923_2.Views.Admin
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Controls.Primitives;
    using Microsoft.UI.Xaml.Navigation;
    using UBB_SE_2026_923_2.Configuration;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.ViewModels.Admin;

    public sealed partial class AdminSchedulePage : Page
    {
        public AdminShiftViewModel AdminShiftViewModel { get; }

        private bool initialized;

        public AdminSchedulePage()
        {
            this.InitializeComponent();

            this.AdminShiftViewModel = App.Services.GetRequiredService<AdminShiftViewModel>();
            this.DataContext = this.AdminShiftViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (this.initialized)
            {
                return;
            }

            this.initialized = true;

            this.AdminShiftViewModel.LoadAndFilterShifts();
            DateCalendar.SelectedDates.Add(System.DateTime.Today);
        }

        private void DateCalendar_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs eventArgs)
        {
            if (sender.SelectedDates == null || sender.SelectedDates.Count == 0)
            {
                return;
            }

            var picked = sender.SelectedDates[0].Date;

            if (picked >= AppSettings.SqlMinimumDate)
            {
                this.AdminShiftViewModel.SelectedDate = picked;
            }
        }

        private void DepartmentFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DepartmentFilterComboBox.SelectedItem is string selectedDept && this.initialized)
            {
                this.AdminShiftViewModel.SelectedDepartment = selectedDept;
            }
        }

        private void ViewMode_Click(object sender, RoutedEventArgs e)
        {
            if (ReferenceEquals(sender, DailyBtn))
            {
                DailyBtn.IsChecked = true;
                WeeklyBtn.IsChecked = false;
                this.AdminShiftViewModel.IsWeeklyView = false;
            }
            else if (ReferenceEquals(sender, WeeklyBtn))
            {
                WeeklyBtn.IsChecked = true;
                DailyBtn.IsChecked = false;
                this.AdminShiftViewModel.IsWeeklyView = true;
            }
        }

        private void SetActive_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int shiftId)
            {
                this.AdminShiftViewModel.SetShiftActive(shiftId);
                this.ShowMessage($"The shift #{shiftId} was marked as active.", InfoBarSeverity.Success);
            }
        }

        private void CancelShift_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int shiftId)
            {
                this.AdminShiftViewModel.CancelShift(shiftId);
                this.ShowMessage($"The shift #{shiftId} was cancelled.", InfoBarSeverity.Informational);
            }
        }

        private void AutoReassign_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Shift shiftToReassign)
            {
                this.AdminShiftViewModel.AutoFindReplacement(shiftToReassign);
                this.ShowMessage("The automatic searching of a replacement has been triggered.", InfoBarSeverity.Success);
            }
        }

        private void ShowMessage(string message, InfoBarSeverity severity)
        {
            StatusInfoBar.Message = message;
            StatusInfoBar.Severity = severity;
            StatusInfoBar.IsOpen = true;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AdminShiftView));
        }
    }
}
