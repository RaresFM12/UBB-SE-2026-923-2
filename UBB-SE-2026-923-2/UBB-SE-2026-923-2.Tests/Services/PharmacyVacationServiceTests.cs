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
    public class PharmacyVacationServiceTests
    {
        private Mock<IPharmacyStaffRepository> mockStaffRepository;
        private Mock<IPharmacyShiftRepository> mockShiftRepository;
        private PharmacyVacationService service;

        [SetUp]
        public void Setup()
        {
            this.mockStaffRepository = new Mock<IPharmacyStaffRepository>();
            this.mockShiftRepository = new Mock<IPharmacyShiftRepository>();
            this.service = new PharmacyVacationService(this.mockStaffRepository.Object, this.mockShiftRepository.Object);
        }

        [Test]
        public void Constructor_NullStaffRepo_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new PharmacyVacationService(null, this.mockShiftRepository.Object));
        }

        [Test]
        public void Constructor_NullShiftRepo_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new PharmacyVacationService(this.mockStaffRepository.Object, null));
        }

        // --- GetPharmacists ---
        [Test]
        public void GetPharmacists_ReturnsOrderedByNameAndLastName()
        {
            var pharmacists = new List<Pharmacyst>
            {
                new Pharmacyst(1, "Zoe", "Adams", string.Empty, true, "cert", 5),
                new Pharmacyst(2, "Alice", "Brown", string.Empty, true, "cert2", 3),
                new Pharmacyst(3, "Alice", "Adams", string.Empty, true, "cert3", 2),
            };
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(pharmacists);

            var result = this.service.GetPharmacists();

            Assert.That(result[0].FirstName, Is.EqualTo("Alice"));
        }

        // --- RegisterVacation ---
        [Test]
        public void RegisterVacation_EndBeforeStart_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                this.service.RegisterVacation(1, DateTime.Now.AddDays(5), DateTime.Now.AddDays(2)));
        }

        [Test]
        public void RegisterVacation_PharmacistNotFound_Throws()
        {
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst>());

            Assert.Throws<ArgumentException>(() =>
                this.service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3)));
        }

        [Test]
        public void RegisterVacation_OverlapsExistingShift_Throws()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5);
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            var existingShift = new Shift(1, pharmacist, "Pharmacy", DateTime.Now.AddDays(2), DateTime.Now.AddDays(3), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { existingShift });

            Assert.Throws<InvalidOperationException>(() =>
                this.service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(4)));
        }

        [Test]
        public void RegisterVacation_OverlapsExistingVacation_ThrowsWithVacationMessage()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5);
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            var existingShift = new Shift(1, pharmacist, "Vacation", DateTime.Now.AddDays(2), DateTime.Now.AddDays(3), ShiftStatus.VACATION);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { existingShift });

            var thrownException = Assert.Throws<InvalidOperationException>(() =>
                this.service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(4)));
            Assert.That(thrownException.Message, Does.Contain("vacation"));
        }

        [Test]
        public void RegisterVacation_ValidInput_AddsVacationShiftWithCorrectProperties()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5);
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            // Setting up an existing shift to prove ID increments correctly (Max ID + 1)
            var existingShift = new Shift(5, pharmacist, "Pharmacy", DateTime.Now.AddDays(20), DateTime.Now.AddDays(21), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { existingShift });

            this.service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3));

            this.mockShiftRepository.Verify(
                repository => repository.AddShift(It.Is<Shift>(service =>
                service.Status == ShiftStatus.VACATION &&
                service.Location == "Vacation" &&
                service.Id == 6 &&
                service.AppointedStaff.StaffID == 1)), Times.Once);
        }
    }
}