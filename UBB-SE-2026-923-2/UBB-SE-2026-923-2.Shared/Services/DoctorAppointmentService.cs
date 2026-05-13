namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;

    public sealed class DoctorAppointmentService : IDoctorAppointmentService
    {
        private const int UpcomingAppointmentsWindowDays = 31;
        private const int DefaultAppointmentDurationMinutes = 30;
        private const int DefaultPatientId = 0;
        private const int NoActiveAppointmentsCount = 0;
        private const string ScheduledStatus = "Scheduled";
        private const string FinishedStatus = "Finished";
        private const string CanceledStatus = "Canceled";
        private const string AvailableStatus = "AVAILABLE";
        private const string PatientNamePrefix = "PAT-";
        private const string DefaultPatientIdString = "0";

        private readonly IAppointmentRepository dataSource;
        private readonly IStaffRepository staffRepository;
        private readonly IShiftRepository? shiftRepository;

        public DoctorAppointmentService(IAppointmentRepository dataSource, IStaffRepository staffRepository, IShiftRepository? shiftRepository = null)
        {
            this.dataSource = dataSource;
            this.staffRepository = staffRepository;
            this.shiftRepository = shiftRepository;
        }

        public async Task<IReadOnlyList<Appointment>> GetUpcomingAppointmentsAsync(int doctorUserId, DateTime fromDate, int skipCount, int takeCount)
        {
            DateTime from = fromDate.Date;
            DateTime to = from.AddDays(UpcomingAppointmentsWindowDays);
            var allAppointments = await this.dataSource.GetAllAppointmentsAsync();

            bool IsForDoctor(Appointment appointment) => appointment.Doctor?.StaffID == doctorUserId;
            bool IsWithinWindow(Appointment appointment)
            {
                DateTime appointmentStart = appointment.Date.Add(appointment.StartTime);
                return appointmentStart >= from && appointmentStart < to;
            }

            DateTime ByDate(Appointment appointment) => appointment.Date;
            TimeSpan ByStartTime(Appointment appointment) => appointment.StartTime;

            return allAppointments
                .Where(IsForDoctor)
                .Where(IsWithinWindow)
                .OrderBy(ByDate)
                .ThenBy(ByStartTime)
                .Skip(skipCount)
                .Take(takeCount)
                .Select(ToDomainAppointment)
                .ToList();
        }

        public async Task<IReadOnlyList<(int DoctorId, string DoctorName)>> GetAllDoctorsAsync()
        {
            var doctors = await this.staffRepository.GetAllDoctorsAsync();

            (int DoctorId, string DoctorName) ToDoctorOption((int DoctorId, string FirstName, string LastName) doctor) =>
                (doctor.DoctorId, ((doctor.FirstName ?? string.Empty) + " " + (doctor.LastName ?? string.Empty)).Trim());

            string ByDoctorName((int DoctorId, string DoctorName) doctor) => doctor.DoctorName;

            return doctors
                .Select(ToDoctorOption)
                .OrderBy(ByDoctorName)
                .ToList();
        }

        public async Task<Appointment?> GetAppointmentDetailsAsync(int appointmentId)
        {
            var allAppointments = await this.dataSource.GetAllAppointmentsAsync();
            bool HasMatchingId(Appointment existingAppointment) => existingAppointment.Id == appointmentId;

            var appointment = allAppointments.FirstOrDefault(HasMatchingId);
            return appointment == null ? null : ToDomainAppointment(appointment);
        }

        public async Task<IReadOnlyList<Appointment>> GetAppointmentsForAdminAsync(int doctorId)
        {
            var allAppointments = await this.dataSource.GetAllAppointmentsAsync();

            bool IsForDoctor(Appointment appointment) => appointment.Doctor?.StaffID == doctorId;
            DateTime ByDate(Appointment appointment) => appointment.Date;
            TimeSpan ByStartTime(Appointment appointment) => appointment.StartTime;

            return allAppointments
                .Where(IsForDoctor)
                .OrderBy(ByDate)
                .ThenBy(ByStartTime)
                .Select(ToDomainAppointment)
                .ToList();
        }

        public async Task CreateAppointmentAsync(string patientName, int doctorId, DateTime date, TimeSpan startTime)
        {
            await this.EnsureDoctorIsBookableAsync(doctorId);

            var appointment = new Appointment
            {
                PatientName = patientName,
                ExternalRefId = ExtractExternalRefId(patientName),
                Doctor = new Doctor { StaffID = doctorId },
                Date = date.Date,
                StartTime = startTime,
                EndTime = startTime.Add(TimeSpan.FromMinutes(DefaultAppointmentDurationMinutes)),
                Status = ScheduledStatus,
            };
            await this.PersistAppointmentAsync(appointment);
        }

        public async Task BookAppointmentAsync(Appointment appointment)
        {
            await this.EnsureDoctorIsBookableAsync(appointment.Doctor.StaffID);
            if (string.IsNullOrWhiteSpace(appointment.ExternalRefId))
            {
                appointment.ExternalRefId = ExtractExternalRefId(appointment.PatientName);
            }

            await this.PersistAppointmentAsync(appointment);
        }

        public async Task FinishAppointmentAsync(Appointment appointment)
        {
            if (string.Equals(appointment?.Status, FinishedStatus, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("This appointment is already finished.");
            }

            await this.dataSource.UpdateAppointmentStatusAsync(appointment!.Id, FinishedStatus);
            appointment.Status = FinishedStatus;

            var allAppointments = await this.dataSource.GetAllAppointmentsAsync();

            DateTime finishedStart = appointment.Date.Date.Add(appointment.StartTime);
            DateTime finishedEnd = appointment.Date.Date.Add(appointment.EndTime);

            bool OverlapsFinishedWindow(Appointment existingAppointment)
            {
                if (existingAppointment.Id == appointment.Id)
                {
                    return false;
                }

                if (existingAppointment.Doctor?.StaffID != appointment.Doctor?.StaffID)
                {
                    return false;
                }

                if (!string.Equals(existingAppointment.Status, ScheduledStatus, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                DateTime otherStart = existingAppointment.Date.Date.Add(existingAppointment.StartTime);
                DateTime otherEnd = existingAppointment.Date.Date.Add(existingAppointment.EndTime);
                return otherStart < finishedEnd && otherEnd > finishedStart;
            }

            int concurrentAppointments = allAppointments.Count(OverlapsFinishedWindow);

            if (concurrentAppointments == NoActiveAppointmentsCount)
            {
                await this.staffRepository.UpdateStatusAsync(appointment.Doctor?.StaffID ?? 0, AvailableStatus);
            }
        }

        private async Task EnsureDoctorIsBookableAsync(int doctorId)
        {
            var doctor = this.staffRepository.GetStaffById(doctorId) as Doctor;
            if (doctor == null)
            {
                return;
            }

            if (doctor.DoctorStatus == DoctorStatus.OFF_DUTY)
            {
                throw new InvalidOperationException(
                    $"Doctor #{doctorId} is OFF_DUTY and cannot accept new appointments.");
            }

            await Task.CompletedTask;
        }

        private static string ExtractExternalRefId(string? patientName)
        {
            if (string.IsNullOrWhiteSpace(patientName))
            {
                return string.Empty;
            }

            return patientName.StartsWith(PatientNamePrefix, StringComparison.OrdinalIgnoreCase)
                ? patientName.Substring(PatientNamePrefix.Length).Trim()
                : patientName.Trim();
        }

        public async Task<IReadOnlyList<Appointment>> GetAppointmentsInRangeAsync(int doctorId, DateTime fromDate, DateTime toDate)
        {
            var rawAppointments = await this.dataSource.GetAllAppointmentsAsync();

            bool IsForDoctor(Appointment appointment) => appointment.Doctor?.StaffID == doctorId;
            bool IsInRange(Appointment appointment)
            {
                var start = appointment.Date.Date + appointment.StartTime;
                var end = appointment.Date.Date + appointment.EndTime;
                if (end <= start)
                {
                    return false;
                }

                return start < toDate && end > fromDate;
            }

            DateTime ByDate(Appointment appointment) => appointment.Date;
            TimeSpan ByStartTime(Appointment appointment) => appointment.StartTime;

            return rawAppointments
                .Select(ToDomainAppointment)
                .Where(IsForDoctor)
                .Where(IsInRange)
                .OrderBy(ByDate)
                .ThenBy(ByStartTime)
                .ToList();
        }

        public async Task CancelAppointmentAsync(Appointment appointment)
        {
            if (string.Equals(appointment?.Status, FinishedStatus, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot cancel an appointment that is already Finished.");
            }

            await this.dataSource.UpdateAppointmentStatusAsync(appointment!.Id, CanceledStatus);
            appointment.Status = CanceledStatus;
        }

        public Task<IReadOnlyList<Shift>> GetShiftsForStaffInRangeAsync(int doctorId, DateTime fromDate, DateTime toDate)
        {
            if (this.shiftRepository == null)
            {
                return Task.FromResult<IReadOnlyList<Shift>>(new List<Shift>());
            }

            bool IsForDoctorInRange(Shift shift) =>
                shift.AppointedStaff.StaffID == doctorId
                && shift.StartTime < toDate
                && shift.EndTime > fromDate
                && shift.Status != ShiftStatus.CANCELLED;

            DateTime ByStartTime(Shift shift) => shift.StartTime;

            IReadOnlyList<Shift> LoadAndFilter() => this.shiftRepository
                .GetAllShifts()
                .Where(IsForDoctorInRange)
                .OrderBy(ByStartTime)
                .ToList();

            return Task.Run(LoadAndFilter);
        }

        private async Task PersistAppointmentAsync(Appointment appointment)
        {
            int patientId = ParsePatientId(appointment.PatientName);
            DateTime start = appointment.Date.Date.Add(appointment.StartTime);
            DateTime end = appointment.Date.Date.Add(appointment.EndTime);
            string status = string.IsNullOrWhiteSpace(appointment.Status) ? ScheduledStatus : appointment.Status;

            await this.dataSource.AddAppointmentAsync(patientId, appointment.Doctor.StaffID, start, end, status);
        }

        private static int ParsePatientId(string? patientName)
        {
            string rawPatientInput = patientName?.Replace(PatientNamePrefix, string.Empty).Trim() ?? DefaultPatientIdString;
            return int.TryParse(rawPatientInput, out int patientId) ? patientId : DefaultPatientId;
        }

        private static Appointment ToDomainAppointment(Appointment appointment)
        {
            const string DefaultPatientName = PatientNamePrefix + DefaultPatientIdString;

            string patientName;
            if (string.IsNullOrWhiteSpace(appointment.PatientName))
            {
                patientName = DefaultPatientName;
            }
            else if (appointment.PatientName.StartsWith(PatientNamePrefix, StringComparison.OrdinalIgnoreCase))
            {
                patientName = appointment.PatientName;
            }
            else if (int.TryParse(appointment.PatientName, out var patientId))
            {
                patientName = PatientNamePrefix + patientId;
            }
            else
            {
                patientName = appointment.PatientName;
            }

            string status = string.IsNullOrWhiteSpace(appointment.Status)
                ? ScheduledStatus
                : appointment.Status;

            return new Appointment
            {
                Id = appointment.Id,
                Doctor = appointment.Doctor,
                PatientName = patientName,
                ExternalRefId = ExtractExternalRefId(patientName),
                Date = appointment.Date,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = status,
                Type = appointment.Type,
                Location = appointment.Location,
                Notes = appointment.Notes,
            };
        }
    }
}
