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
    public class ShiftManagementServiceTests
    {
        private Mock<IShiftManagementStaffRepository> mockStaffRepository;
        private Mock<IShiftManagementShiftRepository> mockShiftRepository;
        private ShiftManagementService service;
        private Doctor doctor1;
        private Doctor doctor2;
        private Pharmacyst pharmacist1;

        [SetUp]
        public void Setup()
        {
            this.mockStaffRepository = new Mock<IShiftManagementStaffRepository>();
            this.mockShiftRepository = new Mock<IShiftManagementShiftRepository>();
            this.service = new ShiftManagementService(this.mockStaffRepository.Object, this.mockShiftRepository.Object);

            this.doctor1 = new Doctor(1, "John", "Doe", "contact", true, "Cardiology", "LIC1", DoctorStatus.AVAILABLE, 5);
            this.doctor2 = new Doctor(2, "Jane", "Smith", "contact", true, "Surgery", "LIC2", DoctorStatus.AVAILABLE, 3);
            this.pharmacist1 = new Pharmacyst(3, "Bob", "Brown", "contact", true, "CertA", 2);
        }

        [Test]
        public void SetShiftActive_ExistingShift_UpdatesStatus()
        {
            var shift = new Shift(1, this.doctor1, "Ward A", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            this.service.SetShiftActive(1);
            this.mockShiftRepository.Verify(repository => repository.UpdateShiftStatus(1, ShiftStatus.ACTIVE), Times.Once);
            this.mockStaffRepository.Verify(repository => repository.UpdateStaffAvailability(1, true, DoctorStatus.AVAILABLE), Times.Once);
        }

        [Test]
        public void SetShiftActive_NonExistingShift_DoesNothing()
        {
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            this.service.SetShiftActive(99);
            this.mockShiftRepository.Verify(repository => repository.UpdateShiftStatus(It.IsAny<int>(), It.IsAny<ShiftStatus>()), Times.Never);
        }

        [Test]
        public void CancelShift_ExistingActiveShift_CancelsAndUpdatesAvailability()
        {
            var shift = new Shift(1, this.doctor1, "Ward A", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.ACTIVE);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            this.service.CancelShift(1);
            this.mockShiftRepository.Verify(repository => repository.UpdateShiftStatus(1, ShiftStatus.CANCELLED), Times.Once);
            this.mockStaffRepository.Verify(repository => repository.UpdateStaffAvailability(1, false, DoctorStatus.OFF_DUTY), Times.Once);
        }

        [Test]
        public void CancelShift_ExistingScheduledShift_CancelsNoAvailabilityUpdate()
        {
            var shift = new Shift(1, this.doctor1, "Ward A", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            this.service.CancelShift(1);
            this.mockShiftRepository.Verify(repository => repository.UpdateShiftStatus(1, ShiftStatus.CANCELLED), Times.Once);
            this.mockStaffRepository.Verify(repository => repository.UpdateStaffAvailability(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<DoctorStatus>()), Times.Never);
        }

        [Test]
        public void CancelShift_NonExistingShift_DoesNothing()
        {
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            this.service.CancelShift(99);
            this.mockShiftRepository.Verify(repository => repository.UpdateShiftStatus(It.IsAny<int>(), It.IsAny<ShiftStatus>()), Times.Never);
        }

        [Test]
        public void ValidateNoOverlap_NoShifts_ReturnsTrue()
        {
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            Assert.That(this.service.ValidateNoOverlap(1, DateTime.Now, DateTime.Now.AddHours(8)), Is.True);
        }

        [Test]
        public void ValidateNoOverlap_OverlappingShift_ReturnsFalse()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, this.doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            Assert.That(this.service.ValidateNoOverlap(1, now.AddHours(4), now.AddHours(12)), Is.False);
        }

        [Test]
        public void ValidateNoOverlap_CancelledShift_ReturnsTrue()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, this.doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.CANCELLED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            Assert.That(this.service.ValidateNoOverlap(1, now.AddHours(4), now.AddHours(12)), Is.True);
        }

        [Test]
        public void ValidateNoOverlap_CompletedShift_ReturnsTrue()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, this.doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.COMPLETED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            Assert.That(this.service.ValidateNoOverlap(1, now.AddHours(4), now.AddHours(12)), Is.True);
        }

        [Test]
        public void ValidateNoOverlap_DifferentStaff_ReturnsTrue()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, this.doctor2, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            Assert.That(this.service.ValidateNoOverlap(1, now.AddHours(4), now.AddHours(12)), Is.True);
        }

        [Test]
        public void TryAddShift_NoOverlap_AddsAndReturnsTrue()
        {
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            var now = DateTime.Now;
            var result = this.service.TryAddShift(this.doctor1, now, now.AddHours(8), "Ward A");
            Assert.That(result, Is.True);
            this.mockShiftRepository.Verify(repository => repository.AddShift(It.IsAny<Shift>()), Times.Once);
        }

        [Test]
        public void TryAddShift_Overlap_ReturnsFalse()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, this.doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = this.service.TryAddShift(this.doctor1, now.AddHours(4), now.AddHours(12), "Ward B");
            Assert.That(result, Is.False);
            this.mockShiftRepository.Verify(repository => repository.AddShift(It.IsAny<Shift>()), Times.Never);
        }

        [Test]
        public void ValidateShiftTimes_EndAfterStart_ReturnsTrue()
        {
            Assert.That(this.service.ValidateShiftTimes(TimeSpan.FromHours(8), TimeSpan.FromHours(16)), Is.True);
        }

        [Test]
        public void ValidateShiftTimes_EndBeforeStart_ReturnsFalse()
        {
            Assert.That(this.service.ValidateShiftTimes(TimeSpan.FromHours(16), TimeSpan.FromHours(8)), Is.False);
        }

        [Test]
        public void ValidateShiftTimes_SameStartAndEnd_ReturnsFalse()
        {
            Assert.That(this.service.ValidateShiftTimes(TimeSpan.FromHours(8), TimeSpan.FromHours(8)), Is.False);
        }

        [Test]
        public void ReassignShift_NullShift_ReturnsFalse()
        {
            Assert.That(this.service.ReassignShift(null, this.doctor2), Is.False);
        }

        [Test]
        public void ReassignShift_NullNewStaff_ReturnsFalse()
        {
            var shift = new Shift(1, this.doctor1, "Ward A", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.SCHEDULED);
            Assert.That(this.service.ReassignShift(shift, null), Is.False);
        }

        [Test]
        public void ReassignShift_DifferentType_ReturnsFalse()
        {
            var shift = new Shift(1, this.doctor1, "Ward A", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.SCHEDULED);
            Assert.That(this.service.ReassignShift(shift, this.pharmacist1), Is.False);
        }

        [Test]
        public void ReassignShift_SameStaff_ReturnsFalse()
        {
            var shift = new Shift(1, this.doctor1, "Ward A", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.SCHEDULED);
            Assert.That(this.service.ReassignShift(shift, this.doctor1), Is.False);
        }

        [Test]
        public void ReassignShift_OverlapForNewStaff_ReturnsFalse()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, this.doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            var existingShift = new Shift(2, this.doctor2, "Ward B", now.AddHours(4), now.AddHours(12), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift, existingShift });

            Assert.That(this.service.ReassignShift(shift, this.doctor2), Is.False);
        }

        [Test]
        public void ReassignShift_Valid_ReturnsTrue()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, this.doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = this.service.ReassignShift(shift, this.doctor2);
            Assert.That(result, Is.True);
            this.mockShiftRepository.Verify(repository => repository.UpdateShiftStaffId(1, 2), Times.Once);
        }

        [Test]
        public void GetDailyShifts_ReturnsShiftsForDate()
        {
            var today = DateTime.Today;
            var shift1 = new Shift(1, this.doctor1, "Ward A", today.AddHours(8), today.AddHours(16), ShiftStatus.SCHEDULED);
            var shift2 = new Shift(2, this.doctor2, "Ward B", today.AddDays(1).AddHours(8), today.AddDays(1).AddHours(16), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift1, shift2 });
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1, this.doctor2 });

            var result = this.service.GetDailyShifts(today);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(1));
        }

        [Test]
        public void GetWeeklyShifts_ReturnsShiftsForWeek()
        {
            var monday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            var shift1 = new Shift(1, this.doctor1, "Ward A", monday.AddHours(8), monday.AddHours(16), ShiftStatus.SCHEDULED);
            var shift2 = new Shift(2, this.doctor2, "Ward B", monday.AddDays(10).AddHours(8), monday.AddDays(10).AddHours(16), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift1, shift2 });
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1, this.doctor2 });

            var result = this.service.GetWeeklyShifts(monday);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetActiveShifts_ReturnsOnlyActive()
        {
            var now = DateTime.Now;
            var shift1 = new Shift(1, this.doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.ACTIVE);
            var shift2 = new Shift(2, this.doctor2, "Ward B", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift1, shift2 });
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1, this.doctor2 });

            var result = this.service.GetActiveShifts();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Status, Is.EqualTo(ShiftStatus.ACTIVE));
        }

        [Test]
        public void IsStaffWorkingDuring_Working_ReturnsTrue()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, this.doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            Assert.That(this.service.IsStaffWorkingDuring(1, now.AddHours(1), now.AddHours(2)), Is.True);
        }

        [Test]
        public void IsStaffWorkingDuring_NotWorking_ReturnsFalse()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, this.doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            Assert.That(this.service.IsStaffWorkingDuring(1, now.AddHours(10), now.AddHours(12)), Is.False);
        }

        [Test]
        public void IsStaffWorkingDuring_DifferentStaff_ReturnsFalse()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, this.doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            Assert.That(this.service.IsStaffWorkingDuring(2, now.AddHours(1), now.AddHours(2)), Is.False);
        }

        [Test]
        public void FindStaffReplacements_ReturnsOnlySameType()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, this.doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1, this.doctor2, this.pharmacist1 });

            var result = this.service.FindStaffReplacements(shift);
            Assert.That(result.All(staff => staff is Doctor), Is.True);
            Assert.That(result.All(service => service.StaffID != 1), Is.True);
        }

        [Test]
        public void GetSpecializationsAndCertificationsForLocation_Pharmacy_ReturnsCerts()
        {
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1, this.pharmacist1 });
            var result = this.service.GetSpecializationsAndCertificationsForLocation("Pharmacy");
            Assert.That(result.Contains("CertA"), Is.True);
        }

        [Test]
        public void GetSpecializationsAndCertificationsForLocation_Hospital_ReturnsSpecializations()
        {
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1, this.doctor2, this.pharmacist1 });
            var result = this.service.GetSpecializationsAndCertificationsForLocation("Hospital");
            Assert.That(result.Contains("Cardiology"), Is.True);
            Assert.That(result.Contains("Surgery"), Is.True);
        }
    }
}
