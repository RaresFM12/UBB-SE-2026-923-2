namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;

    public sealed class ERDispatchService : IERDispatchService
    {
        private const string PendingStatus = "PENDING";
        private const string AssignedStatus = "ASSIGNED";
        private const string UnmatchedStatus = "UNMATCHED";
        private const string DefaultSpecialization = "General";
        private const string FallbackLocation = "Ward A";
        private const string ERAssignmentNotificationTitle = "ER Assignment";

        private static readonly (string Specialization, string Location)[] FallbackTemplates =
        {
            ("Surgeon", FallbackLocation),
            ("Cardiology", FallbackLocation),
            ("Neurology", FallbackLocation),
            ("Pediatrics", FallbackLocation),
        };

        // Serializes match + status mutation so two concurrent dispatches
        // cannot pick the same AVAILABLE doctor.
        private readonly SemaphoreSlim dispatchLock = new SemaphoreSlim(1, 1);

        private readonly IERDispatchRepository requestRepository;
        private readonly IStaffRepository staffRepository;
        private readonly IShiftRepository shiftRepository;
        private readonly INotificationRepository? notificationRepository;

        public ERDispatchService(
            IERDispatchRepository requestRepository,
            IStaffRepository staffRepository,
            IShiftRepository shiftRepository,
            INotificationRepository? notificationRepository = null)
        {
            this.requestRepository = requestRepository;
            this.staffRepository = staffRepository;
            this.shiftRepository = shiftRepository;
            this.notificationRepository = notificationRepository;
        }

        public Task<IReadOnlyList<int>> SimulateIncomingRequestsAsync(int count)
        {
            bool HasSpecializationAndLocation(DoctorProfile doctor) =>
                !string.IsNullOrWhiteSpace(doctor.Specialization) && !string.IsNullOrWhiteSpace(doctor.Location);

            (string Specialization, string Location) ToSpecializationLocationPair(DoctorProfile doctor) =>
                (doctor.Specialization.Trim(), doctor.Location.Trim());

            var availableDoctors = GetAvailableDoctors(this.GetDoctorRosterForDispatch());
            var liveTemplates = availableDoctors
                .Where(HasSpecializationAndLocation)
                .Select(ToSpecializationLocationPair)
                .Distinct()
                .ToArray();

            var templates = liveTemplates.Length > 0 ? liveTemplates : FallbackTemplates;
            var normalizedCount = Math.Max(1, count);
            var startIndex = DateTime.Now.Minute % templates.Length;
            var createdIds = new List<int>(normalizedCount);

            for (int templateIndex = 0; templateIndex < normalizedCount; templateIndex++)
            {
                var template = templates[(startIndex + templateIndex) % templates.Length];
                var newId = this.requestRepository.AddRequest(template.Specialization, template.Location, PendingStatus);
                createdIds.Add(newId);
            }

            return Task.FromResult<IReadOnlyList<int>>(createdIds);
        }

        public Task<IReadOnlyList<int>> GetPendingRequestIdsAsync()
        {
            int ToRequestId(ERRequest request) => request.Id;

            var pendingIds = this.GetPendingRequests()
                .Select(ToRequestId)
                .ToList();
            return Task.FromResult<IReadOnlyList<int>>(pendingIds);
        }

        public async Task<ERDispatchResult> DispatchERRequestAsync(int requestId)
        {
            await this.dispatchLock.WaitAsync();
            try
            {
                bool HasMatchingId(ERRequest pendingRequest) => pendingRequest.Id == requestId;
                var request = this.GetPendingRequests().FirstOrDefault(HasMatchingId);
                if (request == null)
                {
                    return new ERDispatchResult
                    {
                        IsSuccess = false,
                        Message = $"ER request #{requestId} not found or already processed.",
                    };
                }

                var matchedDoctor = this.FindBestMatchingDoctor(request);

                if (matchedDoctor == null)
                {
                    this.requestRepository.UpdateRequestStatus(requestId, UnmatchedStatus, null, null);
                    return new ERDispatchResult
                    {
                        Request = request,
                        IsSuccess = false,
                        Message = $"No AVAILABLE {request.Specialization} specialist found for {request.Location}.",
                    };
                }

                this.requestRepository.UpdateRequestStatus(requestId, AssignedStatus, matchedDoctor.DoctorId, matchedDoctor.FullName);
                await this.staffRepository.UpdateStatusAsync(matchedDoctor.DoctorId, DoctorStatus.IN_EXAMINATION.ToString());

                this.NotifyER(matchedDoctor, request);

                return new ERDispatchResult
                {
                    Request = request,
                    MatchedDoctorId = matchedDoctor.DoctorId,
                    MatchedDoctorName = matchedDoctor.FullName,
                    MatchReason = $"Specialty match ({matchedDoctor.Specialization}) + AVAILABLE status + at {request.Location}",
                    IsSuccess = true,
                    Message = $"Assigned to {matchedDoctor.FullName}. Status changed to IN_EXAMINATION.",
                };
            }
            finally
            {
                this.dispatchLock.Release();
            }
        }

        private void NotifyER(DoctorProfile matchedDoctor, ERRequest request)
        {
            if (this.notificationRepository == null)
            {
                return;
            }

            string message = $"You have been assigned to ER request #{request.Id} ({request.Specialization}) at {request.Location}.";
            this.notificationRepository.AddNotification(matchedDoctor.DoctorId, ERAssignmentNotificationTitle, message);
        }

        public Task<IReadOnlyList<DoctorProfile>> GetManualOverrideCandidatesAsync(int requestId, int nearEndMinutes)
        {
            var request = this.requestRepository.GetRequestById(requestId);
            if (request == null)
            {
                return Task.FromResult<IReadOnlyList<DoctorProfile>>(Array.Empty<DoctorProfile>());
            }

            var now = DateTime.Now;
            var inExaminationDoctors = GetDoctorsInExamination(this.GetDoctorRosterForDispatch());

            bool HasScheduleEnd(DoctorProfile doctor) => doctor.ScheduleEnd.HasValue;
            bool IsNearEnd(DoctorProfile doctor)
            {
                var minutesToEnd = (doctor.ScheduleEnd!.Value - now).TotalMinutes;
                return minutesToEnd >= 0 && minutesToEnd <= nearEndMinutes;
            }

            bool MatchesRequestSpecialization(DoctorProfile doctor) =>
                IsSameValue(doctor.Specialization, request.Specialization);

            int ByDoctorId(DoctorProfile doctor) => doctor.DoctorId;
            DoctorProfile FirstInGroup(IGrouping<int, DoctorProfile> doctorGroup) => doctorGroup.First();
            DateTime ByScheduleEndOrMax(DoctorProfile doctor) => doctor.ScheduleEnd ?? DateTime.MaxValue;
            string ByFullName(DoctorProfile doctor) => doctor.FullName;

            var candidates = inExaminationDoctors
                .Where(HasScheduleEnd)
                .Where(IsNearEnd)
                .Where(MatchesRequestSpecialization)
                .GroupBy(ByDoctorId)
                .Select(FirstInGroup)
                .OrderBy(ByScheduleEndOrMax)
                .ThenBy(ByFullName)
                .ToList();

            return Task.FromResult<IReadOnlyList<DoctorProfile>>(candidates);
        }

        public async Task<ERDispatchResult> ManualOverrideAsync(int requestId, int doctorId, int nearEndMinutes)
        {
            await this.dispatchLock.WaitAsync();
            try
            {
                var request = this.requestRepository.GetRequestById(requestId);
                bool HasMatchingDoctorId(DoctorProfile rosterEntry) => rosterEntry.DoctorId == doctorId;
                var doctor = this.GetDoctorRosterForDispatch().FirstOrDefault(HasMatchingDoctorId);

                if (request == null || doctor == null)
                {
                    return new ERDispatchResult
                    {
                        IsSuccess = false,
                        Message = "Request or doctor not found.",
                    };
                }

                var eligibleCandidates = await this.GetManualOverrideCandidatesAsync(requestId, nearEndMinutes);
                bool HasMatchingDoctorIdInCandidates(DoctorProfile overrideCandidate) => overrideCandidate.DoctorId == doctorId;
                if (!eligibleCandidates.Any(HasMatchingDoctorIdInCandidates))
                {
                    return new ERDispatchResult
                    {
                        Request = request,
                        IsSuccess = false,
                        Message = $"Manual override blocked. Doctor must be IN_EXAMINATION within {nearEndMinutes} min of end_time.",
                    };
                }

                this.requestRepository.UpdateRequestStatus(requestId, AssignedStatus, doctor.DoctorId, doctor.FullName);
                await this.staffRepository.UpdateStatusAsync(doctor.DoctorId, DoctorStatus.IN_EXAMINATION.ToString());

                this.NotifyER(doctor, request);

                return new ERDispatchResult
                {
                    Request = request,
                    MatchedDoctorId = doctor.DoctorId,
                    MatchedDoctorName = doctor.FullName,
                    MatchReason = $"Manual override by administrator ({nearEndMinutes} min near end_time rule)",
                    IsSuccess = true,
                    Message = $"Manually assigned to {doctor.FullName}. Status changed to IN_EXAMINATION.",
                };
            }
            finally
            {
                this.dispatchLock.Release();
            }
        }

        private DoctorProfile? FindBestMatchingDoctor(ERRequest request)
        {
            var availableDoctors = GetAvailableDoctors(this.GetDoctorRosterForDispatch());

            bool IsMatchingAvailableDoctor(DoctorProfile doctor) =>
                IsSameValue(doctor.Specialization, request.Specialization)
                && doctor.Status == DoctorStatus.AVAILABLE
                && IsSameValue(doctor.Location, request.Location);

            string ByFullName(DoctorProfile doctor) => doctor.FullName;

            return availableDoctors
                .Where(IsMatchingAvailableDoctor)
                .OrderBy(ByFullName)
                .FirstOrDefault();
        }

        private IReadOnlyList<DoctorProfile> GetDoctorRosterForDispatch()
        {
            var now = DateTime.Now;
            var allShifts = this.shiftRepository.GetAllShifts();
            var allStaff = this.staffRepository.LoadAllStaff();

            bool IsCurrentNonCancelledShift(Shift shift) =>
                shift.StartTime <= now
                && shift.EndTime >= now
                && shift.Status != ShiftStatus.CANCELLED
                && shift.Status != ShiftStatus.COMPLETED
                && shift.Status != ShiftStatus.VACATION;

            int ByAppointedStaffId(Shift shift) => shift.AppointedStaff.StaffID;
            int GroupKey(IGrouping<int, Shift> shiftGroup) => shiftGroup.Key;
            DateTime ByShiftEndTime(Shift shift) => shift.EndTime;
            Shift EarliestEndingShiftInGroup(IGrouping<int, Shift> shiftGroup) =>
                shiftGroup.OrderBy(ByShiftEndTime).First();

            var currentShiftsByStaffId = allShifts
                .Where(IsCurrentNonCancelledShift)
                .GroupBy(ByAppointedStaffId)
                .ToDictionary(GroupKey, EarliestEndingShiftInGroup);

            var roster = new List<DoctorProfile>();
            foreach (var staffMember in allStaff.OfType<Doctor>())
            {
                if (!currentShiftsByStaffId.TryGetValue(staffMember.StaffID, out var currentShift))
                {
                    continue;
                }

                roster.Add(new DoctorProfile
                {
                    DoctorId = staffMember.StaffID,
                    FullName = $"{staffMember.FirstName} {staffMember.LastName}".Trim(),
                    Specialization = string.IsNullOrWhiteSpace(staffMember.Specialization) ? DefaultSpecialization : staffMember.Specialization.Trim(),
                    Status = staffMember.DoctorStatus,
                    Location = (currentShift.Location ?? string.Empty).Trim(),
                    ScheduleStart = currentShift.StartTime,
                    ScheduleEnd = currentShift.EndTime,
                });
            }

            return roster;
        }

        private IReadOnlyList<ERRequest> GetPendingRequests()
        {
            bool IsPending(ERRequest request) =>
                string.Equals((request.Status ?? string.Empty).Trim(), PendingStatus, StringComparison.OrdinalIgnoreCase);
            DateTime ByCreatedAt(ERRequest request) => request.CreatedAt;

            return this.requestRepository.GetAllRequests()
                .Where(IsPending)
                .OrderBy(ByCreatedAt)
                .ToList();
        }

        private static IReadOnlyList<DoctorProfile> GetAvailableDoctors(IEnumerable<DoctorProfile> roster)
        {
            bool IsAvailable(DoctorProfile doctor) => doctor.Status == DoctorStatus.AVAILABLE;
            return roster.Where(IsAvailable).ToList();
        }

        private static IReadOnlyList<DoctorProfile> GetDoctorsInExamination(IEnumerable<DoctorProfile> roster)
        {
            bool IsInExamination(DoctorProfile doctor) => doctor.Status == DoctorStatus.IN_EXAMINATION;
            return roster.Where(IsInExamination).ToList();
        }

        private static bool IsSameValue(string leftOperator, string rightOperator) =>
            string.Equals((leftOperator ?? string.Empty).Trim(), (rightOperator ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
