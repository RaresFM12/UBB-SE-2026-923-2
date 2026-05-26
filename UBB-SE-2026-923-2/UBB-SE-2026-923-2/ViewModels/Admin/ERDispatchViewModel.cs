namespace UBB_SE_2026_923_2.ViewModels.Admin
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using UBB_SE_2026_923_2.Command;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Base;

    public sealed class ERDispatchViewModel : ObservableObject
    {
        private const int OverrideWindowDays = 3;
        private const int MinutesPerHour = 60;
        private const int HoursPerDay = 24;
        private const int MinutesPerDay = HoursPerDay * MinutesPerHour;
        private const int NearEndMinutesThreshold = OverrideWindowDays * MinutesPerDay;
        private const int DefaultSimulatedRequestCount = 3;
        private const string UnknownDoctorName = "Unknown";
        private const string ManualOverrideHintText = "Manual override accepts IN_EXAMINATION doctors whose active shift ends within 3 days.";
        private readonly IERDispatchService dispatchService;

        public ObservableCollection<UnmatchedRequestRow> UnmatchedRequests { get; } = new ObservableCollection<UnmatchedRequestRow>();

        public ObservableCollection<SuccessfulMatchRow> SuccessfulMatches { get; } = new ObservableCollection<SuccessfulMatchRow>();

        public ObservableCollection<OverrideCandidateRow> OverrideCandidates { get; } = new ObservableCollection<OverrideCandidateRow>();

        private string statusMessage = "Ready";

        public string StatusMessage
        {
            get => this.statusMessage;
            private set => this.SetProperty(ref this.statusMessage, value);
        }

        private string manualInterventionHint = ManualOverrideHintText;

        public string ManualInterventionHint
        {
            get => this.manualInterventionHint;
            private set => this.SetProperty(ref this.manualInterventionHint, value);
        }

        public AsyncRelayCommand RunDispatchCommand { get; }

        public RelayCommand RefreshCommand { get; }

        public AsyncRelayCommand SimulateIncomingCommand { get; }

        public ERDispatchViewModel(IERDispatchService dispatchService)
        {
            this.dispatchService = dispatchService;
            this.RunDispatchCommand = new AsyncRelayCommand(this.RunDispatchAsync);
            bool CanRefresh() => this.UnmatchedRequests.Count > 0 || this.SuccessfulMatches.Count > 0;
            this.RefreshCommand = new RelayCommand(this.Refresh, CanRefresh);

            async Task SimulateDefaultCount() => await this.SimulateIncomingAsync(DefaultSimulatedRequestCount);
            this.SimulateIncomingCommand = new AsyncRelayCommand(SimulateDefaultCount);

            this.Refresh();
        }

        public void LoadFlaggedRequests() => this.Refresh();

        public Task HandleERRequestAsync(ERRequest request)
            => request == null ? Task.CompletedTask : this.HandleRequestByIdAsync(request.Id);

        public Task OverrideAssignmentAsync(int doctorId, int requestId)
            => this.ApplyOverrideAsync(requestId, doctorId);

        public void Refresh()
        {
            this.UnmatchedRequests.Clear();
            this.SuccessfulMatches.Clear();
            this.OverrideCandidates.Clear();
            this.StatusMessage = "Ready";
            this.ManualInterventionHint = ManualOverrideHintText;
        }

        public async Task SimulateIncomingAsync(int requestCount)
        {
            try
            {
                var createdRequestIds = await this.dispatchService.SimulateIncomingRequestsAsync(requestCount);
                this.StatusMessage = $"Simulated {createdRequestIds.Count} incoming request(s) from Clinical Team. Click Run Dispatch.";
                this.ManualInterventionHint = "Incoming ER requests were added as PENDING.";
            }
            catch (Exception exception)
            {
                this.StatusMessage = $"Error: {exception.Message}";
            }
        }

        public async Task RunDispatchAsync()
        {
            this.UnmatchedRequests.Clear();
            this.SuccessfulMatches.Clear();
            this.OverrideCandidates.Clear();
            this.StatusMessage = "Dispatching...";
            this.ManualInterventionHint = ManualOverrideHintText;

            try
            {
                var pendingRequestIds = await this.dispatchService.GetPendingRequestIdsAsync();

                foreach (var requestId in pendingRequestIds)
                {
                    await this.HandleRequestByIdAsync(requestId);
                }

                this.StatusMessage = $"{this.SuccessfulMatches.Count} matched, {this.UnmatchedRequests.Count} unmatched";

                if (this.UnmatchedRequests.Count > 0)
                {
                    await this.LoadOverrideCandidatesAsync(this.UnmatchedRequests.First().RequestId);
                }
                else
                {
                    this.ManualInterventionHint = "No unmatched requests. Override not needed.";
                }
            }
            catch (Exception exception)
            {
                this.StatusMessage = $"Error: {exception.Message}";
            }
        }

        public async Task LoadOverrideCandidatesAsync(int requestId)
        {
            var overrideCandidateDoctors = await this.dispatchService.GetManualOverrideCandidatesAsync(requestId, NearEndMinutesThreshold);
            this.OverrideCandidates.ReplaceWith(overrideCandidateDoctors.Select(OverrideCandidateRow.From));

            this.ManualInterventionHint = this.OverrideCandidates.Count == 0
                ? "No eligible override doctor found (need IN_EXAMINATION doctor ending within 3 days)."
                : $"Found {this.OverrideCandidates.Count} eligible override candidate(s).";
        }

        public async Task<bool> ApplyOverrideAsync(int requestId, int doctorId)
        {
            bool MatchesRequestId(UnmatchedRequestRow unmatchedRow) => unmatchedRow.RequestId == requestId;
            var unmatchedRequest = this.UnmatchedRequests.FirstOrDefault(MatchesRequestId);
            if (unmatchedRequest == null)
            {
                this.ManualInterventionHint = "Select an unmatched request first.";
                return false;
            }

            bool MatchesDoctorId(OverrideCandidateRow candidateRow) => candidateRow.DoctorId == doctorId;
            var overrideCandidate = this.OverrideCandidates.FirstOrDefault(MatchesDoctorId);
            if (overrideCandidate == null)
            {
                this.ManualInterventionHint = "Select an eligible override doctor first.";
                return false;
            }

            var overrideResult = await this.dispatchService.ManualOverrideAsync(requestId, doctorId, NearEndMinutesThreshold);
            if (!overrideResult.IsSuccess)
            {
                this.ManualInterventionHint = overrideResult.Message;
                return false;
            }

            this.ManualInterventionHint = overrideResult.Message;

            this.UnmatchedRequests.Remove(unmatchedRequest);
            this.SuccessfulMatches.Add(new SuccessfulMatchRow
            {
                RequestId = overrideResult.Request.Id,
                AssignedDoctor = overrideResult.MatchedDoctorName ?? UnknownDoctorName,
                Specialization = overrideResult.Request.Specialization,
                MatchReason = overrideResult.MatchReason,
            });

            this.StatusMessage = $"{this.SuccessfulMatches.Count} matched, {this.UnmatchedRequests.Count} unmatched";

            if (this.UnmatchedRequests.Count > 0)
            {
                await this.LoadOverrideCandidatesAsync(this.UnmatchedRequests.First().RequestId);
            }
            else
            {
                this.OverrideCandidates.Clear();
                this.ManualInterventionHint = "Override applied. No unmatched requests left.";
            }

            return true;
        }

        private async Task HandleRequestByIdAsync(int requestId)
        {
            var dispatchResult = await this.dispatchService.DispatchERRequestAsync(requestId);

            if (dispatchResult.IsSuccess)
            {
                this.SuccessfulMatches.Add(new SuccessfulMatchRow
                {
                    RequestId = dispatchResult.Request.Id,
                    AssignedDoctor = dispatchResult.MatchedDoctorName ?? UnknownDoctorName,
                    Specialization = dispatchResult.Request.Specialization,
                    MatchReason = dispatchResult.MatchReason,
                });
            }
            else
            {
                this.UnmatchedRequests.Add(new UnmatchedRequestRow
                {
                    RequestId = dispatchResult.Request.Id,
                    RequestSpecialization = dispatchResult.Request.Specialization,
                    RequestLocation = dispatchResult.Request.Location,
                    NoMatchReason = dispatchResult.Message,
                });
            }
        }

        public sealed class UnmatchedRequestRow
        {
            public int RequestId { get; set; }

            public string RequestSpecialization { get; set; } = string.Empty;

            public string RequestLocation { get; set; } = string.Empty;

            public string NoMatchReason { get; set; } = string.Empty;

            public string RequestLabel => $"#{this.RequestId} - {this.RequestSpecialization} @ {this.RequestLocation}";
        }

        public sealed class SuccessfulMatchRow
        {
            public int RequestId { get; set; }

            public string AssignedDoctor { get; set; } = string.Empty;

            public string Specialization { get; set; } = string.Empty;

            public string MatchReason { get; set; } = string.Empty;
        }

        public sealed class OverrideCandidateRow
        {
            public int DoctorId { get; set; }

            public string FullName { get; set; } = string.Empty;

            public int MinutesToEnd { get; set; }

            private bool HasKnownTimeRemaining => this.MinutesToEnd >= 0;

            public string DisplayLabel => this.HasKnownTimeRemaining
                ? $"{this.FullName} (ends in {this.TimeRemainingText})"
                : this.FullName;

            private string TimeRemainingText
            {
                get
                {
                    if (this.MinutesToEnd >= MinutesPerDay)
                    {
                        var days = this.MinutesToEnd / MinutesPerDay;
                        var hours = this.MinutesToEnd % MinutesPerDay / MinutesPerHour;
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

            public static OverrideCandidateRow From(DoctorProfile candidate) =>
                new OverrideCandidateRow
                {
                    DoctorId = candidate.DoctorId,
                    FullName = candidate.FullName,
                    MinutesToEnd = candidate.MinutesToEnd,
                };
        }
    }
}
