namespace UBB_SE_2026_923_2.Web.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;
    using UBB_SE_2026_923_2.Models;

    public class ErDispatchDashboardViewModel
    {
        public string StatusMessage { get; set; } = "Ready";

        public string ManualInterventionHint { get; set; } = "Manual override accepts IN_EXAMINATION doctors whose active shift ends within 3 days.";

        public int? SelectedRequestId { get; set; }

        public int? SelectedDoctorId { get; set; }

        public List<UnmatchedERRequestViewModel> SessionUnmatched { get; set; } = new List<UnmatchedERRequestViewModel>();

        public List<SuccessfulERMatchViewModel> SessionMatches { get; set; } = new List<SuccessfulERMatchViewModel>();

        public List<OverrideCandidateViewModel> OverrideCandidates { get; set; } = new List<OverrideCandidateViewModel>();

        [JsonIgnore]
        public IReadOnlyList<ERRequest> Pending { get; init; } = new List<ERRequest>();

        [JsonIgnore]
        public IReadOnlyList<ERRequest> Assigned { get; init; } = new List<ERRequest>();

        [JsonIgnore]
        public IReadOnlyList<ERRequest> Unmatched { get; init; } = new List<ERRequest>();

        [JsonIgnore]
        public IReadOnlyList<ERRequest> Cancelled { get; init; } = new List<ERRequest>();
    }

    public class CreateERRequestViewModel
    {
        [Required(ErrorMessage = "Specialization is required.")]
        [Display(Name = "Specialization")]
        public string Specialization { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;
    }

    public class EditERRequestStatusViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Specialization")]
        public string Specialization { get; set; } = string.Empty;

        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        public static IReadOnlyList<string> AvailableStatuses { get; } = new[]
        {
            "PENDING",
            "CANCELLED",
        };
    }

    public class ErDispatchTableViewModel
    {
        public string Title { get; init; } = string.Empty;

        public string HeaderCss { get; init; } = string.Empty;

        public string Kind { get; init; } = string.Empty;

        public IReadOnlyList<ERRequest> Rows { get; init; } = new List<ERRequest>();
    }

    public class UnmatchedERRequestViewModel
    {
        public int RequestId { get; set; }

        public string RequestSpecialization { get; set; } = string.Empty;

        public string RequestLocation { get; set; } = string.Empty;

        public string NoMatchReason { get; set; } = string.Empty;

        public string RequestLabel => $"#{this.RequestId} - {this.RequestSpecialization} @ {this.RequestLocation}";
    }

    public class SuccessfulERMatchViewModel
    {
        public int RequestId { get; set; }

        public string AssignedDoctor { get; set; } = string.Empty;

        public string Specialization { get; set; } = string.Empty;

        public string MatchReason { get; set; } = string.Empty;
    }

    public class OverrideCandidateViewModel
    {
        private const int MinutesPerHour = 60;
        private const int HoursPerDay = 24;
        private const int MinutesPerDay = HoursPerDay * MinutesPerHour;

        public int DoctorId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public int MinutesToEnd { get; set; }

        public string DisplayLabel => this.MinutesToEnd >= 0
            ? $"{this.FullName} (ends in {this.TimeRemainingText})"
            : this.FullName;

        private string TimeRemainingText
        {
            get
            {
                if (this.MinutesToEnd >= MinutesPerDay)
                {
                    var days = this.MinutesToEnd / MinutesPerDay;
                    var hours = (this.MinutesToEnd % MinutesPerDay) / MinutesPerHour;
                    return $"{days}d {hours}h";
                }

                if (this.MinutesToEnd >= MinutesPerHour)
                {
                    var hours = this.MinutesToEnd / MinutesPerHour;
                    var minutes = this.MinutesToEnd % MinutesPerHour;
                    return $"{hours}h {minutes}m";
                }

                return $"{this.MinutesToEnd} min";
            }
        }

        public static OverrideCandidateViewModel From(DoctorProfile candidate) =>
            new OverrideCandidateViewModel
            {
                DoctorId = candidate.DoctorId,
                FullName = candidate.FullName,
                MinutesToEnd = candidate.MinutesToEnd,
            };
    }

    public class OverrideViewModel
    {
        public int RequestId { get; set; }

        public string RequestSummary { get; set; } = string.Empty;

        public int SelectedDoctorId { get; set; }

        public IReadOnlyList<OverrideCandidateViewModel> Candidates { get; init; } = new List<OverrideCandidateViewModel>();
    }
}
