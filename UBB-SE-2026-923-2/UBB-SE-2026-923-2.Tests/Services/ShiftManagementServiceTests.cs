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
    public class ShiftManagementServiceLogicTests
    {
        private Mock<IShiftManagementStaffRepository> mockStaffRepository;
        private Mock<IShiftManagementShiftRepository> mockShiftRepository;
        private ShiftManagementService shiftManagementService;

        [SetUp]
        public void Setup()
        {
            this.mockStaffRepository = new Mock<IShiftManagementStaffRepository>();
            this.mockShiftRepository = new Mock<IShiftManagementShiftRepository>();

            this.shiftManagementService = new ShiftManagementService(
                this.mockStaffRepository.Object,
                this.mockShiftRepository.Object);
        }

        [Test]
        public void SetShiftActive_WhenShiftExists_UpdatesShiftStatusToActive()
        {
            var doctor = CreateDoctor(1, "Cardiology");
            var existingShift = new Shift(5, doctor, "Ward A", DateTime.Today.AddHours(8), DateTime.Today.AddHours(16), ShiftStatus.SCHEDULED);

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift> { existingShift });

            this.shiftManagementService.SetShiftActive(5);

            this.mockShiftRepository.Verify(
                shiftRepository => shiftRepository.UpdateShiftStatus(5, ShiftStatus.ACTIVE),
                Times.Once);
        }

        [Test]
        public void CancelShift_WhenShiftIsActive_UpdatesStaffAvailabilityToOffDuty()
        {
            var doctor = CreateDoctor(1, "Cardiology");
            var existingShift = new Shift(5, doctor, "Ward A", DateTime.Today.AddHours(8), DateTime.Today.AddHours(16), ShiftStatus.ACTIVE);

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift> { existingShift });

            this.shiftManagementService.CancelShift(5);

            this.mockStaffRepository.Verify(
                staffRepository => staffRepository.UpdateStaffAvailability(1, false, DoctorStatus.OFF_DUTY),
                Times.Once);
        }

        [Test]
        public void ValidateNoOverlap_WhenExistingShiftOverlapsRequestedInterval_ReturnsFalse()
        {
            var doctor = CreateDoctor(1, "Cardiology");

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift>
                {
                    new Shift(1, doctor, "Ward A", DateTime.Today.AddHours(8), DateTime.Today.AddHours(16), ShiftStatus.ACTIVE),
                });

            var validationResult = this.shiftManagementService.ValidateNoOverlap(
                1,
                DateTime.Today.AddHours(10),
                DateTime.Today.AddHours(12));

            Assert.That(validationResult, Is.False);
        }

        [Test]
        public void TryAddShift_WhenRequestedShiftDoesNotOverlap_AddsShift()
        {
            var doctor = CreateDoctor(1, "Cardiology");

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift>());

            this.shiftManagementService.TryAddShift(doctor, DateTime.Today.AddHours(8), DateTime.Today.AddHours(16), "Ward A");

            this.mockShiftRepository.Verify(
                shiftRepository => shiftRepository.AddShift(It.Is<Shift>(createdShift => createdShift.AppointedStaff.StaffID == 1)),
                Times.Once);
        }

        [Test]
        public void ValidateShiftTimes_WhenEndTimeIsAfterStartTime_ReturnsTrue()
        {
            var validationResult = this.shiftManagementService.ValidateShiftTimes(TimeSpan.FromHours(8), TimeSpan.FromHours(16));

            Assert.That(validationResult, Is.True);
        }

        [Test]
        public void ReassignShift_WhenNewStaffHasDifferentType_ReturnsFalse()
        {
            var doctor = CreateDoctor(1, "Cardiology");
            var pharmacist = CreatePharmacist(2, "General");
            var existingShift = new Shift(5, doctor, "Ward A", DateTime.Today.AddHours(8), DateTime.Today.AddHours(16), ShiftStatus.SCHEDULED);

            var reassignmentResult = this.shiftManagementService.ReassignShift(existingShift, pharmacist);

            Assert.That(reassignmentResult, Is.False);
        }

        [Test]
        public void ReassignShift_WhenNewStaffIsEligible_UpdatesShiftStaffIdentifier()
        {
            var currentDoctor = CreateDoctor(1, "Cardiology");
            var replacementDoctor = CreateDoctor(2, "Cardiology");
            var existingShift = new Shift(5, currentDoctor, "Ward A", DateTime.Today.AddHours(8), DateTime.Today.AddHours(16), ShiftStatus.SCHEDULED);

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift>());

            this.shiftManagementService.ReassignShift(existingShift, replacementDoctor);

            this.mockShiftRepository.Verify(
                shiftRepository => shiftRepository.UpdateShiftStaffId(5, 2),
                Times.Once);
        }

        [Test]
        public void GetFilteredStaff_WhenLocationIsPharmacy_ReturnsPharmacistsWithRequestedCertification()
        {
            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff>
                {
                    CreateDoctor(1, "Cardiology"),
                    CreatePharmacist(2, "Vaccination"),
                });

            var filteredStaff = this.shiftManagementService.GetFilteredStaff("Pharmacy", "vaccination");

            Assert.That(filteredStaff.Count, Is.EqualTo(1));
        }

        [Test]
        public void FindStaffReplacements_WhenReplacementHasNoOverlap_ReturnsReplacement()
        {
            var currentDoctor = CreateDoctor(1, "Cardiology");
            var replacementDoctor = CreateDoctor(2, "Cardiology");
            var existingShift = new Shift(5, currentDoctor, "Ward A", DateTime.Today.AddHours(8), DateTime.Today.AddHours(16), ShiftStatus.SCHEDULED);

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff> { currentDoctor, replacementDoctor });

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift>());

            var replacements = this.shiftManagementService.FindStaffReplacements(existingShift);

            Assert.That(replacements.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetSpecializationsAndCertificationsForLocation_WhenLocationIsNotPharmacy_ReturnsDistinctDoctorSpecializations()
        {
            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff>
                {
                    CreateDoctor(1, "Cardiology"),
                    CreateDoctor(2, "cardiology"),
                    CreateDoctor(3, "Emergency"),
                });

            var qualifications = this.shiftManagementService.GetSpecializationsAndCertificationsForLocation("Ward A");

            Assert.That(qualifications.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetActiveShifts_WhenOnlyOneShiftIsActive_ReturnsOnlyActiveShift()
        {
            var doctor = CreateDoctor(1, "Cardiology");

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff> { doctor });

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift>
                {
                    new Shift(1, doctor, "Ward A", DateTime.Today.AddHours(8), DateTime.Today.AddHours(16), ShiftStatus.ACTIVE),
                    new Shift(2, doctor, "Ward A", DateTime.Today.AddHours(17), DateTime.Today.AddHours(20), ShiftStatus.SCHEDULED),
                });

            var activeShifts = this.shiftManagementService.GetActiveShifts();

            Assert.That(activeShifts.Count, Is.EqualTo(1));
        }

        [Test]
        public void IsStaffWorkingDuring_WhenStaffHasOverlappingActiveShift_ReturnsTrue()
        {
            var doctor = CreateDoctor(1, "Cardiology");

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift>
                {
                    new Shift(1, doctor, "Ward A", DateTime.Today.AddHours(8), DateTime.Today.AddHours(16), ShiftStatus.ACTIVE),
                });

            var isWorkingDuringInterval = this.shiftManagementService.IsStaffWorkingDuring(
                1,
                DateTime.Today.AddHours(10),
                DateTime.Today.AddHours(12));

            Assert.That(isWorkingDuringInterval, Is.True);
        }

        private static Doctor CreateDoctor(int doctorIdentifier, string specialization)
        {
            return new Doctor(doctorIdentifier, "John", "Doe", "contract", true, specialization, "License", DoctorStatus.AVAILABLE, 5);
        }

        private static Pharmacyst CreatePharmacist(int pharmacistIdentifier, string certification)
        {
            var pharmacist = new Pharmacyst(pharmacistIdentifier, "Alice", "Smith", "contract", true, "", 10);
            pharmacist.Certification = certification;
            return pharmacist;
        }
    }
}