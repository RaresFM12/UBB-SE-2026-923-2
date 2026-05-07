using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.Tests.Services
{
    [TestFixture]
    public class PharmacyScheduleServiceTests
    {
        private Mock<IShiftRepository> mockShiftRepo;
        private Mock<IPharmacyStaffRepository> mockStaffRepo;
        private PharmacyScheduleService service;

        [SetUp]
        public void Setup()
        {
            mockShiftRepo = new Mock<IShiftRepository>();
            mockStaffRepo = new Mock<IPharmacyStaffRepository>();
            service = new PharmacyScheduleService(mockShiftRepo.Object, mockStaffRepo.Object);
        }

        [Test]
        public async Task GetShiftsAsync_NoShifts_ReturnsEmpty()
        {
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift>());
            var result = await service.GetShiftsAsync(1, DateTime.Now, DateTime.Now.AddDays(7));
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetShiftsAsync_ShiftsForDifferentStaff_ReturnsEmpty()
        {
            var staff = new Doctor { StaffID = 2 };
            var shift = new Shift(1, staff, "Pharmacy", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await service.GetShiftsAsync(1, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetShiftsAsync_ShiftInRange_ReturnsIt()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var now = DateTime.Now;
            var shift = new Shift(1, staff, "Pharmacy", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await service.GetShiftsAsync(1, now.AddHours(-1), now.AddHours(10));
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetShiftsAsync_ShiftOutOfRange_ReturnsEmpty()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var shift = new Shift(1, staff, "Pharmacy", DateTime.Now.AddDays(10), DateTime.Now.AddDays(10).AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await service.GetShiftsAsync(1, DateTime.Now, DateTime.Now.AddDays(1));
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetShiftsAsync_MultipleShifts_ReturnsOrderedByStart()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var now = DateTime.Now;
            var shift1 = new Shift(1, staff, "Pharmacy", now.AddHours(5), now.AddHours(10), ShiftStatus.SCHEDULED);
            var shift2 = new Shift(2, staff, "Pharmacy", now.AddHours(1), now.AddHours(4), ShiftStatus.SCHEDULED);
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift1, shift2 });

            var result = await service.GetShiftsAsync(1, now, now.AddDays(1));
            Assert.That(result[0].Id, Is.EqualTo(2));
            Assert.That(result[1].Id, Is.EqualTo(1));
        }

        [Test]
        public void GetPharmacists_ReturnsFromRepo()
        {
            var pharmacists = new List<Pharmacyst>
            {
                new Pharmacyst(1, "A", "B", "", true, "cert", 5),
                new Pharmacyst(2, "C", "D", "", true, "cert2", 3),
            };
            mockStaffRepo.Setup(r => r.GetPharmacists()).Returns(pharmacists);
            var result = service.GetPharmacists();
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetPharmacists_EmptyRepo_ReturnsEmpty()
        {
            mockStaffRepo.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst>());
            var result = service.GetPharmacists();
            Assert.That(result.Count, Is.EqualTo(0));
        }
    }
}
