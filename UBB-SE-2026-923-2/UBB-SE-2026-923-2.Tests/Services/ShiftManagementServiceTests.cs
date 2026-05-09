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

        // --- Status Updates ---
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

        // --- Overlap & Validation ---
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
        public void ValidateShiftTimes_EndBeforeStart_ReturnsFalse()
        {
            Assert.That(this.service.ValidateShiftTimes(TimeSpan.FromHours(16), TimeSpan.FromHours(8)), Is.False);
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

        // --- Reassignment ---
        [Test]
        public void ReassignShift_DifferentType_ReturnsFalse()
        {
            var shift = new Shift(1, this.doctor1, "Ward A", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.SCHEDULED);
            Assert.That(this.service.ReassignShift(shift, this.pharmacist1), Is.False);
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

        // --- Queries & Filters ---
        [Test]
        public void ShiftQueries_ReturnCorrectShifts()
        {
            var now = DateTime.Now;
            var shift1 = new Shift(1, this.doctor1, "Ward A", now, now.AddHours(8), ShiftStatus.ACTIVE);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift1 });
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1 });

            Assert.That(this.service.GetDailyShifts(now).Count, Is.EqualTo(1));
            Assert.That(this.service.GetWeeklyShifts(now).Count, Is.EqualTo(1));
            Assert.That(this.service.GetActiveShifts().Count, Is.EqualTo(1));
            Assert.That(this.service.IsStaffWorkingDuring(1, now.AddHours(1), now.AddHours(2)), Is.True);
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
        public void GetSpecializationsAndCertifications_ReturnsCorrectLabelsForLocation()
        {
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1, this.doctor2, this.pharmacist1 });

            var pharmacyResult = this.service.GetSpecializationsAndCertificationsForLocation("Pharmacy");
            var hospitalResult = this.service.GetSpecializationsAndCertificationsForLocation("Hospital");

            Assert.That(pharmacyResult.Contains("CertA"), Is.True);
            Assert.That(hospitalResult.Contains("Cardiology"), Is.True);
            Assert.That(hospitalResult.Contains("Surgery"), Is.True);
        }

        // --- GetWeeklyHours ---
        [Test]
        public void GetWeeklyHours_ShiftsThisWeek_ReturnsTotalHours()
        {
            var now = DateTime.Now;
            int daysFromMonday = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
            var currentWeekMonday = now.Date.AddDays(-daysFromMonday);
            var shiftThisWeek = new Shift(1, this.doctor1, "Ward A", currentWeekMonday.AddHours(8), currentWeekMonday.AddHours(16), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shiftThisWeek });

            var result = this.service.GetWeeklyHours(1);

            Assert.That(result, Is.EqualTo(8f));
        }

        [Test]
        public void GetWeeklyHours_NoShiftsThisWeek_ReturnsZero()
        {
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());

            var result = this.service.GetWeeklyHours(1);

            Assert.That(result, Is.EqualTo(0f));
        }

        // --- AddShift ---
        [Test]
        public void AddShift_ValidShift_CallsRepository()
        {
            var newShift = new Shift(0, this.doctor1, "Ward A", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.SCHEDULED);

            this.service.AddShift(newShift);

            this.mockShiftRepository.Verify(repository => repository.AddShift(newShift), Times.Once);
        }

        [Test]
        public void AddShift_AnyShift_DelegatesToRepository()
        {
            var anotherShift = new Shift(0, this.pharmacist1, "Pharmacy", DateTime.Now, DateTime.Now.AddHours(6), ShiftStatus.SCHEDULED);

            this.service.AddShift(anotherShift);

            this.mockShiftRepository.Verify(repository => repository.AddShift(anotherShift), Times.Once);
        }

        // --- GetFilteredStaff ---
        [Test]
        public void GetFilteredStaff_PharmacyLocation_ReturnsPharmacistsWithCertification()
        {
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1, this.pharmacist1 });

            var result = this.service.GetFilteredStaff("Pharmacy", "CertA");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.InstanceOf<Pharmacyst>());
        }

        [Test]
        public void GetFilteredStaff_HospitalLocation_ReturnsDoctorsWithSpecialization()
        {
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1, this.doctor2, this.pharmacist1 });

            var result = this.service.GetFilteredStaff("Hospital", "Cardiology");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].StaffID, Is.EqualTo(1));
        }
    }
}