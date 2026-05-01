using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Views.Doctor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using UBB_SE_2026_923_2.ViewModels.Admin;

namespace UBB_SE_2026_923_2.Views.Admin
{
    public sealed partial class AppointmentsPage : Page
    {
        public AdminAppointmentsViewModel ViewModel { get; }

        public AppointmentsPage()
        {
            this.InitializeComponent();

            ViewModel = App.Services.GetRequiredService<AdminAppointmentsViewModel>();
            DataContext = ViewModel;

            Loaded += AppointmentsPage_Loaded;
        }

        private async void AppointmentsPage_Loaded(object sender, RoutedEventArgs eventArgs)
        {
            await ViewModel.LoadDoctorsAsync();

            if (ViewModel.Doctors.Count > 0)
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
                ShowMessage("Please fill in all fields (Doctor, Patient ID, Date, Time).", InfoBarSeverity.Error);
                return;
            }

            try
            {
                System.DateTime date = AppointmentDatePicker.Date.Value.DateTime;
                System.TimeSpan time = AppointmentTimePicker.SelectedTime.Value;

                await ViewModel.BookAppointmentAsync(patientId, selectedDoctorId, date, time);

                ShowMessage($"Appointment booked successfully for {patientId}!", InfoBarSeverity.Success);

                PatientIdTextBox.Text = string.Empty;
                DoctorComboBox.SelectedIndex = -1;

                if (FilterDoctorComboBox.SelectedValue is int filterDocId && filterDocId == selectedDoctorId)
                {
                    await ViewModel.LoadAppointmentsForDoctorAsync(filterDocId);
                }
            }
            catch (System.Exception exception)
            {
                ShowMessage($"Error booking appointment: {exception.Message}", InfoBarSeverity.Error);
            }
        }

        private async void FilterDoctorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
        {
            if (FilterDoctorComboBox.SelectedValue is int doctorId)
            {
                await ViewModel.LoadAppointmentsForDoctorAsync(doctorId);
            }
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (sender is Button btn && btn.Tag is Appointment appt)
            {
                this.Frame.Navigate(typeof(UBB_SE_2026_923_2.Views.Doctor.AppointmentDetailsPage), appt);
            }
        }

        private async void CancelAppointment_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (sender is Button btn && btn.Tag is Appointment appt)
            {
                try
                {
                    await ViewModel.CancelAppointmentAsync(appt);
                    ShowMessage("Appointment successfully canceled.", InfoBarSeverity.Informational);

                    if (FilterDoctorComboBox.SelectedValue is int doctorId)
                    {
                        await ViewModel.LoadAppointmentsForDoctorAsync(doctorId);
                    }
                }
                catch (System.InvalidOperationException exception)
                {
                    ShowMessage(exception.Message, InfoBarSeverity.Error);
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
