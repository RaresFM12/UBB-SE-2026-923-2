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
    public class FatigueAuditServiceTests
    {
        private Mock<IShiftRepository> mockShiftRepository;
        private Mock<IStaffRepository> mockStaffRepository;
        private FatigueAuditService service;
        private Doctor doctor1;

        [SetUp]
        public void Setup()
        {
            this.mockShiftRepository = new Mock<IShiftRepository>();
            this.mockStaffRepository = new Mock<IStaffRepository>();
            this.service = new FatigueAuditService(this.mockShiftRepository.Object, this.mockStaffRepository.Object);

            this.doctor1 = new Doctor(1, "John", "Doe", "c", true, "Gen", "L1", DoctorStatus.AVAILABLE, 5);
        }

        // --- Constructor Tests ---
        [Test]
        public void Constructor_NullShiftRepo_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new FatigueAuditService(null, this.mockStaffRepository.Object));
        }

        [Test]
        public void Constructor_NullStaffRepo_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new FatigueAuditService(this.mockShiftRepository.Object, null));
        }

        // --- ReassignShift Tests ---
        [Test]
        public void ReassignShift_ValidIds_ReturnsTrue()
        {
            this.service.ReassignShift(1, 2);
            this.mockShiftRepository.Verify(repository => repository.UpdateShiftStaffId(1, 2), Times.Once);
        }

        [Test]
        public void ReassignShift_InvalidId_ReturnsFalse()
        {
            var result = this.service.ReassignShift(-1, 2);
            Assert.That(result, Is.False);
        }

        // --- RunAutoAudit Tests ---
        [Test]
        public void RunAutoAudit_NoShifts_NoViolations()
        {
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1 });
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());

            var result = this.service.RunAutoAudit(DateTime.Now);
            Assert.That(result.Violations.Count, Is.EqualTo(0));
        }

        [Test]
        public void RunAutoAudit_ExceedMaxWeeklyHours_ReportsViolation()
        {
            var monday = new DateTime(2025, 1, 6); // Monday
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1 });
            var shifts = new List<Shift>();
            for (int day = 0; day < 7; day++)
            {
                shifts.Add(new Shift(day + 1, this.doctor1, "Ward",
                    monday.AddDays(day).AddHours(6),
                    monday.AddDays(day).AddHours(16), // 10 hours each = 70 total
                    ShiftStatus.SCHEDULED));
            }

            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(shifts);

            var result = this.service.RunAutoAudit(monday);
            Assert.That(result.HasConflicts, Is.True);
        }

        [Test]
        public void RunAutoAudit_InsufficientRest_ReportsViolation()
        {
            var monday = new DateTime(2025, 1, 6);
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1 });
            var shifts = new List<Shift>
            {
                new Shift(1, this.doctor1, "Ward", monday.AddHours(6), monday.AddHours(14), ShiftStatus.SCHEDULED),
                new Shift(2, this.doctor1, "Ward", monday.AddHours(18), monday.AddHours(26), ShiftStatus.SCHEDULED), // 4h gap
            };
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(shifts);

            var result = this.service.RunAutoAudit(monday);
            Assert.That(result.HasConflicts, Is.True);
        }

        [Test]
        public void RunAutoAudit_CancelledShifts_Ignored()
        {
            var monday = new DateTime(2025, 1, 6);
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1 });

            var shifts = new List<Shift>
            {
                new Shift(1, this.doctor1, "Ward", monday.AddHours(6), monday.AddHours(16), ShiftStatus.CANCELLED),
            };
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(shifts);

            var result = this.service.RunAutoAudit(monday);
            Assert.That(result.Violations.Count, Is.EqualTo(0));
        }

        [Test]
        public void RunAutoAudit_ShiftsFromDifferentWeek_NotCounted()
        {
            var monday = new DateTime(2025, 1, 6);
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1 });
            var nextMonday = monday.AddDays(7);
            var shifts = new List<Shift>();
            for (int day = 0; day < 7; day++)
            {
                shifts.Add(new Shift(day + 1, this.doctor1, "Ward",
                    nextMonday.AddDays(day).AddHours(6),
                    nextMonday.AddDays(day).AddHours(16),
                    ShiftStatus.SCHEDULED));
            }

            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(shifts);

            var result = this.service.RunAutoAudit(monday);
            Assert.That(result.Violations.Any(violation => violation.Rule == "MAX_60H_PER_WEEK"), Is.False);
        }
    }
}