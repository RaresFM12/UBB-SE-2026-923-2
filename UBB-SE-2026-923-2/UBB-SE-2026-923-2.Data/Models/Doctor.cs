namespace UBB_SE_2026_923_2.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Doctor staff. Stored in the same table as <see cref="Staff"/> via TPH
    /// inheritance; the discriminator is <see cref="Staff.Role"/> = "Doctor".
    /// </summary>
    public class Doctor : Staff
    {
        public DoctorStatus DoctorStatus { get; set; }

        // ---- EF Core navigation collections (persisted) ----
        [JsonIgnore]
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        [JsonIgnore]
        public ICollection<MedicalEvaluation> MedicalEvaluations { get; set; } = new List<MedicalEvaluation>();

        public Doctor()
        {
            this.Role = "Doctor";
        }

        public Doctor(int staffId, string firstName, string lastName, string contactInformation, bool isAvailable,
            string specialization, string licenseNumber, DoctorStatus doctorStatus, int yearsOfExperience)
        {
            this.StaffID = staffId;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.ContactInfo = contactInformation;
            this.Available = isAvailable;
            this.Specialization = specialization;
            this.LicenseNumber = licenseNumber;
            this.DoctorStatus = doctorStatus;
            this.YearsOfExperience = yearsOfExperience;
            this.Role = "Doctor";
        }
    }
}