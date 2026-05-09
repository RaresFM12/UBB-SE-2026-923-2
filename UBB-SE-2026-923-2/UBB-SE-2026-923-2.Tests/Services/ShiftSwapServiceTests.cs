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
    public class ShiftSwapServiceTests
    {
        private Mock<IStaffRepository> mockStaffRepository;
        private Mock<IShiftRepository> mockShiftRepository;
        private Mock<IShiftSwapRepository> mockShiftSwapRepository;
        private Mock<INotificationRepository> mockNotificationRepository;
        private ShiftSwapService service;
        private Doctor doctor1;
        private Doctor doctor2;

        [SetUp]
        public void Setup()
        {
            this.mockStaffRepository = new Mock<IStaffRepository>();
            this.mockShiftRepository = new Mock<IShiftRepository>();
            this.mockShiftSwapRepository = new Mock<IShiftSwapRepository>();
            this.mockNotificationRepository = new Mock<INotificationRepository>();
            this.service = new ShiftSwapService(this.mockStaffRepository.Object, this.mockShiftRepository.Object, this.mockShiftSwapRepository.Object, this.mockNotificationRepository.Object);

            this.doctor1 = new Doctor(1, "John", "Doe", "c", true, "Cardiology", "L1", DoctorStatus.AVAILABLE, 5);
            this.doctor2 = new Doctor(2, "Jane", "Smith", "c", true, "Surgery", "L2", DoctorStatus.AVAILABLE, 3);
        }

        [Test]
        public void GetFutureShiftsForStaff_ReturnsOnlyFutureShifts()
        {
            var past = new Shift(1, this.doctor1, "A", DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1).AddHours(8), ShiftStatus.COMPLETED);
            var future = new Shift(2, this.doctor1, "A", DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { past, future });

            var result = this.service.GetFutureShiftsForStaff(1);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(2));
        }

        [Test]
        public void GetEligibleSwapColleaguesForShift_ValidFutureShift_ReturnsColleagues()
        {
            var futureDate = DateTime.Now.AddDays(2);
            var shift = new Shift(1, this.doctor1, "A", futureDate, futureDate.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { shift });
            this.mockStaffRepository.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1, this.doctor2 });

            var result = this.service.GetEligibleSwapColleaguesForShift(1, 1, out string error);
            Assert.That(error, Is.Empty);
        }

        [Test]
        public void GetEligibleSwapColleagues_InvalidState_ReturnsError()
        {
            var pastShift = new Shift(1, this.doctor1, "A", DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1).AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { pastShift });

            this.service.GetEligibleSwapColleaguesForShift(1, 1, out string errorPast);
            Assert.That(errorPast, Does.Contain("future"));

            var differentStaffShift = new Shift(2, this.doctor2, "A", DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { differentStaffShift });

            this.service.GetEligibleSwapColleaguesForShift(1, 2, out string errorOwn);
            Assert.That(errorOwn, Does.Contain("own shift"));
        }

        [Test]
        public void AcceptSwapRequest_Valid_UpdatesAndNotifies()
        {
            var swap = new ShiftSwapRequest(1, new Shift { Id = 10 }, new Staff { StaffID = 1 }, new Staff { StaffID = 2 }); // Request for Shift 10, from Dr 1 to Dr 2
            this.mockShiftSwapRepository.Setup(r => r.GetShiftSwapRequestById(1)).Returns(swap);

            var targetShift = new Shift(10, this.doctor1, "A", DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { targetShift });

            var result = this.service.AcceptSwapRequest(1, 2, out string message);

            Assert.That(result, Is.True);
            this.mockShiftRepository.Verify(r => r.UpdateShiftStaffId(10, 2), Times.Once);
            this.mockShiftSwapRepository.Verify(r => r.UpdateShiftSwapRequestStatus(1, "ACCEPTED"), Times.Once);
            this.mockNotificationRepository.Verify(r => r.AddNotification(1, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void AcceptSwapRequest_InvalidContext_ReturnsFalse()
        {
            // Case 1: Not Found
            this.mockShiftSwapRepository.Setup(r => r.GetShiftSwapRequestById(1)).Returns((ShiftSwapRequest)null);
            Assert.That(this.service.AcceptSwapRequest(1, 2, out _), Is.False);

            // Case 2: Wrong Colleague
            var swapWrong = new ShiftSwapRequest(2, new Shift { Id = 10 }, new Staff { StaffID = 1 }, new Staff { StaffID = 3 });
            this.mockShiftSwapRepository.Setup(r => r.GetShiftSwapRequestById(2)).Returns(swapWrong);
            Assert.That(this.service.AcceptSwapRequest(2, 2, out _), Is.False);

            // Case 3: Not Pending
            var swapDone = new ShiftSwapRequest(3, new Shift { Id = 10 }, new Staff { StaffID = 1 }, new Staff { StaffID = 2 }) { Status = ShiftSwapRequestStatus.ACCEPTED };
            this.mockShiftSwapRepository.Setup(r => r.GetShiftSwapRequestById(3)).Returns(swapDone);
            Assert.That(this.service.AcceptSwapRequest(3, 2, out _), Is.False);
        }

        [Test]
        public void AcceptSwapRequest_Overlap_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, new Shift { Id = 10 }, new Staff { StaffID = 1 }, new Staff { StaffID = 2 });
            this.mockShiftSwapRepository.Setup(r => r.GetShiftSwapRequestById(1)).Returns(swap);

            var now = DateTime.Now.AddDays(1);
            var targetShift = new Shift(10, this.doctor1, "A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            var colleagueShift = new Shift(20, this.doctor2, "B", now.AddHours(4), now.AddHours(12), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(r => r.GetAllShifts()).Returns(new List<Shift> { targetShift, colleagueShift });

            var result = this.service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("already scheduled"));
        }

        [Test]
        public void RejectSwapRequest_Valid_UpdatesAndNotifies()
        {
            var swap = new ShiftSwapRequest(1, new Shift { Id = 10 }, new Staff { StaffID = 1 }, new Staff { StaffID = 2 });
            this.mockShiftSwapRepository.Setup(r => r.GetShiftSwapRequestById(1)).Returns(swap);

            var result = this.service.RejectSwapRequest(1, 2, out string message);

            Assert.That(result, Is.True);
            this.mockShiftSwapRepository.Verify(r => r.UpdateShiftSwapRequestStatus(1, "REJECTED"), Times.Once);
            this.mockNotificationRepository.Verify(r => r.AddNotification(1, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void GetAllDoctors_ReturnsOrderedDoctors()
        {
            this.mockStaffRepository.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { this.doctor2, this.doctor1 });
            var result = this.service.GetAllDoctors();
            Assert.That(result[0].FirstName, Is.EqualTo("Jane"));
            Assert.That(result[1].FirstName, Is.EqualTo("John"));
        }
    }
}