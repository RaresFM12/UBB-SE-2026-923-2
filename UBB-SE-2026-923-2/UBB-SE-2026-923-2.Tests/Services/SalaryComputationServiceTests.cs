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
        private Mock<IPharmacyHandoverRepository> mockHandoverRepository;
        private Mock<IHangoutRepository> mockHangoutRepository;
        private Mock<IHangoutParticipantRepository> mockParticipantRepository;
        private Mock<IStaffRepository> mockStaffRepository;
        private Mock<IShiftManagementShiftRepository> mockShiftRepository;
        private SalaryComputationService service;

        [SetUp]
        public void Setup()
        {
            mockHandoverRepository = new Mock<IPharmacyHandoverRepository>();
            mockHangoutRepository = new Mock<IHangoutRepository>();
            mockParticipantRepository = new Mock<IHangoutParticipantRepository>();
            mockStaffRepository = new Mock<IStaffRepository>();
            mockShiftRepository = new Mock<IShiftManagementShiftRepository>();
            service = new SalaryComputationService(
                mockHandoverRepository.Object,
                mockHangoutRepository.Object,
                mockParticipantRepository.Object,
                mockStaffRepository.Object,
                mockShiftRepository.Object);

            mockParticipantRepository.Setup(r => r.GetAllParticipants()).Returns(new List<(int, int)>());
            mockHangoutRepository.Setup(r => r.GetAllHangouts()).Returns(new List<Hangout>());
            mockHandoverRepository.Setup(r => r.GetAllPharmacyHandovers()).Returns(new List<PharmacyHandover>());
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
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0); // Wednesday
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(0));
            Assert.That(result, Is.GreaterThan(650));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_SaturdayShift_AppliesOvertimeMultiplier()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 0);
            var saturday = new DateTime(2025, 1, 11, 9, 0, 0); // Saturday
            var shift = new Shift(1, doctor, "Ward", saturday, saturday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(750));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_SundayShift_AppliesOvertimeMultiplier()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 0);
            var sunday = new DateTime(2025, 1, 12, 9, 0, 0); // Sunday
            var shift = new Shift(1, doctor, "Ward", sunday, sunday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(800));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_NightShift_AppliesNightMultiplier()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 0);
            var nightStart = new DateTime(2025, 1, 8, 22, 0, 0); // Wednesday night
            var shift = new Shift(1, doctor, "Ward", nightStart, nightStart.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(780));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_SurgeonSpecialization_AppliesBonus()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "Surgeon", "L1", DoctorStatus.AVAILABLE, 0);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(780));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_CardiologistSpecialization_AppliesBonus()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "Cardiologist", "L1", DoctorStatus.AVAILABLE, 0);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(750));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_ExperienceBonus_Applied()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 10);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(780));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_HangoutParticipation_AppliesBonus()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 0);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            mockParticipantRepository.Setup(r => r.GetAllParticipants()).Returns(new List<(int, int)> { (1, 1) });
            mockHangoutRepository.Setup(r => r.GetAllHangouts()).Returns(new List<Hangout>
            {
                new Hangout(1, "Fun", "Desc", new DateTime(2025, 1, 15), 10)
            });

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
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
            Assert.That(result, Is.EqualTo(360).Within(1));
        }

        [Test]
        public async Task ComputeSalaryPharmacistAsync_MedicinesSold_AppliesBonus()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "c", true, "Cert", 0);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, pharmacist, "Pharmacy", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            mockHandoverRepository.Setup(r => r.GetAllPharmacyHandovers()).Returns(
                Enumerable.Range(0, 20).Select(i => new PharmacyHandover
                {
                    PharmacistId = 1,
                    HandoverDate = new DateTime(2025, 1, 10)
                }).ToList());

            var result = await service.ComputeSalaryPharmacistAsync(pharmacist, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(360));
        }

        [Test]
        public void GetAllStaff_ReturnsFromRepo()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "Gen", "L1", DoctorStatus.AVAILABLE, 0);
            mockStaffRepository.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { doctor });
            var result = service.GetAllStaff();
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetAllStaff_NullRepo_ReturnsEmpty()
        {
            var serviceWithoutShiftRepository = new SalaryComputationService(mockHandoverRepository.Object, mockHangoutRepository.Object, mockParticipantRepository.Object);
            var result = serviceWithoutShiftRepository.GetAllStaff();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetAllShifts_ReturnsFromRepo()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "Gen", "L1", DoctorStatus.AVAILABLE, 0);
            var shift = new Shift(1, doctor, "A", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.ACTIVE);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });
            var result = service.GetAllShifts();
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetAllShifts_NullRepo_ReturnsEmpty()
        {
            var serviceWithoutShiftRepository = new SalaryComputationService(mockHandoverRepository.Object, mockHangoutRepository.Object, mockParticipantRepository.Object);
            var result = serviceWithoutShiftRepository.GetAllShifts();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_ERSpecialization_AppliesBonus()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "Emergency", "L1", DoctorStatus.AVAILABLE, 0);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(700));
        }

        [Test]
        public async Task ComputeSalaryPharmacistAsync_ExperienceBonus()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "c", true, "Cert", 5);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, pharmacist, "Pharmacy", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryPharmacistAsync(pharmacist, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.EqualTo(396).Within(1));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_MultipleShiftsSameMonth_SumsUp()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 0);
            var wed1 = new DateTime(2025, 1, 8, 9, 0, 0);
            var wed2 = new DateTime(2025, 1, 15, 9, 0, 0);
            var shifts = new List<Shift>
            {
                new Shift(1, doctor, "Ward", wed1, wed1.AddHours(8), ShiftStatus.COMPLETED),
                new Shift(2, doctor, "Ward", wed2, wed2.AddHours(8), ShiftStatus.COMPLETED),
            };

            var result = await service.ComputeSalaryDoctorAsync(doctor, shifts, 1, 2025);
            Assert.That(result, Is.GreaterThan(1300));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_ZeroExperience_NoExperienceBonus()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 0);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(600));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_FridayNightIntoSaturday_AppliesNightMultiplier()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 0);
            var fridayNight = new DateTime(2025, 1, 10, 22, 0, 0); // Friday night
            var shift = new Shift(1, doctor, "Ward", fridayNight, fridayNight.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(680));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_NoHangoutParticipation_NoHangoutBonus()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 0);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            mockParticipantRepository.Setup(r => r.GetAllParticipants()).Returns(new List<(int, int)>());
            mockHangoutRepository.Setup(r => r.GetAllHangouts()).Returns(new List<Hangout>());

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(600));
        }

        [Test]
        public async Task ComputeSalaryPharmacistAsync_SaturdayShift_AppliesOvertimeMultiplier()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "c", true, "Cert", 0);
            var saturday = new DateTime(2025, 1, 11, 9, 0, 0);
            var shift = new Shift(1, pharmacist, "Pharmacy", saturday, saturday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryPharmacistAsync(pharmacist, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(400));
        }

        [Test]
        public async Task ComputeSalaryPharmacistAsync_SundayShift_AppliesOvertimeMultiplier()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "c", true, "Cert", 0);
            var sunday = new DateTime(2025, 1, 12, 9, 0, 0);
            var shift = new Shift(1, pharmacist, "Pharmacy", sunday, sunday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryPharmacistAsync(pharmacist, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(440));
        }

        [Test]
        public async Task ComputeSalaryPharmacistAsync_NightShift_AppliesNightMultiplier()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "c", true, "Cert", 0);
            var nightStart = new DateTime(2025, 1, 8, 22, 0, 0);
            var shift = new Shift(1, pharmacist, "Pharmacy", nightStart, nightStart.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryPharmacistAsync(pharmacist, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(420));
        }

        [Test]
        public async Task ComputeSalaryPharmacistAsync_MultipleShifts_SumsUp()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "c", true, "Cert", 0);
            var wed1 = new DateTime(2025, 1, 8, 9, 0, 0);
            var wed2 = new DateTime(2025, 1, 15, 9, 0, 0);
            var shifts = new List<Shift>
            {
                new Shift(1, pharmacist, "Pharmacy", wed1, wed1.AddHours(8), ShiftStatus.COMPLETED),
                new Shift(2, pharmacist, "Pharmacy", wed2, wed2.AddHours(8), ShiftStatus.COMPLETED),
            };

            var result = await service.ComputeSalaryPharmacistAsync(pharmacist, shifts, 1, 2025);
            Assert.That(result, Is.EqualTo(720).Within(5));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_HighExperience_LargeBonus()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 25);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(950));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_SurgeonWithExperience_CombinesBonuses()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "Surgeon", "L1", DoctorStatus.AVAILABLE, 10);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(900));
        }
    }
}
