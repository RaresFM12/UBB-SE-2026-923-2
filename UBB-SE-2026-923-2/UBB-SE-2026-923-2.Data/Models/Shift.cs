namespace UBB_SE_2026_923_2.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    public class Shift
    {
        public int Id { get; set; }

        // EF Core navigation property — persisted via shadow FK column "StaffId".
        public Staff Staff { get; set; } = null!;

        // Legacy interface-typed accessor preserved for existing call sites.
        // Delegates to the concrete Staff navigation so the EF-loaded entity
        // is visible through the old API.
        [NotMapped]
        [JsonIgnore]
        public IStaff AppointedStaff
        {
            get => this.Staff;
            set => this.Staff = value as Staff
                ?? throw new ArgumentException(
                    "AppointedStaff must be a concrete Staff instance (Staff, Doctor or Pharmacyst).",
                    nameof(value));
        }

        public string Location { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public ShiftStatus Status { get; set; } = ShiftStatus.SCHEDULED;

        // Parameterless constructor required by EF Core when materializing entities.
        public Shift()
        {
        }

        public Shift(int identifier, IStaff appointedStaff, string location, DateTime startTime, DateTime endTime, ShiftStatus status)
        {
            this.Id = identifier;
            this.AppointedStaff = appointedStaff;
            this.Location = location;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.Status = status;
        }
    }
}
