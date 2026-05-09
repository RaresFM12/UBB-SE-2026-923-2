namespace UBB_SE_2026_923_2.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Moq;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class SalaryComputationServiceLogicTests
    {
        private Mock<IPharmacyHandoverRepository> mockPharmacyHandoverRepository;
        private Mock<IHangoutRepository> mockHangoutRepository;
        private Mock<IHangoutParticipantRepository> mockHangoutParticipantRepository;
        private SalaryComputationService salaryComputationService;

        [SetUp]
        public void Setup()
        {
            this.mockPharmacyHandoverRepository = new Mock<IPharmacyHandoverRepository>();
            this.mockHangoutRepository = new Mock<IHangoutRepository>();
            this.mockHangoutParticipantRepository = new Mock<IHangoutParticipantRepository>();

            this.mockPharmacyHandoverRepository
                .Setup(pharmacyHandoverRepository => pharmacyHandoverRepository.GetAllPharmacyHandovers())
                .Returns(new List<PharmacyHandover>());

            this.mockHangoutRepository
                .Setup(hangoutRepository => hangoutRepository.GetAllHangouts())
                .Returns(new List<Hangout>());

            this.mockHangoutParticipantRepository
                .Setup(hangoutParticipantRepository => hangoutParticipantRepository.GetAllParticipants())
                .Returns(new List<(int HangoutId, int StaffId)>());

            this.salaryComputationService = new SalaryComputationService(
                this.mockPharmacyHandoverRepository.Object,
                this.mockHangoutRepository.Object,
                this.mockHangoutParticipantRepository.Object);
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_WhenDoctorHasRegularEightHourShift_ReturnsBaseSalary()
        {
            var doctor = CreateDoctor(1, "General", 0);
            var monthlyShifts = new List<Shift>
            {
                new Shift(1, doctor, "Ward A", new DateTime(2025, 1, 6, 8, 0, 0), new DateTime(2025, 1, 6, 16, 0, 0), ShiftStatus.ACTIVE),
            };

            var computedSalary = await this.salaryComputationService.ComputeSalaryDoctorAsync(doctor, monthlyShifts, 1, 2025);

            Assert.That(computedSalary, Is.EqualTo(680));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_WhenDoctorIsCardiologist_AddsSpecializationBonus()
        {
            var doctor = CreateDoctor(1, "Cardiologist", 0);
            var monthlyShifts = new List<Shift>
            {
                new Shift(1, doctor, "Ward A", new DateTime(2025, 1, 6, 8, 0, 0), new DateTime(2025, 1, 6, 16, 0, 0), ShiftStatus.ACTIVE),
            };

            var computedSalary = await this.salaryComputationService.ComputeSalaryDoctorAsync(doctor, monthlyShifts, 1, 2025);

            Assert.That(computedSalary, Is.EqualTo(782));
        }

        [Test]
        public async Task ComputeSalaryDoctorAsync_WhenDoctorParticipatedInHangoutForRequestedMonth_AppliesHangoutBonus()
        {
            var doctor = CreateDoctor(1, "General", 0);
            var monthlyShifts = new List<Shift>
            {
                new Shift(1, doctor, "Ward A", new DateTime(2025, 1, 6, 8, 0, 0), new DateTime(2025, 1, 6, 16, 0, 0), ShiftStatus.ACTIVE),
            };

            this.mockHangoutParticipantRepository
                .Setup(hangoutParticipantRepository => hangoutParticipantRepository.GetAllParticipants())
                .Returns(new List<(int HangoutId, int StaffId)> { (10, 1) });

            this.mockHangoutRepository
                .Setup(hangoutRepository => hangoutRepository.GetAllHangouts())
                .Returns(new List<Hangout>
                {
                    new Hangout { HangoutID = 10, Date = new DateTime(2025, 1, 20) },
                });

            var computedSalary = await this.salaryComputationService.ComputeSalaryDoctorAsync(doctor, monthlyShifts, 1, 2025);

            Assert.That(computedSalary, Is.EqualTo(714));
        }

        [Test]
        public async Task ComputeSalaryPharmacistAsync_WhenPharmacistHasRegularEightHourShift_ReturnsBaseSalary()
        {
            var pharmacist = CreatePharmacist(2, 0);
            var monthlyShifts = new List<Shift>
            {
                new Shift(1, pharmacist, "Pharmacy", new DateTime(2025, 1, 6, 8, 0, 0), new DateTime(2025, 1, 6, 16, 0, 0), ShiftStatus.ACTIVE),
            };

            var computedSalary = await this.salaryComputationService.ComputeSalaryPharmacistAsync(pharmacist, monthlyShifts, 1, 2025);

            Assert.That(computedSalary, Is.EqualTo(360));
        }

        [Test]
        public async Task ComputeSalaryPharmacistAsync_WhenPharmacistHasYearsOfExperience_AddsExperienceBonus()
        {
            var pharmacist = CreatePharmacist(2, 5);
            var monthlyShifts = new List<Shift>
            {
                new Shift(1, pharmacist, "Pharmacy", new DateTime(2025, 1, 6, 8, 0, 0), new DateTime(2025, 1, 6, 16, 0, 0), ShiftStatus.ACTIVE),
            };

            var computedSalary = await this.salaryComputationService.ComputeSalaryPharmacistAsync(pharmacist, monthlyShifts, 1, 2025);

            Assert.That(computedSalary, Is.EqualTo(396));
        }

        private static Doctor CreateDoctor(int doctorIdentifier, string specialization, int yearsOfExperience)
        {
            return new Doctor(doctorIdentifier, "John", "Doe", "contract", true, specialization, "License", DoctorStatus.AVAILABLE, yearsOfExperience);
        }

        private static Pharmacyst CreatePharmacist(int pharmacistIdentifier, int yearsOfExperience)
        {
            var pharmacist = new Pharmacyst(pharmacistIdentifier, "Alice", "Smith", "contract", true, "", 10);
            pharmacist.YearsOfExperience = yearsOfExperience;
            return pharmacist;
        }
    }
}