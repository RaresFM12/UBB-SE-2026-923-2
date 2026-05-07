using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UBB_SE_2026_923_2.Models
{
    public class Shift
    {
        public int Id { get; set; }

        // ---- EF Core navigation property (persisted) ----
        public int StaffId { get; set; }
        public Staff Staff { get; set; } = null!;

        // Legacy interface-typed accessor preserved for existing call sites.
        // Delegates to the concrete Staff navigation so the EF-loaded entity
        // is visible through the old API. Phase 2 migrates callers onto Staff.
        // [JsonIgnore] — interface property cannot be deserialized over HTTP;
        // the Staff navigation already carries the same instance.
        [NotMapped]
        [JsonIgnore]
        public IStaff AppointedStaff
        {
            get => Staff;
            set => Staff = value as Staff
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

        public Shift(int id, IStaff appointedStaff, string location, DateTime startTime, DateTime endTime, ShiftStatus status)
        {
            this.Id = id;
            this.AppointedStaff = appointedStaff;
            this.StaffId = appointedStaff.StaffID;
            this.Location = location;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.Status = status;
        }
    }
}
