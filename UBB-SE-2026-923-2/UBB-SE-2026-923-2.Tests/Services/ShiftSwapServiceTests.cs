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
    public class ShiftSwapServiceTests
    {
        private Mock<IStaffRepository> mockStaffRepo;
        private Mock<IShiftRepository> mockShiftRepo;
        private Mock<IShiftSwapRepository> mockSwapRepo;
        private Mock<INotificationRepository> mockNotifRepo;
        private ShiftSwapService service;
        private Doctor doctor1;
        private Doctor doctor2;

        [SetUp]
        public void Setup()
        {
            mockStaffRepo = new Mock<IStaffRepository>();
            mockShiftRepo = new Mock<IShiftRepository>();
            mockSwapRepo = new Mock<IShiftSwapRepository>();
            mockNotifRepo = new Mock<INotificationRepository>();
            service = new ShiftSwapService(mockStaffRepo.Object, mockShiftRepo.Object, mockSwapRepo.Object, mockNotifRepo.Object);

            doctor1 = new Doctor(1, "John", "Doe", "c", true, "Cardiology", "L1", DoctorStatus.AVAILABLE, 5);
            doctor2 = new Doctor(2, "Jane", "Smith", "c", true, "Surgery", "L2", DoctorStatus.AVAILABLE, 3);
        }

        [Test]
        public void GetFutureShiftsForStaff_ReturnsOnlyFutureShifts()
        {
            var past = new Shift(1, doctor1, "A", DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1).AddHours(8), ShiftStatus.COMPLETED);
            var future = new Shift(2, doctor1, "A", DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { past, future });

            var result = service.GetFutureShiftsForStaff(1);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(2));
        }

        [Test]
        public void GetFutureShiftsForStaff_DifferentStaff_ReturnsEmpty()
        {
            var shift = new Shift(1, doctor2, "A", DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = service.GetFutureShiftsForStaff(1);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetEligibleSwapColleaguesForShift_ShiftNotFound_ReturnsEmpty()
        {
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift>());
            var result = service.GetEligibleSwapColleaguesForShift(1, 99, out string error);
            Assert.That(result.Count, Is.EqualTo(0));
            Assert.That(error, Is.EqualTo("Shift not found."));
        }

        [Test]
        public void GetEligibleSwapColleaguesForShift_NotOwnShift_ReturnsEmpty()
        {
            var shift = new Shift(1, doctor2, "A", DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = service.GetEligibleSwapColleaguesForShift(1, 1, out string error);
            Assert.That(error, Does.Contain("own shift"));
        }

        [Test]
        public void GetEligibleSwapColleaguesForShift_PastShift_ReturnsEmpty()
        {
            var shift = new Shift(1, doctor1, "A", DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1).AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = service.GetEligibleSwapColleaguesForShift(1, 1, out string error);
            Assert.That(error, Does.Contain("future"));
        }

        [Test]
        public void AcceptSwapRequest_RequestNotFound_ReturnsFalse()
        {
            mockSwapRepo.Setup(r => r.GetShiftSwapRequestById(1)).Returns((ShiftSwapRequest)null);
            var result = service.AcceptSwapRequest(1, 2, out string msg);
            Assert.That(result, Is.False);
            Assert.That(msg, Does.Contain("not found"));
        }

        [Test]
        public void AcceptSwapRequest_WrongColleague_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 3);
            mockSwapRepo.Setup(r => r.GetShiftSwapRequestById(1)).Returns(swap);
            var result = service.AcceptSwapRequest(1, 2, out string msg);
            Assert.That(result, Is.False);
            Assert.That(msg, Does.Contain("cannot accept"));
        }

        [Test]
        public void AcceptSwapRequest_NotPending_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2) { Status = ShiftSwapRequestStatus.ACCEPTED };
            mockSwapRepo.Setup(r => r.GetShiftSwapRequestById(1)).Returns(swap);
            var result = service.AcceptSwapRequest(1, 2, out string msg);
            Assert.That(result, Is.False);
            Assert.That(msg, Does.Contain("no longer pending"));
        }

        [Test]
        public void AcceptSwapRequest_ShiftNotFound_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 99, 1, 2);
            mockSwapRepo.Setup(r => r.GetShiftSwapRequestById(1)).Returns(swap);
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift>());
            var result = service.AcceptSwapRequest(1, 2, out string msg);
            Assert.That(result, Is.False);
            Assert.That(msg, Does.Contain("Shift not found"));
        }

        [Test]
        public void AcceptSwapRequest_ColleagueOverlap_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            mockSwapRepo.Setup(r => r.GetShiftSwapRequestById(1)).Returns(swap);

            var now = DateTime.Now.AddDays(1);
            var targetShift = new Shift(1, doctor1, "A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            var colleagueShift = new Shift(2, doctor2, "B", now.AddHours(4), now.AddHours(12), ShiftStatus.SCHEDULED);
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { targetShift, colleagueShift });

            var result = service.AcceptSwapRequest(1, 2, out string msg);
            Assert.That(result, Is.False);
            Assert.That(msg, Does.Contain("already scheduled"));
        }

        [Test]
        public void AcceptSwapRequest_Valid_ReturnsTrue()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            mockSwapRepo.Setup(r => r.GetShiftSwapRequestById(1)).Returns(swap);

            var now = DateTime.Now.AddDays(1);
            var targetShift = new Shift(1, doctor1, "A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepo.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { targetShift });

            var result = service.AcceptSwapRequest(1, 2, out string msg);
            Assert.That(result, Is.True);
            Assert.That(msg, Does.Contain("accepted"));
            mockShiftRepo.Verify(r => r.UpdateShiftStaffId(1, 2), Times.Once);
            mockNotifRepo.Verify(r => r.AddNotification(1, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void RejectSwapRequest_RequestNotFound_ReturnsFalse()
        {
            mockSwapRepo.Setup(r => r.GetShiftSwapRequestById(1)).Returns((ShiftSwapRequest)null);
            var result = service.RejectSwapRequest(1, 2, out string msg);
            Assert.That(result, Is.False);
        }

        [Test]
        public void RejectSwapRequest_WrongColleague_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 3);
            mockSwapRepo.Setup(r => r.GetShiftSwapRequestById(1)).Returns(swap);
            var result = service.RejectSwapRequest(1, 2, out string msg);
            Assert.That(result, Is.False);
        }

        [Test]
        public void RejectSwapRequest_NotPending_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2) { Status = ShiftSwapRequestStatus.REJECTED };
            mockSwapRepo.Setup(r => r.GetShiftSwapRequestById(1)).Returns(swap);
            var result = service.RejectSwapRequest(1, 2, out string msg);
            Assert.That(result, Is.False);
        }

        [Test]
        public void RejectSwapRequest_Valid_ReturnsTrue()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            mockSwapRepo.Setup(r => r.GetShiftSwapRequestById(1)).Returns(swap);

            var result = service.RejectSwapRequest(1, 2, out string msg);
            Assert.That(result, Is.True);
            Assert.That(msg, Does.Contain("rejected"));
            mockNotifRepo.Verify(r => r.AddNotification(1, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void GetAllDoctors_ReturnsOrderedDoctors()
        {
            mockStaffRepo.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { doctor2, doctor1 });
            var result = service.GetAllDoctors();
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].FirstName, Is.EqualTo("Jane"));
            Assert.That(result[1].FirstName, Is.EqualTo("John"));
        }
    }
}
