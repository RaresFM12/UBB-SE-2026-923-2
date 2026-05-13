namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;

    public class HangoutService : IHangoutService
    {
        private const int MinimumHangoutTitleLength = 5;
        private const int MaximumHangoutTitleLength = 25;
        private const int MaximumHangoutDescriptionLength = 100;
        private const int MinDaysAheadForHangout = 7;
        private const string FinishedAppointmentStatus = "Finished";
        private const string CanceledAppointmentStatusUs = "Canceled";
        private const string CancelledAppointmentStatusUk = "Cancelled";

        private readonly IHangoutRepository hangoutRepository;
        private readonly IHangoutParticipantRepository hangoutParticipantRepository;
        private readonly IAppointmentRepository appointmentRepository;
        private readonly IStaffRepository staffRepository;
        private readonly IEvaluationsRepository? evaluationsRepository;

        public HangoutService(
            IHangoutRepository hangoutRepository,
            IHangoutParticipantRepository hangoutParticipantRepository,
            IAppointmentRepository appointmentRepository,
            IStaffRepository staffRepository,
            IEvaluationsRepository? evaluationsRepository = null)
        {
            this.hangoutRepository = hangoutRepository;
            this.hangoutParticipantRepository = hangoutParticipantRepository;
            this.appointmentRepository = appointmentRepository;
            this.staffRepository = staffRepository;
            this.evaluationsRepository = evaluationsRepository;
        }

        public int CreateHangout(string title, string description, DateTime date, int maxParticipants, IStaff creator)
        {
            if (string.IsNullOrWhiteSpace(title) || title.Length < MinimumHangoutTitleLength || title.Length > MaximumHangoutTitleLength)
            {
                throw new ArgumentException($"Title must be between {MinimumHangoutTitleLength} and {MaximumHangoutTitleLength} characters.");
            }

            if (description != null && description.Length > MaximumHangoutDescriptionLength)
            {
                throw new ArgumentException($"Description must be at most {MaximumHangoutDescriptionLength} characters.");
            }

            if (date.Date < DateTime.Now.Date.AddDays(MinDaysAheadForHangout))
            {
                throw new ArgumentException("The hangout date must be at least 1 week away from today.");
            }

            if (this.HasConflictingAppointmentOnDate(creator.StaffID, date))
            {
                throw new InvalidOperationException("You cannot create a hangout on a day where you have active scheduled appointments.");
            }

            if (this.HasMedicalEvaluationOnDate(creator.StaffID, date))
            {
                throw new InvalidOperationException("You cannot create a hangout on a day where you have a medical evaluation logged.");
            }

            int newHangoutId = this.hangoutRepository.AddHangout(title, description ?? string.Empty, date, maxParticipants);
            this.hangoutParticipantRepository.AddParticipant(newHangoutId, creator.StaffID);
            return newHangoutId;
        }

        public void JoinHangout(int hangoutId, IStaff staff)
        {
            var hangout = this.hangoutRepository.GetHangoutById(hangoutId);
            if (hangout == null)
            {
                throw new ArgumentException("Hangout not found.");
            }

            bool IsForCurrentHangout((int HangoutId, int StaffId) participant) => participant.HangoutId == hangoutId;
            bool IsCurrentStaffMember((int HangoutId, int StaffId) participant) => participant.StaffId == staff.StaffID;

            var participantsForHangout = this.hangoutParticipantRepository.GetAllParticipants()
                .Where(IsForCurrentHangout)
                .ToList();

            if (participantsForHangout.Count >= hangout.MaxParticipants)
            {
                throw new InvalidOperationException("This hangout is already full.");
            }

            if (participantsForHangout.Any(IsCurrentStaffMember))
            {
                throw new InvalidOperationException("You have already joined this hangout.");
            }

            if (this.HasConflictingAppointmentOnDate(staff.StaffID, hangout.Date))
            {
                throw new InvalidOperationException("You cannot join a hangout on a day where you have active scheduled appointments.");
            }

            if (this.HasMedicalEvaluationOnDate(staff.StaffID, hangout.Date))
            {
                throw new InvalidOperationException("You cannot join a hangout on a day where you have a medical evaluation logged.");
            }

            this.hangoutParticipantRepository.AddParticipant(hangoutId, staff.StaffID);
        }

        public List<Hangout> GetAllHangouts()
        {
            int ByStaffId(IStaff staffMember) => staffMember.StaffID;

            var hangouts = this.hangoutRepository.GetAllHangouts();
            var allParticipants = this.hangoutParticipantRepository.GetAllParticipants();
            var allStaffById = this.staffRepository.LoadAllStaff().ToDictionary(ByStaffId);

            foreach (var hangout in hangouts)
            {
                bool IsForThisHangout((int HangoutId, int StaffId) participant) => participant.HangoutId == hangout.HangoutID;
                int ToStaffId((int HangoutId, int StaffId) participant) => participant.StaffId;

                var staffIdsForHangout = allParticipants
                    .Where(IsForThisHangout)
                    .Select(ToStaffId);
                foreach (var staffId in staffIdsForHangout)
                {
                    if (allStaffById.TryGetValue(staffId, out var staffMember))
                    {
                        hangout.ParticipantList.Add(staffMember);
                    }
                }
            }

            return hangouts;
        }

        private bool HasConflictingAppointmentOnDate(int staffId, DateTime date)
        {
            System.Threading.Tasks.Task<IReadOnlyList<Appointment>> LoadAllAppointments() => this.appointmentRepository.GetAllAppointmentsAsync();
            var allAppointments = System.Threading.Tasks.Task.Run(LoadAllAppointments).GetAwaiter().GetResult();

            bool IsActiveStatus(string status) =>
                !string.Equals(status, FinishedAppointmentStatus, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(status, CanceledAppointmentStatusUs, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(status, CancelledAppointmentStatusUk, StringComparison.OrdinalIgnoreCase);

            bool IsConflictingForStaff(Appointment appointment) =>
                appointment.Doctor?.StaffID == staffId
                && appointment.Date.Date == date.Date
                && IsActiveStatus(appointment.Status);

            return allAppointments.Any(IsConflictingForStaff);
        }

        private bool HasMedicalEvaluationOnDate(int staffId, DateTime date)
        {
            if (this.evaluationsRepository == null)
            {
                return false;
            }

            bool IsForStaffOnDate(MedicalEvaluation evaluation) =>
                evaluation.Evaluator != null
                && evaluation.Evaluator.StaffID == staffId
                && evaluation.EvaluationDate.Date == date.Date;

            return this.evaluationsRepository.GetAllEvaluations().Any(IsForStaffOnDate);
        }
    }
}
