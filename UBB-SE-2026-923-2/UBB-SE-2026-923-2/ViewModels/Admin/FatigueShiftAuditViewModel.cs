namespace UBB_SE_2026_923_2.ViewModels.Admin
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using UBB_SE_2026_923_2.Command;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Base;

    public sealed class FatigueShiftAuditViewModel : ObservableObject
    {
        private const string EnglishCultureCode = "en-US";
        private const string WeeklyDateFormat = "dd MMM yyyy";
        private const string ShiftTimeFormat = "ddd HH:mm";

        private static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo(EnglishCultureCode);

        private readonly IFatigueAuditService auditService;

        public ObservableCollection<AuditViolationRow> Violations { get; } = new ObservableCollection<AuditViolationRow>();

        public ObservableCollection<AutoSuggestRow> Suggestions { get; } = new ObservableCollection<AutoSuggestRow>();

        private DateTimeOffset selectedWeekStart = new DateTimeOffset(StartOfWeek(DateTime.Today));

        public DateTimeOffset SelectedWeekStart
        {
            get => this.selectedWeekStart;
            set
            {
                var normalized = new DateTimeOffset(StartOfWeek(value.Date));
                if (this.SetProperty(ref this.selectedWeekStart, normalized))
                {
                    this.RaisePropertyChanged(nameof(this.WeekLabel));
                    this.RunAutoAuditCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string WeekLabel =>
            $"Week of {this.SelectedWeekStart.ToString(WeeklyDateFormat, EnglishCulture)}";

        private string statusMessage = "Run Auto-Audit to validate this roster.";

        public string StatusMessage
        {
            get => this.statusMessage;
            set => this.SetProperty(ref this.statusMessage, value);
        }

        private bool canPublish;

        public bool CanPublish
        {
            get => this.canPublish;
            private set
            {
                if (this.SetProperty(ref this.canPublish, value))
                {
                    this.RaisePropertyChanged(nameof(this.PublishStatus));
                    this.RaisePropertyChanged(nameof(this.PublishStatusDescription));
                }
            }
        }

        public string PublishStatus => this.CanPublish ? "Publish status: READY" : "Publish status: BLOCKED";

        public string PublishStatusDescription => this.CanPublish
            ? "? No violations detected. Roster is ready to publish."
            : "Roster cannot be published while violations exist. Run audit and resolve all conflicts.";

        public RelayCommand RunAutoAuditCommand { get; }

        public FatigueShiftAuditViewModel(IFatigueAuditService auditService)
        {
            this.auditService = auditService;

            this.RunAutoAuditCommand = new RelayCommand(this.RunAutoAudit, () => true);

            this.RunAutoAudit();
        }

        public void RunAutoAudit()
        {
            var auditResult = this.auditService.RunAutoAudit(this.SelectedWeekStart.Date);

            this.Violations.Clear();
            DateTime GetViolationShiftStart(Models.AuditViolation violation) => violation.ShiftStart;
            foreach (var violation in auditResult.Violations.OrderBy(GetViolationShiftStart))
            {
                this.Violations.Add(new AuditViolationRow
                {
                    ShiftId = violation.ShiftId,
                    Staff = violation.StaffName,
                    Window = $"{violation.ShiftStart.ToString(ShiftTimeFormat, EnglishCulture)} - {violation.ShiftEnd.ToString(ShiftTimeFormat, EnglishCulture)}",
                    Rule = violation.Rule,
                    Message = violation.Message,
                });
            }

            this.Suggestions.Clear();
            int GetSuggestionShiftId(Models.AutoSuggestRecommendation suggestion) => suggestion.ShiftId;
            foreach (var suggestion in auditResult.Suggestions.OrderBy(GetSuggestionShiftId))
            {
                this.Suggestions.Add(new AutoSuggestRow
                {
                    ShiftId = suggestion.ShiftId,
                    OriginalStaffId = suggestion.OriginalStaffId,
                    OriginalStaffName = suggestion.OriginalStaffName,
                    Reason = suggestion.Reason,
                    SuggestedStaffId = suggestion.SuggestedStaffId,
                    SuggestedStaffName = suggestion.SuggestedStaffName,
                });
            }

            this.CanPublish = auditResult.CanPublish;
            this.StatusMessage = auditResult.Summary;
            this.RaisePropertyChanged(nameof(this.HasConflicts));
        }

        public bool HasConflicts => this.Violations.Any();

        public ReassignmentResult ApplyReassignment(int shiftId)
        {
            bool IsMatchingShift(AutoSuggestRow auditSuggestion) => auditSuggestion.ShiftId == shiftId;
            var matchedSuggestion = this.Suggestions.FirstOrDefault(IsMatchingShift);
            if (matchedSuggestion == null || !matchedSuggestion.SuggestedStaffId.HasValue)
            {
                return new ReassignmentResult(
                    false,
                    "Invalid Reassignment",
                    "No valid reassignment candidate found for this shift.");
            }

            bool reassignmentSucceeded = this.auditService.ReassignShift(shiftId, matchedSuggestion.SuggestedStaffId.Value);
            if (!reassignmentSucceeded)
            {
                return new ReassignmentResult(
                    false,
                    "Reassignment Failed",
                    "Could not reassign shift. Please try again.");
            }

            this.RunAutoAudit();

            return new ReassignmentResult(
                true,
                "Reassignment Applied",
                $"Shift #{shiftId} has been reassigned to {matchedSuggestion.SuggestedStaffName}.\n\nAudit was re-run to verify changes.");
        }

        public sealed record ReassignmentResult(bool IsSuccess, string Title, string Message);

        private static DateTime StartOfWeek(DateTime date)
        {
            const int daysInWeek = 7;
            var daysFromMonday = (daysInWeek + (date.DayOfWeek - DayOfWeek.Monday)) % daysInWeek;
            return date.Date.AddDays(-daysFromMonday);
        }

        public sealed class AuditViolationRow
        {
            public int ShiftId { get; set; }

            public string Staff { get; set; } = string.Empty;

            public string Window { get; set; } = string.Empty;

            public string Rule { get; set; } = string.Empty;

            public string Message { get; set; } = string.Empty;
        }

        public sealed class AutoSuggestRow
        {
            public int ShiftId { get; set; }

            public int OriginalStaffId { get; set; }

            public string OriginalStaffName { get; set; } = string.Empty;

            public string Reason { get; set; } = string.Empty;

            public int? SuggestedStaffId { get; set; }

            public string SuggestedStaffName { get; set; } = string.Empty;

            public bool HasSuggestedStaff => this.SuggestedStaffId.HasValue;

            public string OriginalStaffDisplay => $"{this.OriginalStaffName} (ID {this.OriginalStaffId})";

            public string SuggestedStaffDisplay => this.HasSuggestedStaff
                ? $"{this.SuggestedStaffName} (ID {this.SuggestedStaffId})"
                : "None available";
        }
    }
}
