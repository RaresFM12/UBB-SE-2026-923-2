using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace UBB_SE_2026_923_2.Models
{
    public class Hangout
    {
        private const string DateFormat = "yyyy-MM-dd";

        public int HangoutID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string FormattedDate => Date.ToString(DateFormat);
        public int MaxParticipants { get; set; }

        // Legacy in-memory view — not persisted. Phase 2 will migrate callers
        // onto HangoutParticipantEntries below.
        [NotMapped]
        public List<IStaff> ParticipantList { get; } = new List<IStaff>();

        // ---- EF Core navigation collection (persisted) ----
        public ICollection<HangoutParticipant> HangoutParticipantEntries { get; set; } = new List<HangoutParticipant>();

        // Parameterless constructor required by EF Core when materializing entities.
        public Hangout()
        {
        }

        public Hangout(int hangoutID, string title, string description, DateTime date, int maxParticipants)
            : this()
        {
            this.HangoutID = hangoutID;
            this.Title = title;
            this.Description = description;
            this.Date = date;
            this.MaxParticipants = maxParticipants;
            this.ParticipantList = new List<IStaff>(this.MaxParticipants);
        }
    }
}