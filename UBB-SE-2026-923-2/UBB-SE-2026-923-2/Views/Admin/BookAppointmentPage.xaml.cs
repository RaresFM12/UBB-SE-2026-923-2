namespace UBB_SE_2026_923_2.Views.Admin
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using UBB_SE_2026_923_2.ViewModels.Admin;

    public sealed partial class BookAppointmentPage : Page
    {
        public AdminAppointmentsViewModel ViewModel { get; }

        public BookAppointmentPage()
        {
            this.InitializeComponent();

            this.ViewModel = App.Services.GetRequiredService<AdminAppointmentsViewModel>();
            this.DataContext = this.ViewModel;
            this.Loaded += this.BookAppointmentPage_Loaded;
        }

        private async void BookAppointmentPage_Loaded(object sender, RoutedEventArgs eventArgs)
        {
            await this.ViewModel.LoadDoctorsAsync();

            if (this.ViewModel.Doctors.Count > 0)
            {
                DoctorComboBox.SelectedIndex = 0;
            }
        }

        private async void CreateAppointment_Click(object sender, RoutedEventArgs eventArgs)
        {
            string patientId = PatientIdTextBox.Text;

            if (DoctorComboBox.SelectedValue is not int selectedDoctorId ||
                string.IsNullOrWhiteSpace(patientId) ||
                AppointmentDatePicker.Date == null ||
                AppointmentTimePicker.SelectedTime == null)
            {
                this.ShowMessage("Please fill in all fields (Doctor, Patient id, Date, Time).", InfoBarSeverity.Error);
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
                AppointmentDatePicker.Date = null;
                AppointmentTimePicker.SelectedTime = null;
            }
            catch (System.Exception exception)
            {
                this.ShowMessage($"Error booking appointment: {exception.Message}", InfoBarSeverity.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs eventArgs)
        {
            this.Frame.Navigate(typeof(AppointmentsPage));
        }

        private void ShowMessage(string message, InfoBarSeverity severity)
        {
            StatusInfoBar.Message = message;
            StatusInfoBar.Severity = severity;
            StatusInfoBar.IsOpen = true;
        }
    }
}
