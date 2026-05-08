namespace UBB_SE_2026_923_2.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    public class Hangout
    {
        private const string DateFormat = "yyyy-MM-dd";

        public int HangoutID { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string FormattedDate => this.Date.ToString(DateFormat);

        public int MaxParticipants { get; set; }

        // Legacy in-memory view — not persisted, IStaff cannot round-trip
        // through JSON (interface property), so omit it from API payloads.
        [NotMapped]
        [JsonIgnore]
        public List<IStaff> ParticipantList { get; } = new List<IStaff>();

        // ---- EF Core navigation collection (persisted) ----
        [JsonIgnore]
        public ICollection<HangoutParticipant> HangoutParticipantEntries { get; set; } = new List<HangoutParticipant>();

        // Parameterless constructor required by EF Core when materializing entities.
        public Hangout()
        {
        }

        public Hangout(int hangoutId, string title, string description, DateTime date, int maximumParticipants)
            : this()
        {
            this.HangoutID = hangoutId;
            this.Title = title;
            this.Description = description;
            this.Date = date;
            this.MaxParticipants = maximumParticipants;
            this.ParticipantList = new List<IStaff>(this.MaxParticipants);
        }
    }
}