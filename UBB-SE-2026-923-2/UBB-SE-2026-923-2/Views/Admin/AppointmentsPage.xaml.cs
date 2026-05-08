namespace UBB_SE_2026_923_2.Views.Admin
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.ViewModels.Admin;
    using UBB_SE_2026_923_2.Views.Doctor;

    public sealed partial class AppointmentsPage : Page
    {
        public AdminAppointmentsViewModel ViewModel { get; }

        public AppointmentsPage()
        {
            this.InitializeComponent();

            this.ViewModel = App.Services.GetRequiredService<AdminAppointmentsViewModel>();
            this.DataContext = this.ViewModel;

            this.Loaded += this.AppointmentsPage_Loaded;
        }

        private async void AppointmentsPage_Loaded(object sender, RoutedEventArgs eventArgs)
        {
            await this.ViewModel.LoadDoctorsAsync();

            if (this.ViewModel.Doctors.Count > 0)
            {
                DoctorComboBox.SelectedIndex = 0;
                FilterDoctorComboBox.SelectedIndex = 0;
            }
        }

        private async void BookAppointment_Click(object sender, RoutedEventArgs eventArgs)
        {
            string patientId = PatientIdTextBox.Text;

            if (DoctorComboBox.SelectedValue is not int selectedDoctorId ||
                string.IsNullOrWhiteSpace(patientId) ||
                AppointmentDatePicker.Date == null ||
                AppointmentTimePicker.SelectedTime == null)
            {
                this.ShowMessage("Please fill in all fields (Doctor, Patient ID, Date, Time).", InfoBarSeverity.Error);
                return;
            }

            try
            {
                System.DateTime date = AppointmentDatePicker.Date.Value.DateTime;
                System.TimeSpan time = AppointmentTimePicker.SelectedTime.Value;

                await this.ViewModel.BookAppointmentAsync(patientId, selectedDoctorId, date, time);

                this.ShowMessage($"Appointment booked successfully for {patientId}!", InfoBarSeverity.Success);

                PatientIdTextBox.Text = string.Empty;
                DoctorComboBox.SelectedIndex = -1;

                if (FilterDoctorComboBox.SelectedValue is int filterDocId && filterDocId == selectedDoctorId)
                {
                    await this.ViewModel.LoadAppointmentsForDoctorAsync(filterDocId);
                }
            }
            catch (System.Exception exception)
            {
                this.ShowMessage($"Error booking appointment: {exception.Message}", InfoBarSeverity.Error);
            }
        }

        private async void FilterDoctorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
        {
            if (FilterDoctorComboBox.SelectedValue is int doctorId)
            {
                await this.ViewModel.LoadAppointmentsForDoctorAsync(doctorId);
            }
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (sender is Button button && button.Tag is Appointment appointment)
            {
                this.Frame.Navigate(typeof(UBB_SE_2026_923_2.Views.Doctor.AppointmentDetailsPage), appointment);
            }
        }

        private async void CancelAppointment_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (sender is Button button && button.Tag is Appointment appointment)
            {
                try
                {
                    await this.ViewModel.CancelAppointmentAsync(appointment);
                    this.ShowMessage("Appointment successfully canceled.", InfoBarSeverity.Informational);

                    if (FilterDoctorComboBox.SelectedValue is int doctorId)
                    {
                        await this.ViewModel.LoadAppointmentsForDoctorAsync(doctorId);
                    }
                }
                catch (System.InvalidOperationException exception)
                {
                    this.ShowMessage(exception.Message, InfoBarSeverity.Error);
                }
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
