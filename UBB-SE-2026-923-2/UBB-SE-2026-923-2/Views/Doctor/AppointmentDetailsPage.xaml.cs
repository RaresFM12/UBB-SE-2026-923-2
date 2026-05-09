namespace UBB_SE_2026_923_2.Views.Doctor
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public sealed partial class AppointmentDetailsPage : Page
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string TimeFormat = @"hh\:mm";
        private const string FinishedStatus = "Finished";

        private Appointment? currentAppointment;
        private readonly IDoctorAppointmentService service;

        public AppointmentDetailsPage()
        {
            this.InitializeComponent();

            this.service = App.Services.GetRequiredService<IDoctorAppointmentService>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);

            if (eventArgs.Parameter is Appointment appointment)
            {
                this.currentAppointment = appointment;
                this.PopulateData();
            }
        }

        private void PopulateData()
        {
            if (this.currentAppointment == null)
            {
                return;
            }

            PatientNameText.Text = this.currentAppointment.PatientName;
            DoctorNameText.Text = this.currentAppointment.Doctor.FullName;
            DateText.Text = this.currentAppointment.Date.ToString(DateFormat);
            TimeText.Text = $"{this.currentAppointment.StartTime.ToString(TimeFormat)} - {this.currentAppointment.EndTime.ToString(TimeFormat)}";
            StatusText.Text = this.currentAppointment.Status;
        }

        private async void FinishBtn_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (this.currentAppointment == null)
            {
                return;
            }

            try
            {
                await this.service.FinishAppointmentAsync(this.currentAppointment!);

                this.currentAppointment!.Status = FinishedStatus;
                this.PopulateData();
                this.ShowMessage("Appointment finished successfully! Doctor status updated.", InfoBarSeverity.Success);
            }
            catch (Exception exception)
            {
                this.ShowMessage($"Error: {exception.Message}", InfoBarSeverity.Error);
            }
        }

        private void GoBack_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
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
