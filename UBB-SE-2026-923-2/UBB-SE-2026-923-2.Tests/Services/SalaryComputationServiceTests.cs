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
            this.mockHandoverRepository = new Mock<IPharmacyHandoverRepository>();
            this.mockHangoutRepository = new Mock<IHangoutRepository>();
            this.mockParticipantRepository = new Mock<IHangoutParticipantRepository>();
            this.mockStaffRepository = new Mock<IStaffRepository>();
            this.mockShiftRepository = new Mock<IShiftManagementShiftRepository>();

            this.service = new SalaryComputationService(
                this.mockHandoverRepository.Object,
                this.mockHangoutRepository.Object,
                this.mockParticipantRepository.Object,
                this.mockStaffRepository.Object,
                this.mockShiftRepository.Object);

            this.mockParticipantRepository.Setup(repository => repository.GetAllParticipants()).Returns(new List<(int, int)>());
            this.mockHangoutRepository.Setup(repository => repository.GetAllHangouts()).Returns(new List<Hangout>());
            this.mockHandoverRepository.Setup(repository => repository.GetAllPharmacyHandovers()).Returns(new List<PharmacyHandover>());
        }

        // --- Doctor Salary Tests ---
        [Test]
        public async Task ComputeSalaryDoctorAsync_NoShifts_ReturnsZero()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 0);
            var result = await this.service.ComputeSalaryDoctorAsync(doctor, new List<Shift>(), 1, 2025);
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_SingleWeekdayShift_CalculatesCorrectly()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 0);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            var result = await this.service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(0));
            Assert.That(result, Is.GreaterThan(650));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_WeekendAndNightShifts_ApplyMultipliers()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "General", "L1", DoctorStatus.AVAILABLE, 0);
            var saturday = new DateTime(2025, 1, 11, 9, 0, 0);
            var sunday = new DateTime(2025, 1, 12, 9, 0, 0);
            var night = new DateTime(2025, 1, 8, 22, 0, 0);

            var shifts = new List<Shift>
            {
                new Shift(1, doctor, "Ward", saturday, saturday.AddHours(8), ShiftStatus.COMPLETED),
                new Shift(2, doctor, "Ward", sunday, sunday.AddHours(8), ShiftStatus.COMPLETED),
                new Shift(3, doctor, "Ward", night, night.AddHours(8), ShiftStatus.COMPLETED)
            };

            var result = await this.service.ComputeSalaryDoctorAsync(doctor, shifts, 1, 2025);

            // Base for 24h = 24 * 85 = 2040. Multipliers should push this higher.
            Assert.That(result, Is.GreaterThan(2040));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_SpecializationAndHangout_AppliesBonuses()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "Surgeon", "L1", DoctorStatus.AVAILABLE, 10);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, doctor, "Ward", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            this.mockParticipantRepository.Setup(repository => repository.GetAllParticipants()).Returns(new List<(int, int)> { (1, 1) });
            this.mockHangoutRepository.Setup(repository => repository.GetAllHangouts()).Returns(new List<Hangout>
            {
                new Hangout(1, "Fun", "Desc", new DateTime(2025, 1, 15), 10),
            });

            var result = await this.service.ComputeSalaryDoctorAsync(doctor, new List<Shift> { shift }, 1, 2025);
            Assert.That(result, Is.GreaterThan(800));
        }

        // --- Pharmacist Salary Tests ---
        [Test]
        public async Task ComputeSalaryPharmacistAsync_NoShifts_ReturnsZero()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "c", true, "Cert", 0);
            var result = await this.service.ComputeSalaryPharmacistAsync(pharmacist, new List<Shift>(), 1, 2025);
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task ComputeSalaryPharmacistAsync_MedicinesSoldAndExperience_AppliesBonuses()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "c", true, "Cert", 5);
            var wednesday = new DateTime(2025, 1, 8, 9, 0, 0);
            var shift = new Shift(1, pharmacist, "Pharmacy", wednesday, wednesday.AddHours(8), ShiftStatus.COMPLETED);

            this.mockHandoverRepository.Setup(repository => repository.GetAllPharmacyHandovers()).Returns(
                Enumerable.Range(0, 20).Select(index => new PharmacyHandover
                {
                    Pharmacist = new Staff { StaffID = 1 },
                    HandoverDate = new DateTime(2025, 1, 10),
                }).ToList());

            var result = await this.service.ComputeSalaryPharmacistAsync(pharmacist, new List<Shift> { shift }, 1, 2025);

            // Base = 8 * 45 = 360. Bonuses push it higher.
            Assert.That(result, Is.GreaterThan(360));
        }

        // --- Passthrough Tests ---
        [Test]
        public void GetAllStaff_ReturnsFromRepo()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "Gen", "L1", DoctorStatus.AVAILABLE, 0);
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { doctor });
            var result = this.service.GetAllStaff();
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetAllShifts_ReturnsFromRepo()
        {
            var doctor = new Doctor(1, "A", "B", "c", true, "Gen", "L1", DoctorStatus.AVAILABLE, 0);
            var shift = new Shift(1, doctor, "A", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.ACTIVE);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });
            var result = this.service.GetAllShifts();
            Assert.That(result.Count, Is.EqualTo(1));
        }
    }
}