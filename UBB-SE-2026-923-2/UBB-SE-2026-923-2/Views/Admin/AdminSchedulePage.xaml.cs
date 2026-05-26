namespace UBB_SE_2026_923_2.Views.Admin
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
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

            this.AdminShiftViewModel.IsWeeklyView = false;
            this.AdminShiftViewModel.SelectedDepartment = "All Departments";
            this.AdminShiftViewModel.LoadAndFilterShifts();
            ShiftDatePicker.Date = System.DateTimeOffset.Now;
        }

        private void ShiftDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (!args.NewDate.HasValue)
            {
                return;
            }

            var picked = args.NewDate.Value.Date;

            if (picked >= AppSettings.SqlMinimumDate)
            {
                this.AdminShiftViewModel.SelectedDate = picked;
            }
        }

        private void CreateShift_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AdminShiftView));
        }

        private void ComputeSalary_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UBB_SE_2026_923_2.Views.SalaryPlaceholderPage));
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
    }
}
