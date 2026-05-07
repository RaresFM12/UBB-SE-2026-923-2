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
        private Mock<IEvaluationsRepository> mockEvalRepo;
        private Mock<IHighRiskMedicineRepository> mockHighRiskRepo;
        private Mock<IAppointmentRepository> mockAppointmentRepo;
        private Mock<IStaffRepository> mockStaffRepo;
        private Mock<IShiftRepository> mockShiftRepo;
        private Mock<ICurrentUserService> mockCurrentUserService;
        private MedicalEvaluationService service;
        private Doctor doctor1;

        [SetUp]
        public void Setup()
        {
            mockEvalRepo = new Mock<IEvaluationsRepository>();
            mockHighRiskRepo = new Mock<IHighRiskMedicineRepository>();
            mockAppointmentRepo = new Mock<IAppointmentRepository>();
            mockStaffRepo = new Mock<IStaffRepository>();
            mockShiftRepo = new Mock<IShiftRepository>();
            mockCurrentUserService = new Mock<ICurrentUserService>();

            service = new MedicalEvaluationService(
                mockEvalRepo.Object,
                mockHighRiskRepo.Object,
                mockAppointmentRepo.Object,
                mockStaffRepo.Object,
                mockShiftRepo.Object,
                mockCurrentUserService.Object);

            doctor1 = new Doctor(1, "John", "Doe", "c", true, "Gen", "L1", DoctorStatus.AVAILABLE, 5);
            mockHighRiskRepo.Setup(r => r.GetAllHighRiskMedicines()).Returns(new List<(string, string)>());
            mockEvalRepo.Setup(r => r.GetAllEvaluations()).Returns(new List<MedicalEvaluation>());
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift>());
        }

        [Test]
        public void GetAllDoctors_ReturnsDoctorsOnly()
        {
            var pharmacist = new Pharmacyst(2, "B", "C", "c", true, "Cert", 3);
            mockStaffRepo.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { doctor1, pharmacist });
            var result = service.GetAllDoctors();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].StaffID, Is.EqualTo(1));
        }

        [Test]
        public void GetAllDoctors_Empty_ReturnsEmpty()
        {
            mockStaffRepo.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff>());
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
            mockAppointmentRepo.Setup(r => r.GetAllAppointmentsAsync()).ReturnsAsync(appointments);

            var result = service.GetAppointmentsByDoctor(1);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetEvaluationsByDoctor_ValidId_ReturnsFiltered()
        {
            var eval1 = new MedicalEvaluation { EvaluationID = 1, Evaluator = doctor1 };
            var eval2 = new MedicalEvaluation { EvaluationID = 2, Evaluator = new Doctor { StaffID = 2 } };
            mockEvalRepo.Setup(r => r.GetAllEvaluations()).Returns(new List<MedicalEvaluation> { eval1, eval2 });

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
            mockCurrentUserService.Setup(s => s.UserId).Returns(1);
            var eval = new MedicalEvaluation
            {
                PatientId = "5",
                Symptoms = "Fever",
                Notes = "Note",
                MedicationsList = "Aspirin",
                Evaluator = doctor1
            };

            service.SaveEvaluation(eval);
            mockEvalRepo.Verify(r => r.AddEvaluation(1, 5, "Fever", "Note", "Aspirin", false), Times.Once);
        }

        [Test]
        public void SaveEvaluation_RiskMarker_SetsAssumedRisk()
        {
            mockCurrentUserService.Setup(s => s.UserId).Returns(1);
            var eval = new MedicalEvaluation
            {
                PatientId = "5",
                Symptoms = "[RISK] Severe pain",
                Notes = "",
                MedicationsList = "",
                Evaluator = doctor1
            };

            service.SaveEvaluation(eval);
            mockEvalRepo.Verify(r => r.AddEvaluation(1, 5, "[RISK] Severe pain", "", "", true), Times.Once);
        }

        [Test]
        public void UpdateEvaluation_NullRecord_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => service.UpdateEvaluation(null));
        }

        [Test]
        public void UpdateEvaluation_ZeroId_Throws()
        {
            var eval = new MedicalEvaluation { EvaluationID = 0 };
            Assert.Throws<ArgumentException>(() => service.UpdateEvaluation(eval));
        }

        [Test]
        public void UpdateEvaluation_ValidRecord_CallsRepo()
        {
            var eval = new MedicalEvaluation { EvaluationID = 1, Symptoms = "S", Notes = "N", MedicationsList = "M" };
            service.UpdateEvaluation(eval);
            mockEvalRepo.Verify(r => r.UpdateEvaluation(1, "S", "N", "M"), Times.Once);
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
            mockHighRiskRepo.Setup(r => r.GetAllHighRiskMedicines())
                .Returns(new List<(string, string)> { ("Warfarin", "High bleeding risk") });

            var result = service.CheckMedicineConflict("P1", "Warfarin");
            Assert.That(result, Is.EqualTo("High bleeding risk"));
        }

        [Test]
        public void CheckMedicineConflict_HistoryAllergy_ReturnsAlert()
        {
            mockHighRiskRepo.Setup(r => r.GetAllHighRiskMedicines()).Returns(new List<(string, string)>());
            mockEvalRepo.Setup(r => r.GetAllEvaluations()).Returns(new List<MedicalEvaluation>
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
            mockHighRiskRepo.Setup(r => r.GetAllHighRiskMedicines()).Returns(new List<(string, string)>());
            mockEvalRepo.Setup(r => r.GetAllEvaluations()).Returns(new List<MedicalEvaluation>());

            var result = service.CheckMedicineConflict("P1", "Aspirin");
            Assert.That(result, Is.Null);
        }

        [Test]
        public void SaveEvaluation_NullPatientId_UsesDefault()
        {
            mockCurrentUserService.Setup(s => s.UserId).Returns(1);
            var eval = new MedicalEvaluation
            {
                PatientId = null,
                Symptoms = "",
                Notes = "",
                MedicationsList = "",
                Evaluator = doctor1
            };

            service.SaveEvaluation(eval);
            mockEvalRepo.Verify(r => r.AddEvaluation(1, 0, "", "", "", false), Times.Once);
        }

        [Test]
        public void SaveEvaluation_NoEvaluator_UsesCurrentUserId()
        {
            mockCurrentUserService.Setup(s => s.UserId).Returns(42);
            var eval = new MedicalEvaluation
            {
                PatientId = "1",
                Symptoms = "",
                Notes = "",
                MedicationsList = "",
                Evaluator = null
            };

            service.SaveEvaluation(eval);
            mockEvalRepo.Verify(r => r.AddEvaluation(42, 1, "", "", "", false), Times.Once);
        }
    }
}
