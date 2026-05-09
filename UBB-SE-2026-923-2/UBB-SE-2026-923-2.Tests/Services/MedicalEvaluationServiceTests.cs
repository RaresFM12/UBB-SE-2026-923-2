namespace UBB_SE_2026_923_2.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using Moq;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class MedicalEvaluationServiceLogicTests
    {
        private Mock<IEvaluationsRepository> mockEvaluationsRepository;
        private Mock<IHighRiskMedicineRepository> mockHighRiskMedicineRepository;
        private Mock<IAppointmentRepository> mockAppointmentRepository;
        private Mock<IStaffRepository> mockStaffRepository;
        private Mock<IShiftRepository> mockShiftRepository;
        private Mock<ICurrentUserService> mockCurrentUserService;
        private Mock<INotificationRepository> mockNotificationRepository;
        private MedicalEvaluationService medicalEvaluationService;

        [SetUp]
        public void Setup()
        {
            this.mockEvaluationsRepository = new Mock<IEvaluationsRepository>();
            this.mockHighRiskMedicineRepository = new Mock<IHighRiskMedicineRepository>();
            this.mockAppointmentRepository = new Mock<IAppointmentRepository>();
            this.mockStaffRepository = new Mock<IStaffRepository>();
            this.mockShiftRepository = new Mock<IShiftRepository>();
            this.mockCurrentUserService = new Mock<ICurrentUserService>();
            this.mockNotificationRepository = new Mock<INotificationRepository>();

            this.medicalEvaluationService = new MedicalEvaluationService(
                this.mockEvaluationsRepository.Object,
                this.mockHighRiskMedicineRepository.Object,
                this.mockAppointmentRepository.Object,
                this.mockStaffRepository.Object,
                this.mockShiftRepository.Object,
                this.mockCurrentUserService.Object,
                this.mockNotificationRepository.Object);
        }

        [Test]
        public void GetAppointmentsByDoctor_WhenAppointmentsHaveDifferentDoctors_ReturnsOnlyConfirmedAppointmentsForRequestedDoctor()
        {
            var requestedDoctorIdentifier = 3;

            this.mockAppointmentRepository
                .Setup(appointmentRepository => appointmentRepository.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>
                {
                    CreateAppointment(1, requestedDoctorIdentifier, DateTime.Today.AddDays(1), 9, "Confirmed"),
                    CreateAppointment(2, 9, DateTime.Today.AddDays(1), 10, "Confirmed"),
                    CreateAppointment(3, requestedDoctorIdentifier, DateTime.Today.AddDays(1), 11, "Scheduled"),
                });

            var appointmentsForRequestedDoctor = this.medicalEvaluationService.GetAppointmentsByDoctor(requestedDoctorIdentifier);

            Assert.That(appointmentsForRequestedDoctor.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetEvaluationsByDoctor_WhenDoctorIdentifierIsInvalid_ReturnsEmptyList()
        {
            var evaluationsForDoctor = this.medicalEvaluationService.GetEvaluationsByDoctor("invalid-doctor-id");

            Assert.That(evaluationsForDoctor.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetEvaluationsByDoctor_WhenEvaluationsExistForRequestedDoctor_ReturnsEvaluationsOrderedDescendingByIdentifier()
        {
            var requestedDoctorIdentifier = 5;

            this.mockEvaluationsRepository
                .Setup(evaluationsRepository => evaluationsRepository.GetAllEvaluations())
                .Returns(new List<MedicalEvaluation>
                {
                    CreateMedicalEvaluation(1, requestedDoctorIdentifier, "10", "Headache", "Notes", "Paracetamol"),
                    CreateMedicalEvaluation(3, requestedDoctorIdentifier, "10", "Cold", "Notes", "Ibuprofen"),
                    CreateMedicalEvaluation(2, 9, "10", "Fever", "Notes", "Aspirin"),
                });

            var evaluationsForRequestedDoctor = this.medicalEvaluationService.GetEvaluationsByDoctor(requestedDoctorIdentifier.ToString());

            Assert.That(evaluationsForRequestedDoctor[0].EvaluationID, Is.EqualTo(3));
        }

        [Test]
        public void SaveEvaluation_WhenSymptomsContainRiskMarker_SavesEvaluationWithAssumedRiskTrue()
        {
            var evaluationWithRiskMarker = CreateMedicalEvaluation(0, 7, "12", "[RISK] severe symptoms", "Notes", "Medicine");

            this.medicalEvaluationService.SaveEvaluation(evaluationWithRiskMarker);

            this.mockEvaluationsRepository.Verify(
                evaluationsRepository => evaluationsRepository.AddEvaluation(
                    7,
                    12,
                    "[RISK] severe symptoms",
                    "Notes",
                    "Medicine",
                    true),
                Times.Once);
        }

        [Test]
        public void SaveEvaluation_WhenEvaluatorIsMissing_UsesCurrentUserIdentifierAsDoctorIdentifier()
        {
            this.mockCurrentUserService
                .Setup(currentUserService => currentUserService.UserId)
                .Returns(44);

            var evaluationWithoutEvaluator = new MedicalEvaluation
            {
                PatientId = "12",
                Symptoms = "Symptoms",
                Notes = "Notes",
                MedicationsList = "Medicine",
                Evaluator = null,
            };

            this.medicalEvaluationService.SaveEvaluation(evaluationWithoutEvaluator);

            this.mockEvaluationsRepository.Verify(
                evaluationsRepository => evaluationsRepository.AddEvaluation(
                    44,
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()),
                Times.Once);
        }

        [Test]
        public void UpdateEvaluation_WhenEvaluationIdentifierIsInvalid_ThrowsArgumentException()
        {
            var evaluationWithoutIdentifier = CreateMedicalEvaluation(0, 7, "12", "Symptoms", "Notes", "Medicine");

            Assert.Throws<ArgumentException>(() => this.medicalEvaluationService.UpdateEvaluation(evaluationWithoutIdentifier));
        }

        [Test]
        public void RaiseFatigueIntervention_WhenNotificationRepositoryExists_AddsNotificationForAdministrator()
        {
            this.medicalEvaluationService.RaiseFatigueIntervention(8, "Doctor Smith");

            this.mockNotificationRepository.Verify(
                notificationRepository => notificationRepository.AddNotification(
                    0,
                    "Fatigue Intervention Required",
                    It.Is<string>(notificationMessage => notificationMessage.Contains("Doctor Smith"))),
                Times.Once);
        }

        [Test]
        public void IsDoctorFatigued_WhenDoctorHasAtLeastTwelveRecentShiftHours_ReturnsTrue()
        {
            var requestedDoctorIdentifier = 8;
            var requestedDoctor = new Doctor { StaffID = requestedDoctorIdentifier };

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift>
                {
                    new Shift(
                        1,
                        requestedDoctor,
                        "Ward A",
                        DateTime.Now.AddHours(-13),
                        DateTime.Now.AddHours(-1),
                        ShiftStatus.ACTIVE),
                });

            var isDoctorFatigued = this.medicalEvaluationService.IsDoctorFatigued(requestedDoctorIdentifier.ToString());

            Assert.That(isDoctorFatigued, Is.True);
        }

        [Test]
        public void CheckMedicineConflict_WhenMedicineIsHighRisk_ReturnsHighRiskWarning()
        {
            this.mockHighRiskMedicineRepository
                .Setup(highRiskMedicineRepository => highRiskMedicineRepository.GetAllHighRiskMedicines())
                .Returns(new List<(string MedicineName, string WarningMessage)>
                {
                    ("Aspirin", "High risk warning"),
                });

            var warningMessage = this.medicalEvaluationService.CheckMedicineConflict("12", "aspirin");

            Assert.That(warningMessage, Is.EqualTo("High risk warning"));
        }

        [Test]
        public void CheckMedicineConflict_WhenPatientHadHistoricalAllergyToSameMedicine_ReturnsHistoryAlert()
        {
            this.mockHighRiskMedicineRepository
                .Setup(highRiskMedicineRepository => highRiskMedicineRepository.GetAllHighRiskMedicines())
                .Returns(new List<(string MedicineName, string WarningMessage)>());

            this.mockEvaluationsRepository
                .Setup(evaluationsRepository => evaluationsRepository.GetAllEvaluations())
                .Returns(new List<MedicalEvaluation>
                {
                    CreateMedicalEvaluation(1, 7, "12", "Allergy", "Notes", "Aspirin"),
                });

            var warningMessage = this.medicalEvaluationService.CheckMedicineConflict("12", "aspirin");

            Assert.That(warningMessage, Does.Contain("HISTORY ALERT"));
        }

        private static Appointment CreateAppointment(
            int appointmentIdentifier,
            int doctorIdentifier,
            DateTime appointmentDate,
            int startHour,
            string appointmentStatus)
        {
            return new Appointment
            {
                Id = appointmentIdentifier,
                Doctor = new Doctor { StaffID = doctorIdentifier },
                Date = appointmentDate.Date,
                StartTime = TimeSpan.FromHours(startHour),
                EndTime = TimeSpan.FromHours(startHour + 1),
                Status = appointmentStatus,
            };
        }

        private static MedicalEvaluation CreateMedicalEvaluation(
            int evaluationIdentifier,
            int doctorIdentifier,
            string patientIdentifier,
            string symptoms,
            string notes,
            string medicationsList)
        {
            return new MedicalEvaluation
            {
                EvaluationID = evaluationIdentifier,
                Evaluator = new Doctor { StaffID = doctorIdentifier },
                PatientId = patientIdentifier,
                Symptoms = symptoms,
                Notes = notes,
                MedicationsList = medicationsList,
            };
        }
    }
}