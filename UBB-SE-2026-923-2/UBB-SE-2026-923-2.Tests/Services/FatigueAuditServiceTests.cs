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
    public class FatigueAuditServiceTests
    {
        private Mock<IShiftRepository> mockShiftRepo;
        private Mock<IStaffRepository> mockStaffRepo;
        private FatigueAuditService service;
        private Doctor doctor1;
        private Doctor doctor2;

        [SetUp]
        public void Setup()
        {
            mockShiftRepo = new Mock<IShiftRepository>();
            mockStaffRepo = new Mock<IStaffRepository>();
            service = new FatigueAuditService(mockShiftRepo.Object, mockStaffRepo.Object);

            doctor1 = new Doctor(1, "John", "Doe", "c", true, "Gen", "L1", DoctorStatus.AVAILABLE, 5);
            doctor2 = new Doctor(2, "Jane", "Smith", "c", true, "Surgery", "L2", DoctorStatus.AVAILABLE, 3);
        }

        [Test]
        public void Constructor_NullShiftRepo_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new FatigueAuditService(null, mockStaffRepo.Object));
        }

        [Test]
        public void Constructor_NullStaffRepo_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new FatigueAuditService(mockShiftRepo.Object, null));
        }

        [Test]
        public void ReassignShift_ValidIds_ReturnsTrue()
        {
            service.ReassignShift(1, 2);
            mockShiftRepo.Verify(r => r.UpdateShiftStaffId(1, 2), Times.Once);
        }

        [Test]
        public void ReassignShift_ZeroShiftId_ReturnsFalse()
        {
            var result = service.ReassignShift(0, 2);
            Assert.That(result, Is.False);
        }

        [Test]
        public void ReassignShift_NegativeShiftId_ReturnsFalse()
        {
            var result = service.ReassignShift(-1, 2);
            Assert.That(result, Is.False);
        }

        [Test]
        public void ReassignShift_ZeroStaffId_ReturnsFalse()
        {
            var result = service.ReassignShift(1, 0);
            Assert.That(result, Is.False);
        }

        [Test]
        public void ReassignShift_NegativeStaffId_ReturnsFalse()
        {
            var result = service.ReassignShift(1, -1);
            Assert.That(result, Is.False);
        }

        [Test]
        public void RunAutoAudit_NoShifts_NoViolations()
        {
            mockStaffRepo.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { doctor1 });
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift>());

            var result = service.RunAutoAudit(DateTime.Now);
            Assert.That(result.Violations.Count, Is.EqualTo(0));
            Assert.That(result.HasConflicts, Is.False);
        }

        [Test]
        public void RunAutoAudit_ExceedMaxWeeklyHours_ReportsViolation()
        {
            var monday = new DateTime(2025, 1, 6); // Monday
            mockStaffRepo.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { doctor1 });

            // Create shifts totaling > 60 hours in the week
            var shifts = new List<Shift>();
            for (int day = 0; day < 7; day++)
            {
                shifts.Add(new Shift(day + 1, doctor1, "Ward",
                    monday.AddDays(day).AddHours(6),
                    monday.AddDays(day).AddHours(16), // 10 hours each = 70 total
                    ShiftStatus.SCHEDULED));
            }
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(shifts);

            var result = service.RunAutoAudit(monday);
            Assert.That(result.HasConflicts, Is.True);
            Assert.That(result.Violations.Any(v => v.Rule == "MAX_60H_PER_WEEK"), Is.True);
        }

        [Test]
        public void RunAutoAudit_InsufficientRest_ReportsViolation()
        {
            var monday = new DateTime(2025, 1, 6);
            mockStaffRepo.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { doctor1 });

            // Two shifts with less than 12h gap
            var shifts = new List<Shift>
            {
                new Shift(1, doctor1, "Ward", monday.AddHours(6), monday.AddHours(14), ShiftStatus.SCHEDULED),
                new Shift(2, doctor1, "Ward", monday.AddHours(18), monday.AddHours(26), ShiftStatus.SCHEDULED), // 4h gap
            };
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(shifts);

            var result = service.RunAutoAudit(monday);
            Assert.That(result.HasConflicts, Is.True);
            Assert.That(result.Violations.Any(v => v.Rule == "MIN_12H_REST"), Is.True);
        }

        [Test]
        public void RunAutoAudit_SufficientRest_NoViolation()
        {
            var monday = new DateTime(2025, 1, 6);
            mockStaffRepo.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { doctor1 });

            var shifts = new List<Shift>
            {
                new Shift(1, doctor1, "Ward", monday.AddHours(6), monday.AddHours(14), ShiftStatus.SCHEDULED),
                new Shift(2, doctor1, "Ward", monday.AddDays(1).AddHours(6), monday.AddDays(1).AddHours(14), ShiftStatus.SCHEDULED), // 16h gap
            };
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(shifts);

            var result = service.RunAutoAudit(monday);
            Assert.That(result.Violations.Any(v => v.Rule == "MIN_12H_REST"), Is.False);
        }

        [Test]
        public void RunAutoAudit_CancelledShifts_Ignored()
        {
            var monday = new DateTime(2025, 1, 6);
            mockStaffRepo.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { doctor1 });

            var shifts = new List<Shift>
            {
                new Shift(1, doctor1, "Ward", monday.AddHours(6), monday.AddHours(16), ShiftStatus.CANCELLED),
            };
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(shifts);

            var result = service.RunAutoAudit(monday);
            Assert.That(result.Violations.Count, Is.EqualTo(0));
        }

        [Test]
        public void RunAutoAudit_MultipleStaff_EachCheckedIndependently()
        {
            var monday = new DateTime(2025, 1, 6);
            mockStaffRepo.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { doctor1, doctor2 });

            // doctor1 has too many hours, doctor2 is fine
            var shifts = new List<Shift>();
            for (int day = 0; day < 7; day++)
            {
                shifts.Add(new Shift(day + 1, doctor1, "Ward", monday.AddDays(day).AddHours(6), monday.AddDays(day).AddHours(16), ShiftStatus.SCHEDULED));
            }
            shifts.Add(new Shift(8, doctor2, "Ward", monday.AddHours(8), monday.AddHours(16), ShiftStatus.SCHEDULED));
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(shifts);

            var result = service.RunAutoAudit(monday);
            Assert.That(result.Violations.All(v => v.StaffId == 1), Is.True);
        }
    }
}
