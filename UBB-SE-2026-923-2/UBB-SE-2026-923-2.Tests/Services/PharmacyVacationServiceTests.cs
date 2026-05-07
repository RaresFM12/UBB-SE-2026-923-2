namespace UBB_SE_2026_923_2.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        [Test]
        public void GetPharmacists_ReturnsOrderedByName()
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
            Assert.That(result[0].LastName, Is.EqualTo("Adams"));
            Assert.That(result[1].FirstName, Is.EqualTo("Alice"));
            Assert.That(result[1].LastName, Is.EqualTo("Brown"));
            Assert.That(result[2].FirstName, Is.EqualTo("Zoe"));
        }

        [Test]
        public void GetPharmacists_Empty_ReturnsEmpty()
        {
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst>());
            var result = this.service.GetPharmacists();
            Assert.That(result.Count, Is.EqualTo(0));
        }

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
        public void RegisterVacation_NoOverlap_AddsShift()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5);
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());

            this.service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3));
            this.mockShiftRepository.Verify(
                repository => repository.AddShift(It.Is<Shift>(s =>
                s.Status == ShiftStatus.VACATION &&
                s.AppointedStaff.StaffID == 1)), Times.Once);
        }

        [Test]
        public void RegisterVacation_SameDay_NoOverlap_Works()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5);
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());

            var date = DateTime.Now.AddDays(10).Date;
            Assert.DoesNotThrow(() => this.service.RegisterVacation(1, date, date));
        }

        [Test]
        public void RegisterVacation_NonOverlappingShift_Works()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5);
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            var existingShift = new Shift(1, pharmacist, "Pharmacy", DateTime.Now.AddDays(10), DateTime.Now.AddDays(11), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { existingShift });

            Assert.DoesNotThrow(() => this.service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3)));
        }

        [Test]
        public void GetPharmacists_SinglePharmacist_ReturnsOne()
        {
            var pharmacists = new List<Pharmacyst> { new Pharmacyst(1, "John", "Doe", string.Empty, true, "cert", 5) };
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(pharmacists);
            var result = this.service.GetPharmacists();
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetPharmacists_ManyPharmacists_AllReturned()
        {
            var pharmacists = new List<Pharmacyst>();
            for (int pharmacistIndex = 1; pharmacistIndex <= 10; pharmacistIndex++)
            {
                pharmacists.Add(new Pharmacyst(pharmacistIndex, $"First{pharmacistIndex}", $"Last{pharmacistIndex}", string.Empty, true, $"cert{pharmacistIndex}", pharmacistIndex));
            }

            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(pharmacists);
            var result = this.service.GetPharmacists();
            Assert.That(result.Count, Is.EqualTo(10));
        }

        [Test]
        public void GetPharmacists_SameFirstName_OrderedByLastName()
        {
            var pharmacists = new List<Pharmacyst>
            {
                new Pharmacyst(1, "Anna", "Zeta", string.Empty, true, "c1", 1),
                new Pharmacyst(2, "Anna", "Alpha", string.Empty, true, "c2", 2),
            };
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(pharmacists);
            var result = this.service.GetPharmacists();
            Assert.That(result[0].LastName, Is.EqualTo("Alpha"));
            Assert.That(result[1].LastName, Is.EqualTo("Zeta"));
        }

        [Test]
        public void RegisterVacation_ValidInput_ShiftHasVacationStatus()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5);
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());

            this.service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3));
            this.mockShiftRepository.Verify(repository => repository.AddShift(It.Is<Shift>(service => service.Status == ShiftStatus.VACATION)), Times.Once);
        }

        [Test]
        public void RegisterVacation_ValidInput_ShiftLocationIsVacation()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5);
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());

            this.service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3));
            this.mockShiftRepository.Verify(repository => repository.AddShift(It.Is<Shift>(service => service.Location == "Vacation")), Times.Once);
        }

        [Test]
        public void RegisterVacation_ExistingShifts_NextIdIsMaxPlusOne()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5);
            var otherStaff = new Pharmacyst(2, "C", "D", string.Empty, true, "cert2", 3);
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            var existingShift = new Shift(5, otherStaff, "Pharmacy", DateTime.Now.AddDays(20), DateTime.Now.AddDays(21), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { existingShift });

            this.service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3));
            this.mockShiftRepository.Verify(repository => repository.AddShift(It.Is<Shift>(service => service.Id == 6)), Times.Once);
        }

        [Test]
        public void RegisterVacation_NoExistingShifts_IdIsOne()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5);
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());

            this.service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(3));
            this.mockShiftRepository.Verify(repository => repository.AddShift(It.Is<Shift>(service => service.Id == 1)), Times.Once);
        }

        [Test]
        public void RegisterVacation_EndDateSameAsStart_Works()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5);
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());

            var date = DateTime.Now.AddDays(5).Date;
            Assert.DoesNotThrow(() => this.service.RegisterVacation(1, date, date));
            this.mockShiftRepository.Verify(repository => repository.AddShift(It.IsAny<Shift>()), Times.Once);
        }

        [Test]
        public void RegisterVacation_OverlapsExistingShift_ThrowsWithShiftMessage()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5);
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            var existingShift = new Shift(1, pharmacist, "Pharmacy", DateTime.Now.AddDays(2), DateTime.Now.AddDays(3), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { existingShift });

            var thrownException = Assert.Throws<InvalidOperationException>(() =>
                this.service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(4)));
            Assert.That(thrownException.Message, Does.Contain("shift"));
        }

        [Test]
        public void RegisterVacation_AdjacentShiftBefore_NoOverlap_Works()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5);
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            var startDate = DateTime.Now.AddDays(5).Date;
            var existingShift = new Shift(1, pharmacist, "Pharmacy", startDate.AddDays(-2), startDate, ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { existingShift });

            Assert.DoesNotThrow(() => this.service.RegisterVacation(1, startDate, startDate.AddDays(2)));
        }

        [Test]
        public void RegisterVacation_AdjacentShiftAfter_NoOverlap_Works()
        {
            var pharmacist = new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5);
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });

            var startDate = DateTime.Now.AddDays(5).Date;
            var endDate = startDate.AddDays(2);
            var existingShift = new Shift(1, pharmacist, "Pharmacy", endDate.AddDays(1), endDate.AddDays(3), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { existingShift });

            Assert.DoesNotThrow(() => this.service.RegisterVacation(1, startDate, endDate));
        }

        [Test]
        public void RegisterVacation_MultiplePharmacists_OnlyChecksCorrectOne()
        {
            var pharmacist1 = new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5);
            var pharmacist2 = new Pharmacyst(2, "C", "D", string.Empty, true, "cert2", 3);
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist1, pharmacist2 });

            var existingShift = new Shift(1, pharmacist2, "Pharmacy", DateTime.Now.AddDays(2), DateTime.Now.AddDays(3), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { existingShift });

            Assert.DoesNotThrow(() => this.service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(4)));
        }

        [Test]
        public void Constructor_BothValid_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new PharmacyVacationService(this.mockStaffRepository.Object, this.mockShiftRepository.Object));
        }

        [Test]
        public void GetPharmacists_ReturnsReadOnlyList()
        {
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst>());
            var result = this.service.GetPharmacists();
            Assert.That(result, Is.InstanceOf<IReadOnlyList<Pharmacyst>>());
        }

        [Test]
        public void RegisterVacation_ShiftForDifferentPharmacist_DoesNotBlock()
        {
            var pharmacist1 = new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5);
            var pharmacist2 = new Pharmacyst(2, "C", "D", string.Empty, true, "cert2", 3);
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist1, pharmacist2 });

            var existingShift = new Shift(1, pharmacist2, "Pharmacy", DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { existingShift });

            Assert.DoesNotThrow(() => this.service.RegisterVacation(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5)));
        }
    }
}
