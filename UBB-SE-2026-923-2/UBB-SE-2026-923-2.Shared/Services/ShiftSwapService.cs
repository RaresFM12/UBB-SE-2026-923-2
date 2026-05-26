namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;

    public class ShiftSwapService : IShiftSwapService
    {
        private const string AcceptedStatus = "ACCEPTED";
        private const string RejectedStatus = "REJECTED";
        private const string SwapRequestNotificationTitle = "New Shift Swap Request";
        private const string SwapAcceptedNotificationTitle = "Shift Swap Accepted";
        private const string SwapRejectedNotificationTitle = "Shift Swap Rejected";

        private readonly IStaffRepository staffRepository;
        private readonly IShiftRepository shiftRepository;
        private readonly IShiftSwapRepository shiftSwapRepository;
        private readonly INotificationRepository notificationRepository;

        public ShiftSwapService(
            IStaffRepository staffRepository,
            IShiftRepository shiftRepository,
            IShiftSwapRepository shiftSwapRepository,
            INotificationRepository notificationRepository)
        {
            this.staffRepository = staffRepository;
            this.shiftRepository = shiftRepository;
            this.shiftSwapRepository = shiftSwapRepository;
            this.notificationRepository = notificationRepository;
        }

        public List<Shift> GetFutureShiftsForStaff(int staffId)
        {
            var allShifts = this.shiftRepository.GetAllShifts();
            var now = DateTime.Now;

            bool IsFutureShiftForStaff(Shift shift) =>
                shift.AppointedStaff.StaffID == staffId && shift.StartTime > now;

            return allShifts.Where(IsFutureShiftForStaff).OrderBy(shifts => shifts.StartTime).ToList();
        }

        private static string NormalizeForComparison(string? text) => (text ?? string.Empty).Trim().ToLowerInvariant();

        public List<IStaff> GetEligibleSwapColleaguesForShift(int requesterId, int shiftId, out string error)
        {
            error = string.Empty;

            var allShifts = this.shiftRepository.GetAllShifts();
            bool HasMatchingShiftId(Shift existingShift) => existingShift.Id == shiftId;
            var shift = allShifts.FirstOrDefault(HasMatchingShiftId);
            if (shift == null)
            {
                error = "Shift not found.";
                return new List<IStaff>();
            }

            if (shift.AppointedStaff.StaffID != requesterId)
            {
                error = "You can only request swap for your own shift.";
                return new List<IStaff>();
            }

            if (shift.StartTime <= DateTime.Now)
            {
                error = "You can only request swap for a future shift.";
                return new List<IStaff>();
            }

            var allStaff = this.staffRepository.LoadAllStaff();
            bool HasRequesterId(IStaff staffMember) => staffMember.StaffID == requesterId;
            var requester = allStaff.FirstOrDefault(HasRequesterId);
            if (requester == null)
            {
                error = "Requester not found.";
                return new List<IStaff>();
            }

            var colleaguesWithSameProfile = new List<IStaff>();

            bool IsScheduledOrActive(Shift scheduledShift) =>
                scheduledShift.Status == ShiftStatus.SCHEDULED || scheduledShift.Status == ShiftStatus.ACTIVE;

            if (requester is Doctor requestingDoctor)
            {
                var requesterSpecialization = NormalizeForComparison(requestingDoctor.Specialization);
                bool HasMatchingSpecialization(Doctor doctor) =>
                    doctor.StaffID != requesterId && NormalizeForComparison(doctor.Specialization) == requesterSpecialization;

                colleaguesWithSameProfile = allStaff
                    .OfType<Doctor>()
                    .Where(HasMatchingSpecialization)
                    .Cast<IStaff>()
                    .ToList();
            }
            else if (requester is Pharmacyst requestingPharmacist)
            {
                var requesterCertification = NormalizeForComparison(requestingPharmacist.Certification);
                bool HasMatchingCertification(Pharmacyst pharmacist) =>
                    pharmacist.StaffID != requesterId && NormalizeForComparison(pharmacist.Certification) == requesterCertification;

                colleaguesWithSameProfile = allStaff
                    .OfType<Pharmacyst>()
                    .Where(HasMatchingCertification)
                    .Cast<IStaff>()
                    .ToList();
            }

            bool HasNoOverlappingShifts(IStaff colleague)
            {
                bool OverlapsTargetShift(Shift existingShift) =>
                    existingShift.AppointedStaff.StaffID == colleague.StaffID
                    && existingShift.StartTime < shift.EndTime
                    && existingShift.EndTime > shift.StartTime
                    && IsScheduledOrActive(existingShift);
                return !allShifts.Any(OverlapsTargetShift);
            }

            return colleaguesWithSameProfile
                .Where(HasNoOverlappingShifts)
                .ToList();
        }

        public bool RequestShiftSwap(int requesterId, int shiftId, int colleagueId, out string message)
        {
            message = string.Empty;

            var eligibleColleagues = this.GetEligibleSwapColleaguesForShift(requesterId, shiftId, out var validationError);
            if (!string.IsNullOrWhiteSpace(validationError))
            {
                message = validationError;
                return false;
            }

            bool HasMatchingColleagueId(IStaff colleague) => colleague.StaffID == colleagueId;
            if (!eligibleColleagues.Any(HasMatchingColleagueId))
            {
                message = "Selected colleague is not eligible (must be same profile and free in interval).";
                return false;
            }

            bool HasMatchingShiftIdInLookup(Shift existingShift) => existingShift.Id == shiftId;
            var shift = this.shiftRepository.GetAllShifts().FirstOrDefault(HasMatchingShiftIdInLookup);
            var requester = this.staffRepository.GetStaffById(requesterId);
            if (requester == null)
            {
                message = "Requester not found.";
                return false;
            }

            var colleague = this.staffRepository.GetStaffById(colleagueId);
            if (colleague == null)
            {
                message = "Colleague not found.";
                return false;
            }

            var swapRequest = new ShiftSwapRequest(
                0,
                shift!,
                requester as Staff ?? new Staff { StaffID = requesterId },
                colleague as Staff ?? new Staff { StaffID = colleagueId });

            var swapId = this.shiftSwapRepository.AddShiftSwapRequest(swapRequest);
            if (swapId <= 0)
            {
                message = "Failed to create shift swap request.";
                return false;
            }

            this.notificationRepository.AddNotification(
                colleagueId,
                SwapRequestNotificationTitle,
                $"You received a shift swap request from {requester.FirstName} {requester.LastName} for shift #{shiftId} ({shift!.StartTime:yyyy-MM-dd HH:mm} - {shift.EndTime:HH:mm}).");

            message = "Shift swap request sent successfully.";
            return true;
        }

        public List<ShiftSwapRequest> GetIncomingSwapRequests(int colleagueId)
        {
            bool IsPendingForColleague(ShiftSwapRequest swapRequest) =>
                swapRequest.Colleague.StaffID == colleagueId && swapRequest.Status == ShiftSwapRequestStatus.PENDING;
            DateTime ByRequestedAt(ShiftSwapRequest swapRequest) => swapRequest.RequestedAt;

            return this.shiftSwapRepository.GetAllShiftSwapRequests()
                .Where(IsPendingForColleague)
                .OrderByDescending(ByRequestedAt)
                .ToList();
        }

        public bool AcceptSwapRequest(int swapId, int colleagueId, out string message)
        {
            message = string.Empty;

            var swapRequest = this.shiftSwapRepository.GetShiftSwapRequestById(swapId);
            if (swapRequest == null)
            {
                message = "Swap request not found.";
                return false;
            }

            if (swapRequest.Colleague.StaffID != colleagueId)
            {
                message = "You cannot accept this request.";
                return false;
            }

            if (swapRequest.Status != ShiftSwapRequestStatus.PENDING)
            {
                message = "This request is no longer pending.";
                return false;
            }

            var allShifts = this.shiftRepository.GetAllShifts();
            bool HasTargetShiftId(Shift existingShift) => existingShift.Id == swapRequest.Shift.Id;
            var shift = allShifts.FirstOrDefault(HasTargetShiftId);
            if (shift == null)
            {
                message = "Shift not found.";
                return false;
            }

            bool IsScheduledOrActive(Shift scheduledShift) =>
                scheduledShift.Status == ShiftStatus.SCHEDULED || scheduledShift.Status == ShiftStatus.ACTIVE;

            bool ColleagueOverlapsTargetShift(Shift existingShift) =>
                existingShift.AppointedStaff.StaffID == colleagueId
                && existingShift.StartTime < shift.EndTime
                && existingShift.EndTime > shift.StartTime
                && IsScheduledOrActive(existingShift);

            if (allShifts.Any(ColleagueOverlapsTargetShift))
            {
                message = "You are already scheduled to work in that interval.";
                return false;
            }

            this.shiftRepository.UpdateShiftStaffId(swapRequest.Shift.Id, colleagueId);
            this.shiftSwapRepository.UpdateShiftSwapRequestStatus(swapId, AcceptedStatus);
            this.notificationRepository.AddNotification(
                swapRequest.Requester.StaffID,
                SwapAcceptedNotificationTitle,
                $"Your swap request #{swapId} was accepted.");

            message = "Swap accepted.";
            return true;
        }

        public List<Doctor> GetAllDoctors()
        {
            string ByFirstName(Doctor doctor) => doctor.FirstName;
            string ByLastName(Doctor doctor) => doctor.LastName;

            return this.staffRepository.LoadAllStaff()
                .OfType<Doctor>()
                .OrderBy(ByFirstName)
                .ThenBy(ByLastName)
                .ToList();
        }

        public bool RejectSwapRequest(int swapId, int colleagueId, out string message)
        {
            message = string.Empty;

            var swapRequest = this.shiftSwapRepository.GetShiftSwapRequestById(swapId);
            if (swapRequest == null)
            {
                message = "Swap request not found.";
                return false;
            }

            if (swapRequest.Colleague.StaffID != colleagueId)
            {
                message = "You cannot reject this request.";
                return false;
            }

            if (swapRequest.Status != ShiftSwapRequestStatus.PENDING)
            {
                message = "This request is no longer pending.";
                return false;
            }

            this.shiftSwapRepository.UpdateShiftSwapRequestStatus(swapId, RejectedStatus);
            this.notificationRepository.AddNotification(
                swapRequest.Requester.StaffID,
                SwapRejectedNotificationTitle,
                $"Your swap request #{swapId} was rejected.");
            message = "Swap rejected.";
            return true;
        }

        public List<ShiftSwapRequest> GetAllShiftSwapRequests()
        {
            return this.shiftSwapRepository.GetAllShiftSwapRequests().ToList();
        }
    }
}
