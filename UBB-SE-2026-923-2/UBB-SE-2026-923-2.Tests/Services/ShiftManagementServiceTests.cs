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
            mockStaffRepository = new Mock<IShiftManagementStaffRepository>();
            mockShiftRepository = new Mock<IShiftManagementShiftRepository>();
            service = new ShiftManagementService(mockStaffRepository.Object, mockShiftRepository.Object);

            doctor1 = new Doctor(1, "John", "Doe", "contact", true, "Cardiology", "LIC1", DoctorStatus.AVAILABLE, 5);
            doctor2 = new Doctor(2, "Jane", "Smith", "contact", true, "Surgery", "LIC2", DoctorStatus.AVAILABLE, 3);
            pharmacist1 = new Pharmacyst(3, "Bob", "Brown", "contact", true, "CertA", 2);
        }

        [Test]
        public void SetShiftActive_ExistingShift_UpdatesStatus()
        {
            var shift = new Shift(1, doctor1, "Ward A", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            service.SetShiftActive(1);
            mockShiftRepository.Verify(r => r.UpdateShiftStatus(1, ShiftStatus.ACTIVE), Times.Once);
            mockStaffRepository.Verify(r => r.UpdateStaffAvailability(1, true, DoctorStatus.AVAILABLE), Times.Once);
        }

        [Test]
        public void SetShiftActive_NonExistingShift_DoesNothing()
        {
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift>());
            service.SetShiftActive(99);
            mockShiftRepository.Verify(r => r.UpdateShiftStatus(It.IsAny<int>(), It.IsAny<ShiftStatus>()), Times.Never);
        }

        [Test]
        public void CancelShift_ExistingActiveShift_CancelsAndUpdatesAvailability()
        {
            var shift = new Shift(1, doctor1, "Ward A", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.ACTIVE);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            service.CancelShift(1);
            mockShiftRepository.Verify(r => r.UpdateShiftStatus(1, ShiftStatus.CANCELLED), Times.Once);
            mockStaffRepository.Verify(r => r.UpdateStaffAvailability(1, false, DoctorStatus.OFF_DUTY), Times.Once);
        }

        [Test]
        public void CancelShift_ExistingScheduledShift_CancelsNoAvailabilityUpdate()
        {
            var shift = new Shift(1, doctor1, "Ward A", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            service.CancelShift(1);
            mockShiftRepository.Verify(r => r.UpdateShiftStatus(1, ShiftStatus.CANCELLED), Times.Once);
            mockStaffRepository.Verify(r => r.UpdateStaffAvailability(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<DoctorStatus>()), Times.Never);
        }

        [Test]
        public void CancelShift_NonExistingShift_DoesNothing()
        {
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift>());
            service.CancelShift(99);
            mockShiftRepository.Verify(r => r.UpdateShiftStatus(It.IsAny<int>(), It.IsAny<ShiftStatus>()), Times.Never);
        }

        [Test]
        public void ValidateNoOverlap_NoShifts_ReturnsTrue()
        {
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift>());
            Assert.That(service.ValidateNoOverlap(1, DateTime.Now, DateTime.Now.AddHours(8)), Is.True);
        }

        [Test]
        public void ValidateNoOverlap_OverlappingShift_ReturnsFalse()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            Assert.That(service.ValidateNoOverlap(1, now.AddHours(4), now.AddHours(12)), Is.False);
        }

        [Test]
        public void ValidateNoOverlap_CancelledShift_ReturnsTrue()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.CANCELLED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            Assert.That(service.ValidateNoOverlap(1, now.AddHours(4), now.AddHours(12)), Is.True);
        }

        [Test]
        public void ValidateNoOverlap_CompletedShift_ReturnsTrue()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.COMPLETED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            Assert.That(service.ValidateNoOverlap(1, now.AddHours(4), now.AddHours(12)), Is.True);
        }

        [Test]
        public void ValidateNoOverlap_DifferentStaff_ReturnsTrue()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, doctor2, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            Assert.That(service.ValidateNoOverlap(1, now.AddHours(4), now.AddHours(12)), Is.True);
        }

        [Test]
        public void TryAddShift_NoOverlap_AddsAndReturnsTrue()
        {
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift>());
            var now = DateTime.Now;
            var result = service.TryAddShift(doctor1, now, now.AddHours(8), "Ward A");
            Assert.That(result, Is.True);
            mockShiftRepository.Verify(r => r.AddShift(It.IsAny<Shift>()), Times.Once);
        }

        [Test]
        public void TryAddShift_Overlap_ReturnsFalse()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = service.TryAddShift(doctor1, now.AddHours(4), now.AddHours(12), "Ward B");
            Assert.That(result, Is.False);
            mockShiftRepository.Verify(r => r.AddShift(It.IsAny<Shift>()), Times.Never);
        }

        [Test]
        public void ValidateShiftTimes_EndAfterStart_ReturnsTrue()
        {
            Assert.That(service.ValidateShiftTimes(TimeSpan.FromHours(8), TimeSpan.FromHours(16)), Is.True);
        }

        [Test]
        public void ValidateShiftTimes_EndBeforeStart_ReturnsFalse()
        {
            Assert.That(service.ValidateShiftTimes(TimeSpan.FromHours(16), TimeSpan.FromHours(8)), Is.False);
        }

        [Test]
        public void ValidateShiftTimes_SameStartAndEnd_ReturnsFalse()
        {
            Assert.That(service.ValidateShiftTimes(TimeSpan.FromHours(8), TimeSpan.FromHours(8)), Is.False);
        }

        [Test]
        public void ReassignShift_NullShift_ReturnsFalse()
        {
            Assert.That(service.ReassignShift(null, doctor2), Is.False);
        }

        [Test]
        public void ReassignShift_NullNewStaff_ReturnsFalse()
        {
            var shift = new Shift(1, doctor1, "Ward A", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.SCHEDULED);
            Assert.That(service.ReassignShift(shift, null), Is.False);
        }

        [Test]
        public void ReassignShift_DifferentType_ReturnsFalse()
        {
            var shift = new Shift(1, doctor1, "Ward A", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.SCHEDULED);
            Assert.That(service.ReassignShift(shift, pharmacist1), Is.False);
        }

        [Test]
        public void ReassignShift_SameStaff_ReturnsFalse()
        {
            var shift = new Shift(1, doctor1, "Ward A", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.SCHEDULED);
            Assert.That(service.ReassignShift(shift, doctor1), Is.False);
        }

        [Test]
        public void ReassignShift_OverlapForNewStaff_ReturnsFalse()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            var existingShift = new Shift(2, doctor2, "Ward B", now.AddHours(4), now.AddHours(12), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift, existingShift });

            Assert.That(service.ReassignShift(shift, doctor2), Is.False);
        }

        [Test]
        public void ReassignShift_Valid_ReturnsTrue()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = service.ReassignShift(shift, doctor2);
            Assert.That(result, Is.True);
            mockShiftRepository.Verify(r => r.UpdateShiftStaffId(1, 2), Times.Once);
        }

        [Test]
        public void GetDailyShifts_ReturnsShiftsForDate()
        {
            var today = DateTime.Today;
            var shift1 = new Shift(1, doctor1, "Ward A", today.AddHours(8), today.AddHours(16), ShiftStatus.SCHEDULED);
            var shift2 = new Shift(2, doctor2, "Ward B", today.AddDays(1).AddHours(8), today.AddDays(1).AddHours(16), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift1, shift2 });
            mockStaffRepository.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { doctor1, doctor2 });

            var result = service.GetDailyShifts(today);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(1));
        }

        [Test]
        public void GetWeeklyShifts_ReturnsShiftsForWeek()
        {
            var monday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            var shift1 = new Shift(1, doctor1, "Ward A", monday.AddHours(8), monday.AddHours(16), ShiftStatus.SCHEDULED);
            var shift2 = new Shift(2, doctor2, "Ward B", monday.AddDays(10).AddHours(8), monday.AddDays(10).AddHours(16), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift1, shift2 });
            mockStaffRepository.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { doctor1, doctor2 });

            var result = service.GetWeeklyShifts(monday);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetActiveShifts_ReturnsOnlyActive()
        {
            var now = DateTime.Now;
            var shift1 = new Shift(1, doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.ACTIVE);
            var shift2 = new Shift(2, doctor2, "Ward B", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift1, shift2 });
            mockStaffRepository.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { doctor1, doctor2 });

            var result = service.GetActiveShifts();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Status, Is.EqualTo(ShiftStatus.ACTIVE));
        }

        [Test]
        public void IsStaffWorkingDuring_Working_ReturnsTrue()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            Assert.That(service.IsStaffWorkingDuring(1, now.AddHours(1), now.AddHours(2)), Is.True);
        }

        [Test]
        public void IsStaffWorkingDuring_NotWorking_ReturnsFalse()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            Assert.That(service.IsStaffWorkingDuring(1, now.AddHours(10), now.AddHours(12)), Is.False);
        }

        [Test]
        public void IsStaffWorkingDuring_DifferentStaff_ReturnsFalse()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            Assert.That(service.IsStaffWorkingDuring(2, now.AddHours(1), now.AddHours(2)), Is.False);
        }

        [Test]
        public void FindStaffReplacements_ReturnsOnlySameType()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });
            mockStaffRepository.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { doctor1, doctor2, pharmacist1 });

            var result = service.FindStaffReplacements(shift);
            Assert.That(result.All(s => s is Doctor), Is.True);
            Assert.That(result.All(s => s.StaffID != 1), Is.True);
        }

        [Test]
        public void GetSpecializationsAndCertificationsForLocation_Pharmacy_ReturnsCerts()
        {
            mockStaffRepository.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { doctor1, pharmacist1 });
            var result = service.GetSpecializationsAndCertificationsForLocation("Pharmacy");
            Assert.That(result.Contains("CertA"), Is.True);
        }

        [Test]
        public void GetSpecializationsAndCertificationsForLocation_Hospital_ReturnsSpecializations()
        {
            mockStaffRepository.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { doctor1, doctor2, pharmacist1 });
            var result = service.GetSpecializationsAndCertificationsForLocation("Hospital");
            Assert.That(result.Contains("Cardiology"), Is.True);
            Assert.That(result.Contains("Surgery"), Is.True);
        }
    }
}
