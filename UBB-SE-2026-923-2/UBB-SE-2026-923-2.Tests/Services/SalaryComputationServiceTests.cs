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
    public class SalaryComputationServiceTests
    {
        private Mock<IPharmacyHandoverRepository> mockHandoverRepo;
        private Mock<IHangoutRepository> mockHangoutRepo;
        private Mock<IHangoutParticipantRepository> mockParticipantRepo;
        private Mock<IStaffRepository> mockStaffRepo;
        private Mock<IShiftManagementShiftRepository> mockShiftRepo;
        private SalaryComputationService service;

        [SetUp]
        public void Setup()
        {
            mockHandoverRepo = new Mock<IPharmacyHandoverRepository>();
            mockHangoutRepo = new Mock<IHangoutRepository>();
            mockParticipantRepo = new Mock<IHangoutParticipantRepository>();
            mockStaffRepo = new Mock<IStaffRepository>();
            mockShiftRepo = new Mock<IShiftManagementShiftRepository>();
            service = new SalaryComputationService(
                mockHandoverRepo.Object,
                mockHangoutRepo.Object,
                mockParticipantRepo.Object,
                mockStaffRepo.Object,
                mockShiftRepo.Object);

            mockParticipantRepo.Setup(r => r.GetAllParticipants()).Returns(new List<(int, int)>());
            mockHangoutRepo.Setup(r => r.GetAllHangouts()).Returns(new List<Hangout>());
            mockHandoverRepo.Setup(r => r.GetAllPharmacyHandovers()).Returns(new List<PharmacyHandover>());
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_NoShifts_ReturnsZero()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 0);
            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift>(), 1, 2025);
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_SingleWeekdayShift_CalculatesCorrectly()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 0);
            // Wednesday shift, 8 hours, rate = 85
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0); // Wednesday
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(0));
            // Base: 8 * 85 = 680
            Assert.That(result, Is.GreaterThan(650));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_SaturdayShift_AppliesOvertimeMultiplier()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 0);
            var saturday = new DateTime(2025, 1, 11, 9, 0, 0); // Saturday
            var shift = new Shift(1, doctor, "Ward", saturday, saturday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            // 8 * 85 * 1.15 = 782
            Assert.That(result, Is.GreaterThan(750));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_SundayShift_AppliesOvertimeMultiplier()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 0);
            var sunday = new DateTime(2025, 1, 12, 9, 0, 0); // Sunday
            var shift = new Shift(1, doctor, "Ward", sunday, sunday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            // 8 * 85 * 1.25 = 850
            Assert.That(result, Is.GreaterThan(800));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_NightShift_AppliesNightMultiplier()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 0);
            var nightStart = new DateTime(2025, 1, 8, 22, 0, 0); // Wednesday night
            var shift = new Shift(1, doctor, "Ward", nightStart, nightStart.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            // 8 * 85 * 1.20 = 816
            Assert.That(result, Is.GreaterThan(780));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_SurgeonSpecialization_AppliesBonus()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "Surgeon", "L1", DoctorStatus.AVAILABLE, 0);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            // 680 + 680 * 0.20 = 816
            Assert.That(result, Is.GreaterThan(780));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_CardiologistSpecialization_AppliesBonus()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "Cardiologist", "L1", DoctorStatus.AVAILABLE, 0);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            // 680 + 680 * 0.15 = 782
            Assert.That(result, Is.GreaterThan(750));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_ExperienceBonus_Applied()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 10);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            // 680 + 680 * 10 * 0.02 = 680 + 136 = 816
            Assert.That(result, Is.GreaterThan(780));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_HangoutParticipation_AppliesBonus()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 0);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            mockParticipantRepo.Setup(r => r.GetAllParticipants()).Returns(new List<(int, int)> { (1, 1) });
            mockHangoutRepo.Setup(r => r.GetAllHangouts()).Returns(new List<Hangout>
            {
                new Hangout(1, "Fun", "Desc", new DateTime(2025, 1, 15), 10)
            });

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            // 680 * 1.05 = 714
            Assert.That(result, Is.GreaterThan(700));
        }

        [Test]
        public async Task ComputeSalaryPharmacistAsync_NoShifts_ReturnsZero()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "c", true, "Cert", 0);
            var result = await service.ComputeSalaryPharmacistAsync(pharmacist, new List<Shift>(), 1, 2025);
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task ComputeSalaryPharmacistAsync_WeekdayShift_CalculatesCorrectly()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "c", true, "Cert", 0);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, pharmacist, "Pharmacy", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryPharmacistAsync(pharmacist, new List<Shift> { shift }, 1, 2025);
            // 8 * 45 = 360
            Assert.That(result, Is.EqualTo(360).Within(1));
        }

        [Test]
        public async Task ComputeSalaryPharmacistAsync_MedicinesSold_AppliesBonus()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "c", true, "Cert", 0);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, pharmacist, "Pharmacy", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            mockHandoverRepo.Setup(r => r.GetAllPharmacyHandovers()).Returns(
                Enumerable.Range(0, 20).Select(i => new PharmacyHandover
                {
                    PharmacistId = 1,
                    HandoverDate = new DateTime(2025, 1, 10)
                }).ToList());

            var result = await service.ComputeSalaryPharmacistAsync(pharmacist, new List<Shift> { shift }, 1, 2025);
            // 360 + 360 * (20/10 * 0.01) = 360 + 7.2 = 367.2
            Assert.That(result, Is.GreaterThan(360));
        }

        [Test]
        public void GetAllStaff_ReturnsFromRepo()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "Gen", "L1", DoctorStatus.AVAILABLE, 0);
            mockStaffRepo.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { doctor });
            var result = service.GetAllStaff();
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetAllStaff_NullRepo_ReturnsEmpty()
        {
            var svc = new SalaryComputationService(mockHandoverRepo.Object, mockHangoutRepo.Object, mockParticipantRepo.Object);
            var result = svc.GetAllStaff();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetAllShifts_ReturnsFromRepo()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "Gen", "L1", DoctorStatus.AVAILABLE, 0);
            var shift = new Shift(1, doctor, "A", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.ACTIVE);
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });
            var result = service.GetAllShifts();
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetAllShifts_NullRepo_ReturnsEmpty()
        {
            var svc = new SalaryComputationService(mockHandoverRepo.Object, mockHangoutRepo.Object, mockParticipantRepo.Object);
            var result = svc.GetAllShifts();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_ERSpecialization_AppliesBonus()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "Emergency", "L1", DoctorStatus.AVAILABLE, 0);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            // 680 + 680 * 0.10 = 748
            Assert.That(result, Is.GreaterThan(700));
        }

        [Test]
        public async Task ComputeSalaryPharmacistAsync_ExperienceBonus()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "c", true, "Cert", 5);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, pharmacist, "Pharmacy", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryPharmacistAsync(pharmacist, new List<Shift> { shift }, 1, 2025);
            // 360 + 360 * 5 * 0.02 = 360 + 36 = 396
            Assert.That(result, Is.EqualTo(396).Within(1));
        }
    }
}
