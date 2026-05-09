namespace UBB_SE_2026_923_2.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Appointment
    {
        public int Id { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string Status { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        // External patient identifier supplied by the Patient Management team.
        // Not persisted as a separate column; populated by services from
        // PatientName so callers can rely on a stable name for cross-team data.
        [NotMapped]
        public string ExternalRefId { get; set; } = string.Empty;

        // EF Core navigation property — persisted via shadow FK column "DoctorId".
        // Optional: appointments created through the integration-test path may have no doctor assigned.
        public Doctor? Doctor { get; set; }
    }
}
