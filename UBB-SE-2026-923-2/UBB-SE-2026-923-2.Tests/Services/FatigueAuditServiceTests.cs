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
    public class FatigueAuditServiceLogicTests
    {
        private Mock<IShiftRepository> mockShiftRepository;
        private Mock<IStaffRepository> mockStaffRepository;
        private FatigueAuditService fatigueAuditService;

        [SetUp]
        public void Setup()
        {
            this.mockShiftRepository = new Mock<IShiftRepository>();
            this.mockStaffRepository = new Mock<IStaffRepository>();

            this.fatigueAuditService = new FatigueAuditService(
                this.mockShiftRepository.Object,
                this.mockStaffRepository.Object);
        }

        [Test]
        public void Constructor_WhenShiftRepositoryIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new FatigueAuditService(null, this.mockStaffRepository.Object));
        }

        [Test]
        public void Constructor_WhenStaffRepositoryIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new FatigueAuditService(this.mockShiftRepository.Object, null));
        }

        [Test]
        public void ReassignShift_WhenShiftIdentifierAndNewStaffIdentifierAreValid_UpdatesShiftStaffIdentifier()
        {
            this.fatigueAuditService.ReassignShift(1, 2);

            this.mockShiftRepository.Verify(
                shiftRepository => shiftRepository.UpdateShiftStaffId(1, 2),
                Times.Once);
        }

        [Test]
        public void ReassignShift_WhenShiftIdentifierIsInvalid_ReturnsFalse()
        {
            var reassignmentResult = this.fatigueAuditService.ReassignShift(0, 2);

            Assert.That(reassignmentResult, Is.False);
        }

        [Test]
        public void ReassignShift_WhenNewStaffIdentifierIsInvalid_ReturnsFalse()
        {
            var reassignmentResult = this.fatigueAuditService.ReassignShift(1, 0);

            Assert.That(reassignmentResult, Is.False);
        }

        [Test]
        public void RunAutoAudit_WhenThereAreNoShifts_ReturnsResultWithoutConflicts()
        {
            var availableDoctor = CreateDoctor(1, "John", "Doe", "General", true, DoctorStatus.AVAILABLE);

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff> { availableDoctor });

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift>());

            var auditResult = this.fatigueAuditService.RunAutoAudit(new DateTime(2025, 1, 6));

            Assert.That(auditResult.HasConflicts, Is.False);
        }

        [Test]
        public void RunAutoAudit_WhenDoctorExceedsMaximumWeeklyHours_ReturnsMaximumWeeklyHoursViolation()
        {
            var auditWeekMonday = new DateTime(2025, 1, 6);
            var overworkedDoctor = CreateDoctor(1, "John", "Doe", "General", true, DoctorStatus.AVAILABLE);
            var weeklyShifts = CreateSevenTenHourShiftsForDoctor(overworkedDoctor, auditWeekMonday);

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff> { overworkedDoctor });

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(weeklyShifts);

            var auditResult = this.fatigueAuditService.RunAutoAudit(auditWeekMonday);

            Assert.That(auditResult.Violations.Any(auditViolation => auditViolation.Rule == "MAX_60H_PER_WEEK"), Is.True);
        }

        [Test]
        public void RunAutoAudit_WhenDoctorHasLessThanTwelveHoursRestBetweenShifts_ReturnsMinimumRestViolation()
        {
            var auditWeekMonday = new DateTime(2025, 1, 6);
            var tiredDoctor = CreateDoctor(1, "John", "Doe", "General", true, DoctorStatus.AVAILABLE);

            var weeklyShifts = new List<Shift>
            {
                new Shift(
                    1,
                    tiredDoctor,
                    "Ward A",
                    auditWeekMonday.AddHours(6),
                    auditWeekMonday.AddHours(14),
                    ShiftStatus.SCHEDULED),

                new Shift(
                    2,
                    tiredDoctor,
                    "Ward A",
                    auditWeekMonday.AddHours(18),
                    auditWeekMonday.AddHours(26),
                    ShiftStatus.SCHEDULED),
            };

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff> { tiredDoctor });

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(weeklyShifts);

            var auditResult = this.fatigueAuditService.RunAutoAudit(auditWeekMonday);

            Assert.That(auditResult.Violations.Any(auditViolation => auditViolation.Rule == "MIN_12H_REST"), Is.True);
        }

        [Test]
        public void RunAutoAudit_WhenShiftIsCancelled_DoesNotReturnViolationForCancelledShift()
        {
            var auditWeekMonday = new DateTime(2025, 1, 6);
            var availableDoctor = CreateDoctor(1, "John", "Doe", "General", true, DoctorStatus.AVAILABLE);

            var cancelledShift = new Shift(
                1,
                availableDoctor,
                "Ward A",
                auditWeekMonday.AddHours(6),
                auditWeekMonday.AddHours(20),
                ShiftStatus.CANCELLED);

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff> { availableDoctor });

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift> { cancelledShift });

            var auditResult = this.fatigueAuditService.RunAutoAudit(auditWeekMonday);

            Assert.That(auditResult.Violations.Count, Is.EqualTo(0));
        }

        [Test]
        public void RunAutoAudit_WhenMatchingAvailableReplacementExists_ReturnsSuggestedReplacementIdentifier()
        {
            var auditWeekMonday = new DateTime(2025, 1, 6);
            var overworkedDoctor = CreateDoctor(1, "John", "Doe", "General", true, DoctorStatus.AVAILABLE);
            var replacementDoctor = CreateDoctor(2, "Alice", "Smith", "General", true, DoctorStatus.AVAILABLE);
            var weeklyShifts = CreateSevenTenHourShiftsForDoctor(overworkedDoctor, auditWeekMonday);

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff> { overworkedDoctor, replacementDoctor });

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(weeklyShifts);

            var auditResult = this.fatigueAuditService.RunAutoAudit(auditWeekMonday);

            Assert.That(auditResult.Suggestions.First().SuggestedStaffId, Is.EqualTo(2));
        }

        private static Doctor CreateDoctor(
            int doctorIdentifier,
            string firstName,
            string lastName,
            string specialization,
            bool isAvailable,
            DoctorStatus doctorStatus)
        {
            return new Doctor(
                doctorIdentifier,
                firstName,
                lastName,
                "contract",
                isAvailable,
                specialization,
                "License",
                doctorStatus,
                5);
        }

        private static List<Shift> CreateSevenTenHourShiftsForDoctor(Doctor assignedDoctor, DateTime auditWeekMonday)
        {
            var weeklyShifts = new List<Shift>();

            for (var dayOffset = 0; dayOffset < 7; dayOffset++)
            {
                weeklyShifts.Add(
                    new Shift(
                        dayOffset + 1,
                        assignedDoctor,
                        "Ward A",
                        auditWeekMonday.AddDays(dayOffset).AddHours(6),
                        auditWeekMonday.AddDays(dayOffset).AddHours(16),
                        ShiftStatus.SCHEDULED));
            }

            return weeklyShifts;
        }
    }
}