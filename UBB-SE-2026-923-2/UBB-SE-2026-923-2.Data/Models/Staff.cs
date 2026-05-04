using System.Collections.Generic;

namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// TPH base type for hospital staff. <see cref="Doctor"/> and
    /// <see cref="Pharmacyst"/> derive from this class; the EF Core
    /// discriminator is the <see cref="Role"/> column.
    /// </summary>
    public class Staff : IStaff
    {
        public int StaffID { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public bool Available { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Certification { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public double HourlyRate { get; set; }

        // ---- EF Core navigation collections (persisted) ----
        public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<HangoutParticipant> HangoutParticipantEntries { get; set; } = new List<HangoutParticipant>();
        public ICollection<ShiftSwapRequest> ShiftSwapRequestsAsRequester { get; set; } = new List<ShiftSwapRequest>();
        public ICollection<ShiftSwapRequest> ShiftSwapRequestsAsColleague { get; set; } = new List<ShiftSwapRequest>();

        public Staff()
        {
        }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}