namespace UBB_SE_2026_923_2.Web.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public sealed class DoctorAppointmentListViewModel
    {
        public int? SelectedDoctorId { get; set; }

        public List<DoctorOptionViewModel> Doctors { get; set; } = new();

        public List<DoctorAppointmentListItemViewModel> Appointments { get; set; } = new();

        public string? ErrorMessage { get; set; }
    }

    public sealed class DoctorAppointmentListItemViewModel
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string Status { get; set; } = string.Empty;
    }

    public sealed class DoctorAppointmentCreateViewModel
    {
        [Required(ErrorMessage = "Patient id is required.")]
        [RegularExpression(@"^(PAT\s*-\s*)?\d+$", ErrorMessage = "Patient id must be numeric, for example 123 or PAT-123.")]
        [Display(Name = "Patient id")]
        public string PatientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doctor is required.")]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Start time is required.")]
        [DataType(DataType.Time)]
        [Display(Name = "Start time")]
        public TimeSpan StartTime { get; set; } = TimeSpan.FromHours(9);

        public List<DoctorOptionViewModel> Doctors { get; set; } = new();
    }

    public sealed class DoctorAppointmentEditViewModel
    {
        public int Id { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = string.Empty;

        public IReadOnlyList<string> StatusOptions { get; } = new List<string>
        {
            "Finished",
        };
    }

    public sealed class DoctorAppointmentDeleteViewModel
    {
        public int Id { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
    }

    public sealed class DoctorAppointmentDetailsViewModel
    {
        public int Id { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string Status { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }

    public sealed class DoctorAppointmentScheduleViewModel
    {
        public DateTime FromDate { get; set; } = DateTime.Today;

        public DateTime ToDate { get; set; } = DateTime.Today.AddDays(7);

        public List<DoctorAppointmentListItemViewModel> Appointments { get; set; } = new();

        public string? ErrorMessage { get; set; }
    }

    public sealed class DoctorOptionViewModel
    {
        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public static DoctorOptionViewModel From((int DoctorId, string DoctorName) doctor) =>
            new DoctorOptionViewModel
            {
                DoctorId = doctor.DoctorId,
                DoctorName = string.IsNullOrWhiteSpace(doctor.DoctorName)
                    ? $"Doctor #{doctor.DoctorId}"
                    : doctor.DoctorName,
            };
    }
}
