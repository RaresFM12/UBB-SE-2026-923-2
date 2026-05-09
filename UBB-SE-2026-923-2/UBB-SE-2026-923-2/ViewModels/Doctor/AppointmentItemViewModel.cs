namespace UBB_SE_2026_923_2.ViewModels.Doctor
{
    using System;
    using UBB_SE_2026_923_2.Models;

    public class AppointmentItemViewModel
    {
        public int AppointmentItemId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string DateText => this.Date.ToString("dd MMM yyyy");

        public string Notes { get; set; } = string.Empty;

        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string TimeRangeText => $"{this.StartTime:hh\\:mm} - {this.EndTime:hh\\:mm}";

        public string LocationSafe => string.IsNullOrWhiteSpace(this.Location) ? "Location TBD" : this.Location;

        public AppointmentItemViewModel(Appointment item)
        {
            this.AppointmentItemId = item.Id;
            this.PatientName = item.PatientName ?? string.Empty;
            this.Date = item.Date;
            this.Notes = item.Notes ?? string.Empty;
            this.DoctorId = item.Doctor?.StaffID ?? 0;
            this.DoctorName = item.Doctor?.FullName ?? string.Empty;
            this.Type = item.Type ?? string.Empty;
            this.Location = item.Location ?? string.Empty;
            this.Status = item.Status ?? string.Empty;
            this.StartTime = item.StartTime;
            this.EndTime = item.EndTime;
        }

        public Appointment ToAppointment() => new Appointment
        {
            Id = this.AppointmentItemId,
            PatientName = this.PatientName,
            Doctor = new Doctor { StaffID = this.DoctorId },
            Date = this.Date,
            StartTime = this.StartTime,
            EndTime = this.EndTime,
            Status = this.Status,
            Type = this.Type,
            Location = this.Location,
            Notes = this.Notes,
        };
    }
}