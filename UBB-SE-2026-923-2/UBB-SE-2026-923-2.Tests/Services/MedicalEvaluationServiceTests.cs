namespace UBB_SE_2026_923_2.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Moq;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class MedicalEvaluationServiceTests
    {
        private Mock<IEvaluationsRepository> mockEvaluationsRepository;
        private Mock<IHighRiskMedicineRepository> mockHighRiskMedicineRepository;
        private Mock<IAppointmentRepository> mockAppointmentRepository;
        private Mock<IStaffRepository> mockStaffRepository;
        private Mock<IShiftRepository> mockShiftRepository;
        private Mock<ICurrentUserService> mockCurrentUserService;
        private Mock<INotificationRepository> mockNotificationRepository;
        private MedicalEvaluationService service;
        private Doctor doctor1;

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

            this.service = new MedicalEvaluationService(
                this.mockEvaluationsRepository.Object,
                this.mockHighRiskMedicineRepository.Object,
                this.mockAppointmentRepository.Object,
                this.mockStaffRepository.Object,
                this.mockShiftRepository.Object,
                this.mockCurrentUserService.Object,
                this.mockNotificationRepository.Object);

            this.doctor1 = new Doctor(1, "John", "Doe", "c", true, "Gen", "L1", DoctorStatus.AVAILABLE, 5);
            this.mockHighRiskMedicineRepository.Setup(repository => repository.GetAllHighRiskMedicines()).Returns(new List<(string, string)>());
            this.mockEvaluationsRepository.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation>());
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
        }

        // --- GetAllDoctors & GetAppointments ---
        [Test]
        public void GetAllDoctors_ReturnsDoctorsOnly()
        {
            var pharmacist = new Pharmacyst(2, "B", "C", "c", true, "Cert", 3);
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1, pharmacist });
            var result = this.service.GetAllDoctors();
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetAppointmentsByDoctor_ReturnsConfirmedOnly()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { Doctor = new Doctor { StaffID = 1 }, Status = "Confirmed", Date = DateTime.Now, StartTime = TimeSpan.FromHours(9) },
                new Appointment { Doctor = new Doctor { StaffID = 1 }, Status = "Cancelled", Date = DateTime.Now, StartTime = TimeSpan.FromHours(10) },
                new Appointment { Doctor = new Doctor { StaffID = 2 }, Status = "Confirmed", Date = DateTime.Now, StartTime = TimeSpan.FromHours(11) },
            };
            this.mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(appointments);

            var result = this.service.GetAppointmentsByDoctor(1);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        // --- GetEvaluations ---
        [Test]
        public void GetEvaluationsByDoctor_ValidId_ReturnsFiltered()
        {
            var eval1 = new MedicalEvaluation { EvaluationID = 1, Evaluator = this.doctor1 };
            var eval2 = new MedicalEvaluation { EvaluationID = 2, Evaluator = new Doctor { StaffID = 2 } };
            this.mockEvaluationsRepository.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation> { eval1, eval2 });

            var result = this.service.GetEvaluationsByDoctor("1");
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetEvaluationsByDoctor_InvalidId_ReturnsEmpty()
        {
            var result = this.service.GetEvaluationsByDoctor("abc");
            Assert.That(result.Count, Is.EqualTo(0));
        }

        // --- Save & Update Evaluation ---
        [Test]
        public void SaveEvaluation_ValidRecord_CallsRepo()
        {
            this.mockCurrentUserService.Setup(service => service.UserId).Returns(1);
            var evaluation = new MedicalEvaluation
            {
                PatientId = "5",
                Symptoms = "Fever",
                Notes = "Note",
                MedicationsList = "Aspirin",
                Evaluator = this.doctor1,
            };

            this.service.SaveEvaluation(evaluation);
            this.mockEvaluationsRepository.Verify(repository => repository.AddEvaluation(1, 5, "Fever", "Note", "Aspirin", false), Times.Once);
        }

        [Test]
        public void SaveEvaluation_RiskMarker_SetsAssumedRisk()
        {
            this.mockCurrentUserService.Setup(service => service.UserId).Returns(1);
            var evaluation = new MedicalEvaluation
            {
                PatientId = "5",
                Symptoms = "[RISK] Severe pain",
                Notes = string.Empty,
                MedicationsList = string.Empty,
                Evaluator = this.doctor1,
            };

            this.service.SaveEvaluation(evaluation);
            this.mockEvaluationsRepository.Verify(repository => repository.AddEvaluation(1, 5, "[RISK] Severe pain", string.Empty, string.Empty, true), Times.Once);
        }

        [Test]
        public void UpdateEvaluation_ZeroId_Throws()
        {
            var evaluation = new MedicalEvaluation { EvaluationID = 0 };
            Assert.Throws<ArgumentException>(() => this.service.UpdateEvaluation(evaluation));
        }

        [Test]
        public void UpdateEvaluation_ValidRecord_CallsRepo()
        {
            var evaluation = new MedicalEvaluation { EvaluationID = 1, Symptoms = "S", Notes = "N", MedicationsList = "M" };
            this.service.UpdateEvaluation(evaluation);
            this.mockEvaluationsRepository.Verify(repository => repository.UpdateEvaluation(1, "S", "N", "M"), Times.Once);
        }

        // --- CheckMedicineConflict ---
        [Test]
        public void CheckMedicineConflict_HighRiskMedicine_ReturnsWarning()
        {
            this.mockHighRiskMedicineRepository.Setup(repository => repository.GetAllHighRiskMedicines())
                .Returns(new List<(string, string)> { ("Warfarin", "High bleeding risk") });

            var result = this.service.CheckMedicineConflict("P1", "Warfarin");
            Assert.That(result, Is.EqualTo("High bleeding risk"));
        }

        [Test]
        public void CheckMedicineConflict_HistoryAllergy_ReturnsAlert()
        {
            this.mockHighRiskMedicineRepository.Setup(repository => repository.GetAllHighRiskMedicines()).Returns(new List<(string, string)>());
            this.mockEvaluationsRepository.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation>
            {
                new MedicalEvaluation { PatientId = "P1", Symptoms = "Allergy to medication", MedicationsList = "Aspirin", Notes = string.Empty },
            });

            var result = this.service.CheckMedicineConflict("P1", "Aspirin");
            Assert.That(result, Does.Contain("HISTORY ALERT"));
        }

        [Test]
        public void CheckMedicineConflict_NoConflict_ReturnsNull()
        {
            this.mockHighRiskMedicineRepository.Setup(repository => repository.GetAllHighRiskMedicines()).Returns(new List<(string, string)>());
            this.mockEvaluationsRepository.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation>());

            var result = this.service.CheckMedicineConflict("P1", "Aspirin");
            Assert.That(result, Is.Null);
        }

        // --- NEW TESTS: Fatigue Features ---
        [Test]
        public void IsDoctorFatigued_UnderThreshold_ReturnsFalse()
        {
            var recentTime = DateTime.Now.AddHours(-5);
            var shift = new Shift(1, this.doctor1, "Ward", recentTime, recentTime.AddHours(8), ShiftStatus.COMPLETED);
            this.mockShiftRepository.Setup(repo => repo.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = this.service.IsDoctorFatigued("1");
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsDoctorFatigued_OverThreshold_ReturnsTrue()
        {
            var recentTime = DateTime.Now.AddHours(-15);
            var shift = new Shift(1, this.doctor1, "Ward", recentTime, recentTime.AddHours(13), ShiftStatus.ACTIVE);
            this.mockShiftRepository.Setup(repo => repo.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = this.service.IsDoctorFatigued("1");
            Assert.That(result, Is.True);
        }

        [Test]
        public void RaiseFatigueIntervention_AddsNotificationWithCorrectMessage()
        {
            this.service.RaiseFatigueIntervention(1, "Dr. John");

            this.mockNotificationRepository.Verify(repo => repo.AddNotification(0, "Fatigue Intervention Required",
                It.Is<string>(message => message.Contains("Dr. John") && message.Contains("exceeded the 12h duty limit"))), Times.Once);
        }

        // --- DeleteEvaluation ---
        [Test]
        public void DeleteEvaluation_ValidId_CallsRepository()
        {
            this.service.DeleteEvaluation(5);

            this.mockEvaluationsRepository.Verify(repository => repository.DeleteEvaluation(5), Times.Once);
        }

        [Test]
        public void DeleteEvaluation_AnyId_DelegatesToRepository()
        {
            this.service.DeleteEvaluation(99);

            this.mockEvaluationsRepository.Verify(repository => repository.DeleteEvaluation(99), Times.Once);
        }
    }
}