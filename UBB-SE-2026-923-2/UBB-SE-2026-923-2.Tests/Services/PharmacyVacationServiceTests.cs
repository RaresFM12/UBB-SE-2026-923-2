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
    public class PharmacyVacationServiceLogicTests
    {
        private Mock<IPharmacyStaffRepository> mockPharmacyStaffRepository;
        private Mock<IPharmacyShiftRepository> mockPharmacyShiftRepository;
        private PharmacyVacationService pharmacyVacationService;

        [SetUp]
        public void Setup()
        {
            this.mockPharmacyStaffRepository = new Mock<IPharmacyStaffRepository>();
            this.mockPharmacyShiftRepository = new Mock<IPharmacyShiftRepository>();

            this.pharmacyVacationService = new PharmacyVacationService(
                this.mockPharmacyStaffRepository.Object,
                this.mockPharmacyShiftRepository.Object);
        }

        [Test]
        public void Constructor_WhenStaffRepositoryIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new PharmacyVacationService(null, this.mockPharmacyShiftRepository.Object));
        }

        [Test]
        public void GetPharmacists_WhenPharmacistsAreUnordered_ReturnsPharmacistsOrderedByFirstName()
        {
            this.mockPharmacyStaffRepository
                .Setup(pharmacyStaffRepository => pharmacyStaffRepository.GetPharmacists())
                .Returns(new List<Pharmacyst>
                {
                    CreatePharmacist(2, "Charlie", "Zeta"),
                    CreatePharmacist(1, "Alice", "Alpha"),
                });

            var pharmacists = this.pharmacyVacationService.GetPharmacists();

            Assert.That(pharmacists[0].FirstName, Is.EqualTo("Alice"));
        }

        [Test]
        public void RegisterVacation_WhenEndDateIsBeforeStartDate_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => this.pharmacyVacationService.RegisterVacation(1, DateTime.Today.AddDays(5), DateTime.Today.AddDays(3)));
        }

        [Test]
        public void RegisterVacation_WhenPharmacistDoesNotExist_ThrowsArgumentException()
        {
            this.mockPharmacyStaffRepository
                .Setup(pharmacyStaffRepository => pharmacyStaffRepository.GetPharmacists())
                .Returns(new List<Pharmacyst>());

            Assert.Throws<ArgumentException>(
                () => this.pharmacyVacationService.RegisterVacation(1, DateTime.Today.AddDays(3), DateTime.Today.AddDays(5)));
        }

        [Test]
        public void RegisterVacation_WhenVacationOverlapsExistingShift_ThrowsInvalidOperationException()
        {
            var requestedPharmacist = CreatePharmacist(1, "Alice", "Smith");

            this.mockPharmacyStaffRepository
                .Setup(pharmacyStaffRepository => pharmacyStaffRepository.GetPharmacists())
                .Returns(new List<Pharmacyst> { requestedPharmacist });

            this.mockPharmacyShiftRepository
                .Setup(pharmacyShiftRepository => pharmacyShiftRepository.GetAllShifts())
                .Returns(new List<Shift>
                {
                    new Shift(
                        1,
                        requestedPharmacist,
                        "Pharmacy",
                        DateTime.Today.AddDays(4),
                        DateTime.Today.AddDays(4).AddHours(8),
                        ShiftStatus.ACTIVE),
                });

            Assert.Throws<InvalidOperationException>(
                () => this.pharmacyVacationService.RegisterVacation(1, DateTime.Today.AddDays(4), DateTime.Today.AddDays(5)));
        }

        [Test]
        public void RegisterVacation_WhenVacationDoesNotOverlapExistingShift_AddsVacationShift()
        {
            var requestedPharmacist = CreatePharmacist(1, "Alice", "Smith");

            this.mockPharmacyStaffRepository
                .Setup(pharmacyStaffRepository => pharmacyStaffRepository.GetPharmacists())
                .Returns(new List<Pharmacyst> { requestedPharmacist });

            this.mockPharmacyShiftRepository
                .Setup(pharmacyShiftRepository => pharmacyShiftRepository.GetAllShifts())
                .Returns(new List<Shift>());

            this.pharmacyVacationService.RegisterVacation(1, DateTime.Today.AddDays(4), DateTime.Today.AddDays(5));

            this.mockPharmacyShiftRepository.Verify(
                pharmacyShiftRepository => pharmacyShiftRepository.AddShift(
                    It.Is<Shift>(vacationShift => vacationShift.Status == ShiftStatus.VACATION)),
                Times.Once);
        }

        private static Pharmacyst CreatePharmacist(int pharmacistIdentifier, string firstName, string lastName)
        {
            return new Pharmacyst(pharmacistIdentifier, firstName, lastName, "contract", true, "", 10);
        }
    }
}