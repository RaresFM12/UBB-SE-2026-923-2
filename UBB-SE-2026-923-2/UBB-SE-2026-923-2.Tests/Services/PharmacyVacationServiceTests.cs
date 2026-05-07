using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.Tests.Services
{
    [TestFixture]
    public class PharmacyVacationServiceTests
    {
        private Mock<IPharmacyStaffRepository> mockStaffRepo;
        private Mock<IPharmacyShiftRepository> mockShiftRepo;
        private PharmacyVacationService service;

        [SetUp]
        public void Setup()
        {
            mockStaffRepo = new Mock<IPharmacyStaffRepository>();
            mockShiftRepo = new Mock<IPharmacyShiftRepository>();
            service = new PharmacyVacationService(mockStaffRepo.Object, mockShiftRepo.Object);
        }

        [Test]
        public void Constructor_NullStaffRepo_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new PharmacyVacationService(null, mockShiftRepo.Object));
        }

        [Test]
        public void Constructor_NullShiftRepo_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new PharmacyVacationService(mockStaffRepo.Object, null));
        }

        [Test]
        public void GetPharmacists_ReturnsOrderedByName()
        {
            var pharmacists = new List<Pharmacyst>
            {
                new Pharmacyst(1, "Zoe", "Adams", "", true, "cert", 5),
                new Pharmacyst(2, "Alice", "Brown", "", true, "cert2", 3),
                new Pharmacyst(3, "Alice", "Adams", "", true, "cert3", 2),
            };
            mockStaffRepo.Setup(r => r.GetPharmacists()).Returns(pharmacists);
            var result = service.GetPharmacists();
            Assert.That(result[0].FirstName, Is.EqualTo("Alice"));
            Assert.That(result[0].LastName, Is.EqualTo("Adams"));
            Assert.That(result[1].FirstName, Is.EqualTo("Alice"));
            Assert.That(result[1].LastName, Is.EqualTo("Brown"));
            Assert.That(result[2].FirstName, Is.EqualTo("Zoe"));
        }

        [Test]
        public void GetPharmacists_Empty_ReturnsEmpty()
        {
            mockStaffRepo.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst>());
            var result = service.GetPharmacists();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void RegisterVacation_EndBeforeStart_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                service.RegisterVacation(1, DateTime.Now.AddDays(5), DateTime.Now.AddDays(2)));
        }

        [Test]
        public void RegisterVacation_PharmacistNotFound_Throws()
        {
            mockStaffRepo.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst>());
            Assert.Throws<ArgumentException>(() =>
                service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3)));
        }

        [Test]
        public void RegisterVacation_OverlapsExistingShift_Throws()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            mockStaffRepo.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            var existingShift = new Shift(1, pharmacist, "Pharmacy", DateTime.Now.AddDays(2), DateTime.Now.AddDays(3), ShiftStatus.SCHEDULED);
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { existingShift });

            Assert.Throws<InvalidOperationException>(() =>
                service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(4)));
        }

        [Test]
        public void RegisterVacation_OverlapsExistingVacation_ThrowsWithVacationMessage()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            mockStaffRepo.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            var existingShift = new Shift(1, pharmacist, "Vacation", DateTime.Now.AddDays(2), DateTime.Now.AddDays(3), ShiftStatus.VACATION);
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { existingShift });

            var ex = Assert.Throws<InvalidOperationException>(() =>
                service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(4)));
            Assert.That(ex.Message, Does.Contain("vacation"));
        }

        [Test]
        public void RegisterVacation_NoOverlap_AddsShift()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            mockStaffRepo.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift>());

            service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3));
            mockShiftRepo.Verify(r => r.AddShift(It.Is<Shift>(s =>
                s.Status == ShiftStatus.VACATION &&
                s.AppointedStaff.StaffID == 1)), Times.Once);
        }

        [Test]
        public void RegisterVacation_SameDay_NoOverlap_Works()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            mockStaffRepo.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift>());

            var date = DateTime.Now.AddDays(10).Date;
            Assert.DoesNotThrow(() => service.RegisterVacation(1, date, date));
        }

        [Test]
        public void RegisterVacation_NonOverlappingShift_Works()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            mockStaffRepo.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            var existingShift = new Shift(1, pharmacist, "Pharmacy", DateTime.Now.AddDays(10), DateTime.Now.AddDays(11), ShiftStatus.SCHEDULED);
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { existingShift });

            Assert.DoesNotThrow(() => service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3)));
        }
    }
}
