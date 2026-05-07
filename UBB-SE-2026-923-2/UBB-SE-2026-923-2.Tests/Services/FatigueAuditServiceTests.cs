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
        private Doctor doctor2;

        [SetUp]
        public void Setup()
        {
            this.mockShiftRepository = new Mock<IShiftRepository>();
            this.mockStaffRepository = new Mock<IStaffRepository>();
            this.service = new FatigueAuditService(this.mockShiftRepository.Object, this.mockStaffRepository.Object);

            this.doctor1 = new Doctor(1, "John", "Doe", "c", true, "Gen", "L1", DoctorStatus.AVAILABLE, 5);
            this.doctor2 = new Doctor(2, "Jane", "Smith", "c", true, "Surgery", "L2", DoctorStatus.AVAILABLE, 3);
        }

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

        [Test]
        public void ReassignShift_ValidIds_ReturnsTrue()
        {
            this.service.ReassignShift(1, 2);
            this.mockShiftRepository.Verify(repository => repository.UpdateShiftStaffId(1, 2), Times.Once);
        }

        [Test]
        public void ReassignShift_ZeroShiftId_ReturnsFalse()
        {
            var result = this.service.ReassignShift(0, 2);
            Assert.That(result, Is.False);
        }

        [Test]
        public void ReassignShift_NegativeShiftId_ReturnsFalse()
        {
            var result = this.service.ReassignShift(-1, 2);
            Assert.That(result, Is.False);
        }

        [Test]
        public void ReassignShift_ZeroStaffId_ReturnsFalse()
        {
            var result = this.service.ReassignShift(1, 0);
            Assert.That(result, Is.False);
        }

        [Test]
        public void ReassignShift_NegativeStaffId_ReturnsFalse()
        {
            var result = this.service.ReassignShift(1, -1);
            Assert.That(result, Is.False);
        }

        [Test]
        public void RunAutoAudit_NoShifts_NoViolations()
        {
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1 });
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());

            var result = this.service.RunAutoAudit(DateTime.Now);
            Assert.That(result.Violations.Count, Is.EqualTo(0));
            Assert.That(result.HasConflicts, Is.False);
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
            Assert.That(result.Violations.Any(violation => violation.Rule == "MAX_60H_PER_WEEK"), Is.True);
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
            Assert.That(result.Violations.Any(violation => violation.Rule == "MIN_12H_REST"), Is.True);
        }

        [Test]
        public void RunAutoAudit_SufficientRest_NoViolation()
        {
            var monday = new DateTime(2025, 1, 6);
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1 });

            var shifts = new List<Shift>
            {
                new Shift(1, this.doctor1, "Ward", monday.AddHours(6), monday.AddHours(14), ShiftStatus.SCHEDULED),
                new Shift(2, this.doctor1, "Ward", monday.AddDays(1).AddHours(6), monday.AddDays(1).AddHours(14), ShiftStatus.SCHEDULED), // 16h gap
            };
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(shifts);

            var result = this.service.RunAutoAudit(monday);
            Assert.That(result.Violations.Any(violation => violation.Rule == "MIN_12H_REST"), Is.False);
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
        public void RunAutoAudit_MultipleStaff_EachCheckedIndependently()
        {
            var monday = new DateTime(2025, 1, 6);
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1, this.doctor2 });
            var shifts = new List<Shift>();
            for (int day = 0; day < 7; day++)
            {
                shifts.Add(new Shift(day + 1, this.doctor1, "Ward", monday.AddDays(day).AddHours(6), monday.AddDays(day).AddHours(16), ShiftStatus.SCHEDULED));
            }

            shifts.Add(new Shift(8, this.doctor2, "Ward", monday.AddHours(8), monday.AddHours(16), ShiftStatus.SCHEDULED));
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(shifts);

            var result = this.service.RunAutoAudit(monday);
            Assert.That(result.Violations.All(violation => violation.StaffId == 1), Is.True);
        }

        [Test]
        public void RunAutoAudit_Exactly60Hours_NoViolation()
        {
            var monday = new DateTime(2025, 1, 6);
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1 });
            var shifts = new List<Shift>();
            for (int day = 0; day < 6; day++)
            {
                shifts.Add(new Shift(day + 1, this.doctor1, "Ward",
                    monday.AddDays(day).AddHours(6),
                    monday.AddDays(day).AddHours(16),
                    ShiftStatus.SCHEDULED));
            }

            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(shifts);

            var result = this.service.RunAutoAudit(monday);
            Assert.That(result.Violations.Any(violation => violation.Rule == "MAX_60H_PER_WEEK"), Is.False);
        }

        [Test]
        public void RunAutoAudit_Exactly12HoursRest_NoViolation()
        {
            var monday = new DateTime(2025, 1, 6);
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1 });

            var shifts = new List<Shift>
            {
                new Shift(1, this.doctor1, "Ward", monday.AddHours(6), monday.AddHours(14), ShiftStatus.SCHEDULED),
                new Shift(2, this.doctor1, "Ward", monday.AddHours(26), monday.AddHours(34), ShiftStatus.SCHEDULED), // 12h gap exactly
            };
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(shifts);

            var result = this.service.RunAutoAudit(monday);
            Assert.That(result.Violations.Any(violation => violation.Rule == "MIN_12H_REST"), Is.False);
        }

        [Test]
        public void RunAutoAudit_NoStaff_NoViolations()
        {
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff>());
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());

            var result = this.service.RunAutoAudit(DateTime.Now);
            Assert.That(result.Violations.Count, Is.EqualTo(0));
        }

        [Test]
        public void RunAutoAudit_BothViolationsSameStaff_ReportsBoth()
        {
            var monday = new DateTime(2025, 1, 6);
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1 });
            var shifts = new List<Shift>();
            for (int day = 0; day < 7; day++)
            {
                shifts.Add(new Shift(day + 1, this.doctor1, "Ward",
                    monday.AddDays(day).AddHours(6),
                    monday.AddDays(day).AddHours(16),
                    ShiftStatus.SCHEDULED));
            }

            shifts.Add(new Shift(8, this.doctor1, "Ward", monday.AddHours(20), monday.AddHours(28), ShiftStatus.SCHEDULED));
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(shifts);

            var result = this.service.RunAutoAudit(monday);
            Assert.That(result.HasConflicts, Is.True);
            Assert.That(result.Violations.Any(violation => violation.Rule == "MAX_60H_PER_WEEK"), Is.True);
            Assert.That(result.Violations.Any(violation => violation.Rule == "MIN_12H_REST"), Is.True);
        }

        [Test]
        public void ReassignShift_ValidIds_ReturnsTrue_Verifiable()
        {
            var result = this.service.ReassignShift(5, 10);
            Assert.That(result, Is.True);
            this.mockShiftRepository.Verify(repository => repository.UpdateShiftStaffId(5, 10), Times.Once);
        }

        [Test]
        public void RunAutoAudit_ActiveShifts_AreConsidered()
        {
            var monday = new DateTime(2025, 1, 6);
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1 });

            var shifts = new List<Shift>();
            for (int day = 0; day < 7; day++)
            {
                shifts.Add(new Shift(day + 1, this.doctor1, "Ward",
                    monday.AddDays(day).AddHours(6),
                    monday.AddDays(day).AddHours(16),
                    ShiftStatus.ACTIVE));
            }

            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(shifts);

            var result = this.service.RunAutoAudit(monday);
            Assert.That(result.HasConflicts, Is.True);
        }

        [Test]
        public void RunAutoAudit_CompletedShifts_AreConsidered()
        {
            var monday = new DateTime(2025, 1, 6);
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1 });

            var shifts = new List<Shift>
            {
                new Shift(1, this.doctor1, "Ward", monday.AddHours(6), monday.AddHours(14), ShiftStatus.COMPLETED),
                new Shift(2, this.doctor1, "Ward", monday.AddHours(18), monday.AddHours(26), ShiftStatus.COMPLETED), // 4h gap
            };
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(shifts);

            var result = this.service.RunAutoAudit(monday);
            Assert.That(result.Violations.Any(violation => violation.Rule == "MIN_12H_REST"), Is.True);
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

        [Test]
        public void RunAutoAudit_SingleShortShift_NoViolations()
        {
            var monday = new DateTime(2025, 1, 6);
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1 });

            var shifts = new List<Shift>
            {
                new Shift(1, this.doctor1, "Ward", monday.AddHours(9), monday.AddHours(13), ShiftStatus.SCHEDULED),
            };
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(shifts);

            var result = this.service.RunAutoAudit(monday);
            Assert.That(result.HasConflicts, Is.False);
            Assert.That(result.Violations.Count, Is.EqualTo(0));
        }

        [Test]
        public void RunAutoAudit_ThreeShiftsWithSufficientRest_NoRestViolation()
        {
            var monday = new DateTime(2025, 1, 6);
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1 });

            var shifts = new List<Shift>
            {
                new Shift(1, this.doctor1, "Ward", monday.AddHours(6), monday.AddHours(14), ShiftStatus.SCHEDULED),
                new Shift(2, this.doctor1, "Ward", monday.AddDays(1).AddHours(6), monday.AddDays(1).AddHours(14), ShiftStatus.SCHEDULED),
                new Shift(3, this.doctor1, "Ward", monday.AddDays(2).AddHours(6), monday.AddDays(2).AddHours(14), ShiftStatus.SCHEDULED),
            };
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(shifts);

            var result = this.service.RunAutoAudit(monday);
            Assert.That(result.Violations.Any(violation => violation.Rule == "MIN_12H_REST"), Is.False);
        }
    }
}
