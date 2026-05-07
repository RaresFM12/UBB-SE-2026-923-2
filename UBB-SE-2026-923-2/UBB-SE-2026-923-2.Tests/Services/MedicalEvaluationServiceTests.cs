using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.Tests.Services
{
    [TestFixture]
    public class MedicalEvaluationServiceTests
    {
        private Mock<IEvaluationsRepository> mockEvaluationsRepository;
        private Mock<IHighRiskMedicineRepository> mockHighRiskMedicineRepository;
        private Mock<IAppointmentRepository> mockAppointmentRepository;
        private Mock<IStaffRepository> mockStaffRepository;
        private Mock<IShiftRepository> mockShiftRepository;
        private Mock<ICurrentUserService> mockCurrentUserService;
        private MedicalEvaluationService service;
        private Doctor doctor1;

        [SetUp]
        public void Setup()
        {
            mockEvaluationsRepository = new Mock<IEvaluationsRepository>();
            mockHighRiskMedicineRepository = new Mock<IHighRiskMedicineRepository>();
            mockAppointmentRepository = new Mock<IAppointmentRepository>();
            mockStaffRepository = new Mock<IStaffRepository>();
            mockShiftRepository = new Mock<IShiftRepository>();
            mockCurrentUserService = new Mock<ICurrentUserService>();

            service = new MedicalEvaluationService(
                mockEvaluationsRepository.Object,
                mockHighRiskMedicineRepository.Object,
                mockAppointmentRepository.Object,
                mockStaffRepository.Object,
                mockShiftRepository.Object,
                mockCurrentUserService.Object);

            doctor1 = new Doctor(1, "John", "Doe", "c", true, "Gen", "L1", DoctorStatus.AVAILABLE, 5);
            mockHighRiskMedicineRepository.Setup(repository => repository.GetAllHighRiskMedicines()).Returns(new List<(string, string)>());
            mockEvaluationsRepository.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation>());
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
        }

        [Test]
        public void GetAllDoctors_ReturnsDoctorsOnly()
        {
            var pharmacist = new Pharmacyst(2, "B", "C", "c", true, "Cert", 3);
            mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { doctor1, pharmacist });
            var result = service.GetAllDoctors();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].StaffID, Is.EqualTo(1));
        }

        [Test]
        public void GetAllDoctors_Empty_ReturnsEmpty()
        {
            mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff>());
            var result = service.GetAllDoctors();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetAppointmentsByDoctor_ReturnsConfirmedOnly()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { DoctorId = 1, Status = "Confirmed", Date = DateTime.Now, StartTime = TimeSpan.FromHours(9) },
                new Appointment { DoctorId = 1, Status = "Cancelled", Date = DateTime.Now, StartTime = TimeSpan.FromHours(10) },
                new Appointment { DoctorId = 2, Status = "Confirmed", Date = DateTime.Now, StartTime = TimeSpan.FromHours(11) },
            };
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(appointments);

            var result = service.GetAppointmentsByDoctor(1);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetEvaluationsByDoctor_ValidId_ReturnsFiltered()
        {
            var eval1 = new MedicalEvaluation { EvaluationID = 1, Evaluator = doctor1 };
            var eval2 = new MedicalEvaluation { EvaluationID = 2, Evaluator = new Doctor { StaffID = 2 } };
            mockEvaluationsRepository.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation> { eval1, eval2 });

            var result = service.GetEvaluationsByDoctor("1");
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].EvaluationID, Is.EqualTo(1));
        }

        [Test]
        public void GetEvaluationsByDoctor_InvalidId_ReturnsEmpty()
        {
            var result = service.GetEvaluationsByDoctor("abc");
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void SaveEvaluation_NullRecord_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => service.SaveEvaluation(null));
        }

        [Test]
        public void SaveEvaluation_ValidRecord_CallsRepo()
        {
            mockCurrentUserService.Setup(service => service.UserId).Returns(1);
            var evaluation = new MedicalEvaluation
            {
                PatientId = "5",
                Symptoms = "Fever",
                Notes = "Note",
                MedicationsList = "Aspirin",
                Evaluator = doctor1
            };

            service.SaveEvaluation(evaluation);
            mockEvaluationsRepository.Verify(repository => repository.AddEvaluation(1, 5, "Fever", "Note", "Aspirin", false), Times.Once);
        }

        [Test]
        public void SaveEvaluation_RiskMarker_SetsAssumedRisk()
        {
            mockCurrentUserService.Setup(service => service.UserId).Returns(1);
            var evaluation = new MedicalEvaluation
            {
                PatientId = "5",
                Symptoms = "[RISK] Severe pain",
                Notes = "",
                MedicationsList = "",
                Evaluator = doctor1
            };

            service.SaveEvaluation(evaluation);
            mockEvaluationsRepository.Verify(repository => repository.AddEvaluation(1, 5, "[RISK] Severe pain", "", "", true), Times.Once);
        }

        [Test]
        public void UpdateEvaluation_NullRecord_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => service.UpdateEvaluation(null));
        }

        [Test]
        public void UpdateEvaluation_ZeroId_Throws()
        {
            var evaluation = new MedicalEvaluation { EvaluationID = 0 };
            Assert.Throws<ArgumentException>(() => service.UpdateEvaluation(evaluation));
        }

        [Test]
        public void UpdateEvaluation_ValidRecord_CallsRepo()
        {
            var evaluation = new MedicalEvaluation { EvaluationID = 1, Symptoms = "S", Notes = "N", MedicationsList = "M" };
            service.UpdateEvaluation(evaluation);
            mockEvaluationsRepository.Verify(repository => repository.UpdateEvaluation(1, "S", "N", "M"), Times.Once);
        }

        [Test]
        public void CheckMedicineConflict_NullMedications_ReturnsNull()
        {
            var result = service.CheckMedicineConflict("P1", null);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void CheckMedicineConflict_EmptyMedications_ReturnsNull()
        {
            var result = service.CheckMedicineConflict("P1", "");
            Assert.That(result, Is.Null);
        }

        [Test]
        public void CheckMedicineConflict_NullPatientId_ReturnsNull()
        {
            var result = service.CheckMedicineConflict(null, "Aspirin");
            Assert.That(result, Is.Null);
        }

        [Test]
        public void CheckMedicineConflict_HighRiskMedicine_ReturnsWarning()
        {
            mockHighRiskMedicineRepository.Setup(repository => repository.GetAllHighRiskMedicines())
                .Returns(new List<(string, string)> { ("Warfarin", "High bleeding risk") });

            var result = service.CheckMedicineConflict("P1", "Warfarin");
            Assert.That(result, Is.EqualTo("High bleeding risk"));
        }

        [Test]
        public void CheckMedicineConflict_HistoryAllergy_ReturnsAlert()
        {
            mockHighRiskMedicineRepository.Setup(repository => repository.GetAllHighRiskMedicines()).Returns(new List<(string, string)>());
            mockEvaluationsRepository.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation>
            {
                new MedicalEvaluation
                {
                    PatientId = "P1",
                    Symptoms = "Allergy to medication",
                    MedicationsList = "Aspirin",
                    Notes = ""
                }
            });

            var result = service.CheckMedicineConflict("P1", "Aspirin");
            Assert.That(result, Does.Contain("HISTORY ALERT"));
        }

        [Test]
        public void CheckMedicineConflict_NoConflict_ReturnsNull()
        {
            mockHighRiskMedicineRepository.Setup(repository => repository.GetAllHighRiskMedicines()).Returns(new List<(string, string)>());
            mockEvaluationsRepository.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation>());

            var result = service.CheckMedicineConflict("P1", "Aspirin");
            Assert.That(result, Is.Null);
        }

        [Test]
        public void SaveEvaluation_NullPatientId_UsesDefault()
        {
            mockCurrentUserService.Setup(service => service.UserId).Returns(1);
            var evaluation = new MedicalEvaluation
            {
                PatientId = null,
                Symptoms = "",
                Notes = "",
                MedicationsList = "",
                Evaluator = doctor1
            };

            service.SaveEvaluation(evaluation);
            mockEvaluationsRepository.Verify(repository => repository.AddEvaluation(1, 0, "", "", "", false), Times.Once);
        }

        [Test]
        public void SaveEvaluation_NoEvaluator_UsesCurrentUserId()
        {
            mockCurrentUserService.Setup(service => service.UserId).Returns(42);
            var evaluation = new MedicalEvaluation
            {
                PatientId = "1",
                Symptoms = "",
                Notes = "",
                MedicationsList = "",
                Evaluator = null
            };

            service.SaveEvaluation(evaluation);
            mockEvaluationsRepository.Verify(repository => repository.AddEvaluation(42, 1, "", "", "", false), Times.Once);
        }
    }
}


