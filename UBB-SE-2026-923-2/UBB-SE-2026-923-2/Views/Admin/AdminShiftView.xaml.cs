namespace UBB_SE_2026_923_2.Views.Admin
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.ViewModels.Admin;

    public sealed partial class AdminShiftView : Page
    {
        public AdminShiftViewModel ViewModel { get; set; }

        public AdminShiftView()
        {
            this.InitializeComponent();

            this.ViewModel = App.Services.GetRequiredService<AdminShiftViewModel>();
        }

        private void LocationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ViewModel == null)
            {
                return;
            }

            if (LocationComboBox.SelectedItem is string selectedLocation)
            {
                this.ViewModel.FilterSpecializationsAndCertificationsForLocation(selectedLocation);
            }
        }

        private void SpecializationCertificationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ViewModel == null)
            {
                return;
            }

            var selectedLocation = LocationComboBox.SelectedItem as string;
            var selectedSpecializationOrCertification = SpecializationCertificationComboBox.SelectedItem as string;

            if (!string.IsNullOrEmpty(selectedSpecializationOrCertification) && !string.IsNullOrEmpty(selectedLocation))
            {
                this.ViewModel.FilterStaffForShift(selectedLocation, selectedSpecializationOrCertification);
                StaffComboBox.SelectedIndex = -1;
            }
        }

        private void CreateShift_Click(object sender, RoutedEventArgs e)
        {
            if (StaffComboBox.SelectedItem is not IStaff selectedStaff ||
                LocationComboBox.SelectedItem is not string location ||
                !ShiftDatePicker.Date.HasValue ||
                !StartTimePicker.SelectedTime.HasValue ||
                !EndTimePicker.SelectedTime.HasValue)
            {
                this.ShowMessage("Please fill all the fields of the form!", InfoBarSeverity.Error);
                return;
            }

            System.DateTime date = ShiftDatePicker.Date.Value.Date;
            System.DateTime start = date.Add(StartTimePicker.SelectedTime.Value);
            System.DateTime end = date.Add(EndTimePicker.SelectedTime.Value);

            if (end <= start)
            {
                this.ShowMessage("Error: End hour must be chronologically after the start hour!", InfoBarSeverity.Warning);
                return;
            }

            this.ViewModel.CreateNewShift(selectedStaff, start, end, location);

            this.ShowMessage("The shift was scheduled successfuly!", InfoBarSeverity.Success);

            StaffComboBox.SelectedIndex = -1;
            LocationComboBox.SelectedIndex = -1;
        }

        private void SetActive_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int shiftId)
            {
                this.ViewModel.SetShiftActive(shiftId);
                this.ShowMessage($"The shift #{shiftId} was marked as active.", InfoBarSeverity.Success);
            }
        }

        private void CancelShift_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int shiftId)
            {
                this.ViewModel.CancelShift(shiftId);
                this.ShowMessage($"The shift #{shiftId} was cancelled.", InfoBarSeverity.Informational);
            }
        }

        private void AutoReassign_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Shift shiftToReassign)
            {
                this.ViewModel.AutoFindReplacement(shiftToReassign);
                this.ShowMessage("The automatic searching of a replacement has been triggered.", InfoBarSeverity.Success);
            }
        }

        private void OpenSchedule_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AdminSchedulePage));
        }

        private void ShowMessage(string message, InfoBarSeverity severity)
        {
            StatusInfoBar.Message = message;
            StatusInfoBar.Severity = severity;
            StatusInfoBar.IsOpen = true;
        }
    }
}
