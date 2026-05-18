namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;

    public sealed class MedicalEvaluationService : IMedicalEvaluationService
    {
        private const double FatigueThresholdHours = 12.0;
        private const double FatigueLookbackHours = 24.0;
        private const int DefaultPatientId = 0;
        private const int NotFoundIndex = -1;
        private const string ConfirmedAppointmentStatus = "Confirmed";
        private const string AllergyKeyword = "Allergy";
        private const string AdverseKeyword = "Adverse";
        private const string RiskMarker = "[RISK]";
        private const int AdminBroadcastRecipientId = 0;
        private const string FatigueAlertTitle = "Fatigue Intervention Required";
        private static readonly char[] MedicationSeparators = { ',', ';', '\n', '\r', '/', '|' };

        private readonly IEvaluationsRepository evaluationsRepository;
        private readonly IHighRiskMedicineRepository highRiskMedicineRepository;
        private readonly IAppointmentRepository appointmentRepository;
        private readonly IStaffRepository staffRepository;
        private readonly IShiftRepository shiftRepository;
        private readonly ICurrentUserService currentUserService;
        private readonly INotificationRepository? notificationRepository;

        public MedicalEvaluationService(
            IEvaluationsRepository evaluationsRepository,
            IHighRiskMedicineRepository highRiskMedicineRepository,
            IAppointmentRepository appointmentRepository,
            IStaffRepository staffRepository,
            IShiftRepository shiftRepository,
            ICurrentUserService currentUserService,
            INotificationRepository? notificationRepository = null)
        {
            this.evaluationsRepository = evaluationsRepository;
            this.highRiskMedicineRepository = highRiskMedicineRepository;
            this.appointmentRepository = appointmentRepository;
            this.staffRepository = staffRepository;
            this.shiftRepository = shiftRepository;
            this.currentUserService = currentUserService;
            this.notificationRepository = notificationRepository;
        }

        public List<Doctor> GetAllDoctors() =>
            this.staffRepository.LoadAllStaff().OfType<Doctor>().ToList();

        public List<Appointment> GetAppointmentsByDoctor(int doctorId)
        {
            Task<IReadOnlyList<Appointment>> LoadAllAppointments() => this.appointmentRepository.GetAllAppointmentsAsync();
            var allAppointments = Task.Run(LoadAllAppointments).GetAwaiter().GetResult();

            bool IsConfirmedForDoctor(Appointment appointment) =>
                appointment.Doctor?.StaffID == doctorId
                && string.Equals(appointment.Status, ConfirmedAppointmentStatus, StringComparison.OrdinalIgnoreCase);
            DateTime ByDate(Appointment appointment) => appointment.Date;
            TimeSpan ByStartTime(Appointment appointment) => appointment.StartTime;

            return allAppointments
                .Where(IsConfirmedForDoctor)
                .OrderBy(ByDate)
                .ThenBy(ByStartTime)
                .ToList();
        }

        public List<MedicalEvaluation> GetEvaluationsByDoctor(string doctorId)
        {
            if (!int.TryParse(doctorId, out var parsedDoctorId))
            {
                return new List<MedicalEvaluation>();
            }

            bool IsForDoctor(MedicalEvaluation evaluation) =>
                evaluation.Evaluator != null && evaluation.Evaluator.StaffID == parsedDoctorId;
            int ByEvaluationId(MedicalEvaluation evaluation) => evaluation.EvaluationID;

            return this.evaluationsRepository.GetAllEvaluations()
                .Where(IsForDoctor)
                .OrderByDescending(ByEvaluationId)
                .ToList();
        }

        public void SaveEvaluation(MedicalEvaluation record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            int patientId = int.TryParse(record.PatientId, out var parsedPatientId) ? parsedPatientId : DefaultPatientId;
            bool assumedRisk = ContainsRiskMarker(record.Symptoms);
            int doctorId = record.Evaluator?.StaffID ?? this.currentUserService.UserId;

            this.evaluationsRepository.AddEvaluation(
                doctorId,
                patientId,
                record.Symptoms ?? string.Empty,
                record.Notes ?? string.Empty,
                record.MedicationsList ?? string.Empty,
                assumedRisk);
        }

        public void UpdateEvaluation(MedicalEvaluation record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            if (record.EvaluationID <= 0)
            {
                throw new ArgumentException("EvaluationID is required to update.", nameof(record));
            }

            this.evaluationsRepository.UpdateEvaluation(
                record.EvaluationID,
                record.Symptoms ?? string.Empty,
                record.Notes ?? string.Empty,
                record.MedicationsList ?? string.Empty);
        }

        public void DeleteEvaluation(int evaluationId) =>
            this.evaluationsRepository.DeleteEvaluation(evaluationId);

        public void RaiseFatigueIntervention(int doctorId, string doctorName)
        {
            if (this.notificationRepository == null)
            {
                return;
            }

            string message = string.IsNullOrWhiteSpace(doctorName)
                ? $"Doctor #{doctorId} exceeded the {FatigueThresholdHours:F0}h duty limit. Reassign active cases."
                : $"{doctorName} (Doctor #{doctorId}) exceeded the {FatigueThresholdHours:F0}h duty limit. Reassign active cases.";

            this.notificationRepository.AddNotification(AdminBroadcastRecipientId, FatigueAlertTitle, message);
        }

        public bool IsDoctorFatigued(string doctorId)
        {
            if (!int.TryParse(doctorId, out var parsedDoctorId))
            {
                return false;
            }

            DateTime lookbackStart = DateTime.Now.AddHours(-FatigueLookbackHours);

            bool IsRecentShiftForDoctor(Shift shift) =>
                shift.AppointedStaff.StaffID == parsedDoctorId && shift.EndTime >= lookbackStart;
            double ToShiftHours(Shift shift) => (shift.EndTime - shift.StartTime).TotalHours;

            double recentHours = this.shiftRepository.GetAllShifts()
                .Where(IsRecentShiftForDoctor)
                .Sum(ToShiftHours);

            return recentHours >= FatigueThresholdHours;
        }

        public string? CheckMedicineConflict(string patientId, string medications)
        {
            if (string.IsNullOrWhiteSpace(medications) || string.IsNullOrWhiteSpace(patientId))
            {
                return null;
            }

            string trimmedMedicineName = medications.Trim();
            bool MatchesMedicineName((string MedicineName, string WarningMessage) medicine) =>
                string.Equals(medicine.MedicineName, trimmedMedicineName, StringComparison.OrdinalIgnoreCase);

            var matchingMedicine = this.highRiskMedicineRepository.GetAllHighRiskMedicines()
                .FirstOrDefault(MatchesMedicineName);
            if (!string.IsNullOrEmpty(matchingMedicine.WarningMessage))
            {
                return matchingMedicine.WarningMessage;
            }

            return this.CheckPatientHistoryForRisk(patientId, medications);
        }

        private string? CheckPatientHistoryForRisk(string patientId, string currentMedicines)
        {
            var currentDrugs = SplitMedicines(currentMedicines);
            if (currentDrugs.Count == 0)
            {
                return null;
            }

            bool MatchesPatient(MedicalEvaluation evaluation) =>
                string.Equals(evaluation.PatientId, patientId, StringComparison.OrdinalIgnoreCase);
            bool MentionsAllergyOrAdverse(MedicalEvaluation evaluation) =>
                ContainsKeyword(evaluation.Symptoms, AllergyKeyword)
                || ContainsKeyword(evaluation.Symptoms, AdverseKeyword)
                || ContainsKeyword(evaluation.Notes, AllergyKeyword)
                || ContainsKeyword(evaluation.Notes, AdverseKeyword);

            string? FirstSharedDrug(MedicalEvaluation evaluation)
            {
                var historicalDrugs = SplitMedicines(evaluation.MedicationsList);
                bool MatchesAnyHistoricalDrug(string currentDrug) =>
                    historicalDrugs.Any(historical => string.Equals(historical, currentDrug, StringComparison.OrdinalIgnoreCase));
                return currentDrugs.FirstOrDefault(MatchesAnyHistoricalDrug);
            }

            foreach (var evaluation in this.evaluationsRepository.GetAllEvaluations())
            {
                if (!MatchesPatient(evaluation) || !MentionsAllergyOrAdverse(evaluation))
                {
                    continue;
                }

                var sharedDrug = FirstSharedDrug(evaluation);
                if (sharedDrug != null)
                {
                    return $"HISTORY ALERT: Patient had a past Adverse Reaction/Allergy to {sharedDrug} recorded in their history.";
                }
            }

            return null;
        }

        private static List<string> SplitMedicines(string? medications)
        {
            if (string.IsNullOrWhiteSpace(medications))
            {
                return new List<string>();
            }

            return medications
                .Split(MedicationSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Select(token => token.Trim())
                .Where(token => token.Length > 0)
                .ToList();
        }

        private static bool ContainsKeyword(string? text, string keyword) =>
            !string.IsNullOrEmpty(text) && text.Contains(keyword, StringComparison.OrdinalIgnoreCase);

        private static bool ContainsRiskMarker(string? symptoms) =>
            (symptoms ?? string.Empty).IndexOf(RiskMarker, StringComparison.OrdinalIgnoreCase) > NotFoundIndex;

        public MedicalEvaluation? GetEvaluationById(int evaluationId)
        {
            bool IsMatchingEvaluation(MedicalEvaluation evaluation) => evaluation.EvaluationID == evaluationId;
            return this.evaluationsRepository.GetAllEvaluations().FirstOrDefault(IsMatchingEvaluation);
        }
    }
}
