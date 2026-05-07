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
        private Mock<IPharmacyStaffRepository> mockStaffRepository;
        private Mock<IPharmacyShiftRepository> mockShiftRepository;
        private PharmacyVacationService service;

        [SetUp]
        public void Setup()
        {
            mockStaffRepository = new Mock<IPharmacyStaffRepository>();
            mockShiftRepository = new Mock<IPharmacyShiftRepository>();
            service = new PharmacyVacationService(mockStaffRepository.Object, mockShiftRepository.Object);
        }

        [Test]
        public void Constructor_NullStaffRepo_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new PharmacyVacationService(null, mockShiftRepository.Object));
        }

        [Test]
        public void Constructor_NullShiftRepo_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new PharmacyVacationService(mockStaffRepository.Object, null));
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
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(pharmacists);
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
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst>());
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
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst>());
            Assert.Throws<ArgumentException>(() =>
                service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3)));
        }

        [Test]
        public void RegisterVacation_OverlapsExistingShift_Throws()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            var existingShift = new Shift(1, pharmacist, "Pharmacy", DateTime.Now.AddDays(2), DateTime.Now.AddDays(3), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { existingShift });

            Assert.Throws<InvalidOperationException>(() =>
                service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(4)));
        }

        [Test]
        public void RegisterVacation_OverlapsExistingVacation_ThrowsWithVacationMessage()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            var existingShift = new Shift(1, pharmacist, "Vacation", DateTime.Now.AddDays(2), DateTime.Now.AddDays(3), ShiftStatus.VACATION);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { existingShift });

            var ex = Assert.Throws<InvalidOperationException>(() =>
                service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(4)));
            Assert.That(ex.Message, Does.Contain("vacation"));
        }

        [Test]
        public void RegisterVacation_NoOverlap_AddsShift()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift>());

            service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3));
            mockShiftRepository.Verify(r => r.AddShift(It.Is<Shift>(s =>
                s.Status == ShiftStatus.VACATION &&
                s.AppointedStaff.StaffID == 1)), Times.Once);
        }

        [Test]
        public void RegisterVacation_SameDay_NoOverlap_Works()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift>());

            var date = DateTime.Now.AddDays(10).Date;
            Assert.DoesNotThrow(() => service.RegisterVacation(1, date, date));
        }

        [Test]
        public void RegisterVacation_NonOverlappingShift_Works()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            var existingShift = new Shift(1, pharmacist, "Pharmacy", DateTime.Now.AddDays(10), DateTime.Now.AddDays(11), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { existingShift });

            Assert.DoesNotThrow(() => service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3)));
        }

        [Test]
        public void GetPharmacists_SinglePharmacist_ReturnsOne()
        {
            var pharmacists = new List<Pharmacyst> { new Pharmacyst(1, "John", "Doe", "", true, "cert", 5) };
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(pharmacists);
            var result = service.GetPharmacists();
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetPharmacists_ManyPharmacists_AllReturned()
        {
            var pharmacists = new List<Pharmacyst>();
            for (int i = 1; i <= 10; i++)
                pharmacists.Add(new Pharmacyst(i, $"First{i}", $"Last{i}", "", true, $"cert{i}", i));
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(pharmacists);
            var result = service.GetPharmacists();
            Assert.That(result.Count, Is.EqualTo(10));
        }

        [Test]
        public void GetPharmacists_SameFirstName_OrderedByLastName()
        {
            var pharmacists = new List<Pharmacyst>
            {
                new Pharmacyst(1, "Anna", "Zeta", "", true, "c1", 1),
                new Pharmacyst(2, "Anna", "Alpha", "", true, "c2", 2),
            };
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(pharmacists);
            var result = service.GetPharmacists();
            Assert.That(result[0].LastName, Is.EqualTo("Alpha"));
            Assert.That(result[1].LastName, Is.EqualTo("Zeta"));
        }

        [Test]
        public void RegisterVacation_ValidInput_ShiftHasVacationStatus()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift>());

            service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3));
            mockShiftRepository.Verify(r => r.AddShift(It.Is<Shift>(s => s.Status == ShiftStatus.VACATION)), Times.Once);
        }

        [Test]
        public void RegisterVacation_ValidInput_ShiftLocationIsVacation()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift>());

            service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3));
            mockShiftRepository.Verify(r => r.AddShift(It.Is<Shift>(s => s.Location == "Vacation")), Times.Once);
        }

        [Test]
        public void RegisterVacation_ExistingShifts_NextIdIsMaxPlusOne()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            var otherStaff = new Pharmacyst(2, "C", "D", "", true, "cert2", 3);
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            var existingShift = new Shift(5, otherStaff, "Pharmacy", DateTime.Now.AddDays(20), DateTime.Now.AddDays(21), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { existingShift });

            service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3));
            mockShiftRepository.Verify(r => r.AddShift(It.Is<Shift>(s => s.Id == 6)), Times.Once);
        }

        [Test]
        public void RegisterVacation_NoExistingShifts_IdIsOne()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift>());

            service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3));
            mockShiftRepository.Verify(r => r.AddShift(It.Is<Shift>(s => s.Id == 1)), Times.Once);
        }

        [Test]
        public void RegisterVacation_EndDateSameAsStart_Works()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift>());

            var date = DateTime.Now.AddDays(5).Date;
            Assert.DoesNotThrow(() => service.RegisterVacation(1, date, date));
            mockShiftRepository.Verify(r => r.AddShift(It.IsAny<Shift>()), Times.Once);
        }

        [Test]
        public void RegisterVacation_OverlapsExistingShift_ThrowsWithShiftMessage()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            var existingShift = new Shift(1, pharmacist, "Pharmacy", DateTime.Now.AddDays(2), DateTime.Now.AddDays(3), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { existingShift });

            var ex = Assert.Throws<InvalidOperationException>(() =>
                service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(4)));
            Assert.That(ex.Message, Does.Contain("shift"));
        }

        [Test]
        public void RegisterVacation_AdjacentShiftBefore_NoOverlap_Works()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            var startDate = DateTime.Now.AddDays(5).Date;
            var existingShift = new Shift(1, pharmacist, "Pharmacy", startDate.AddDays(-2), startDate, ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { existingShift });

            Assert.DoesNotThrow(() => service.RegisterVacation(1, startDate, startDate.AddDays(2)));
        }

        [Test]
        public void RegisterVacation_AdjacentShiftAfter_NoOverlap_Works()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            var startDate = DateTime.Now.AddDays(5).Date;
            var endDate = startDate.AddDays(2);
            var existingShift = new Shift(1, pharmacist, "Pharmacy", endDate.AddDays(1), endDate.AddDays(3), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { existingShift });

            Assert.DoesNotThrow(() => service.RegisterVacation(1, startDate, endDate));
        }

        [Test]
        public void RegisterVacation_MultiplePharmacists_OnlyChecksCorrectOne()
        {
            var pharmacist1 = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            var pharmacist2 = new Pharmacyst(2, "C", "D", "", true, "cert2", 3);
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist1, pharmacist2 });

            var existingShift = new Shift(1, pharmacist2, "Pharmacy", DateTime.Now.AddDays(2), DateTime.Now.AddDays(3), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { existingShift });

            Assert.DoesNotThrow(() => service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(4)));
        }

        [Test]
        public void Constructor_BothValid_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new PharmacyVacationService(mockStaffRepository.Object, mockShiftRepository.Object));
        }

        [Test]
        public void GetPharmacists_ReturnsReadOnlyList()
        {
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst>());
            var result = service.GetPharmacists();
            Assert.That(result, Is.InstanceOf<IReadOnlyList<Pharmacyst>>());
        }

        [Test]
        public void RegisterVacation_ShiftForDifferentPharmacist_DoesNotBlock()
        {
            var pharmacist1 = new Pharmacyst(1, "A", "B", "", true, "cert", 5);
            var pharmacist2 = new Pharmacyst(2, "C", "D", "", true, "cert2", 3);
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist1, pharmacist2 });

            var existingShift = new Shift(1, pharmacist2, "Pharmacy", DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { existingShift });

            Assert.DoesNotThrow(() => service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5)));
        }
    }
}
