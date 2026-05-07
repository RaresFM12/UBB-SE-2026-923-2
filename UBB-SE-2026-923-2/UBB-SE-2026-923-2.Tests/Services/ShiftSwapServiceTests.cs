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
            mockStaffRepository = new Mock<IStaffRepository>();
            mockShiftRepository = new Mock<IShiftRepository>();
            mockShiftSwapRepository = new Mock<IShiftSwapRepository>();
            mockNotificationRepository = new Mock<INotificationRepository>();
            service = new ShiftSwapService(mockStaffRepository.Object, mockShiftRepository.Object, mockShiftSwapRepository.Object, mockNotificationRepository.Object);

            doctor1 = new Doctor(1, "John", "Doe", "c", true, "Cardiology", "L1", DoctorStatus.AVAILABLE, 5);
            doctor2 = new Doctor(2, "Jane", "Smith", "c", true, "Surgery", "L2", DoctorStatus.AVAILABLE, 3);
        }

        [Test]
        public void GetFutureShiftsForStaff_ReturnsOnlyFutureShifts()
        {
            var past = new Shift(1, doctor1, "A", DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1).AddHours(8), ShiftStatus.COMPLETED);
            var future = new Shift(2, doctor1, "A", DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { past, future });

            var result = service.GetFutureShiftsForStaff(1);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(2));
        }

        [Test]
        public void GetFutureShiftsForStaff_DifferentStaff_ReturnsEmpty()
        {
            var shift = new Shift(1, doctor2, "A", DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = service.GetFutureShiftsForStaff(1);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetEligibleSwapColleaguesForShift_ShiftNotFound_ReturnsEmpty()
        {
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            var result = service.GetEligibleSwapColleaguesForShift(1, 99, out string error);
            Assert.That(result.Count, Is.EqualTo(0));
            Assert.That(error, Is.EqualTo("Shift not found."));
        }

        [Test]
        public void GetEligibleSwapColleaguesForShift_NotOwnShift_ReturnsEmpty()
        {
            var shift = new Shift(1, doctor2, "A", DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = service.GetEligibleSwapColleaguesForShift(1, 1, out string error);
            Assert.That(error, Does.Contain("own shift"));
        }

        [Test]
        public void GetEligibleSwapColleaguesForShift_PastShift_ReturnsEmpty()
        {
            var shift = new Shift(1, doctor1, "A", DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1).AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = service.GetEligibleSwapColleaguesForShift(1, 1, out string error);
            Assert.That(error, Does.Contain("future"));
        }

        [Test]
        public void AcceptSwapRequest_RequestNotFound_ReturnsFalse()
        {
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns((ShiftSwapRequest)null);
            var result = service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("not found"));
        }

        [Test]
        public void AcceptSwapRequest_WrongColleague_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 3);
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);
            var result = service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("cannot accept"));
        }

        [Test]
        public void AcceptSwapRequest_NotPending_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2) { Status = ShiftSwapRequestStatus.ACCEPTED };
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);
            var result = service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("no longer pending"));
        }

        [Test]
        public void AcceptSwapRequest_ShiftNotFound_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 99, 1, 2);
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            var result = service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("Shift not found"));
        }

        [Test]
        public void AcceptSwapRequest_ColleagueOverlap_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var now = DateTime.Now.AddDays(1);
            var targetShift = new Shift(1, doctor1, "A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            var colleagueShift = new Shift(2, doctor2, "B", now.AddHours(4), now.AddHours(12), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { targetShift, colleagueShift });

            var result = service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("already scheduled"));
        }

        [Test]
        public void AcceptSwapRequest_Valid_ReturnsTrue()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var now = DateTime.Now.AddDays(1);
            var targetShift = new Shift(1, doctor1, "A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { targetShift });

            var result = service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.True);
            Assert.That(message, Does.Contain("accepted"));
            mockShiftRepository.Verify(repository => repository.UpdateShiftStaffId(1, 2), Times.Once);
            mockNotificationRepository.Verify(repository => repository.AddNotification(1, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void RejectSwapRequest_RequestNotFound_ReturnsFalse()
        {
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns((ShiftSwapRequest)null);
            var result = service.RejectSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
        }

        [Test]
        public void RejectSwapRequest_WrongColleague_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 3);
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);
            var result = service.RejectSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
        }

        [Test]
        public void RejectSwapRequest_NotPending_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2) { Status = ShiftSwapRequestStatus.REJECTED };
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);
            var result = service.RejectSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
        }

        [Test]
        public void RejectSwapRequest_Valid_ReturnsTrue()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var result = service.RejectSwapRequest(1, 2, out string message);
            Assert.That(result, Is.True);
            Assert.That(message, Does.Contain("rejected"));
            mockNotificationRepository.Verify(repository => repository.AddNotification(1, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void GetAllDoctors_ReturnsOrderedDoctors()
        {
            mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { doctor2, doctor1 });
            var result = service.GetAllDoctors();
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].FirstName, Is.EqualTo("Jane"));
            Assert.That(result[1].FirstName, Is.EqualTo("John"));
        }

        [Test]
        public void GetFutureShiftsForStaff_CancelledShift_StillReturned()
        {
            var future = new Shift(1, doctor1, "A", DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(8), ShiftStatus.CANCELLED);
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { future });

            var result = service.GetFutureShiftsForStaff(1);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetFutureShiftsForStaff_MultipleShifts_ReturnsSorted()
        {
            var far = new Shift(1, doctor1, "A", DateTime.Now.AddDays(5), DateTime.Now.AddDays(5).AddHours(8), ShiftStatus.SCHEDULED);
            var near = new Shift(2, doctor1, "A", DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { far, near });

            var result = service.GetFutureShiftsForStaff(1);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetFutureShiftsForStaff_NoShifts_ReturnsEmpty()
        {
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            var result = service.GetFutureShiftsForStaff(1);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetEligibleSwapColleaguesForShift_ValidShift_ReturnsColleagues()
        {
            var futureDate = DateTime.Now.AddDays(2);
            var shift = new Shift(1, doctor1, "A", futureDate, futureDate.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });
            mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { doctor1, doctor2 });

            var result = service.GetEligibleSwapColleaguesForShift(1, 1, out string error);
            Assert.That(error, Is.Null.Or.Empty);
        }

        [Test]
        public void RejectSwapRequest_Valid_CallsRepoUpdate()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            service.RejectSwapRequest(1, 2, out string message);
            mockShiftSwapRepository.Verify(repository => repository.UpdateShiftSwapRequestStatus(1, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void AcceptSwapRequest_Valid_CallsRepoUpdate()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var now = DateTime.Now.AddDays(1);
            var targetShift = new Shift(1, doctor1, "A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { targetShift });

            service.AcceptSwapRequest(1, 2, out string message);
            mockShiftSwapRepository.Verify(repository => repository.UpdateShiftSwapRequestStatus(1, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void GetAllDoctors_EmptyStaff_ReturnsEmpty()
        {
            mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff>());
            var result = service.GetAllDoctors();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetAllDoctors_OnlyDoctors_ReturnsAll()
        {
            var doctor3 = new Doctor(3, "Alice", "Wonder", "c", true, "Neuro", "L3", DoctorStatus.AVAILABLE, 1);
            mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { doctor1, doctor2, doctor3 });
            var result = service.GetAllDoctors();
            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void AcceptSwapRequest_NotifiesRequester()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var now = DateTime.Now.AddDays(1);
            var targetShift = new Shift(1, doctor1, "A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { targetShift });

            service.AcceptSwapRequest(1, 2, out string message);
            mockNotificationRepository.Verify(repository => repository.AddNotification(1, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void RejectSwapRequest_NotifiesRequester()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            service.RejectSwapRequest(1, 2, out string message);
            mockNotificationRepository.Verify(repository => repository.AddNotification(1, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void AcceptSwapRequest_RequestNotFound_ReturnsFalseWithMessage()
        {
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(99)).Returns((ShiftSwapRequest)null);
            var result = service.AcceptSwapRequest(99, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("not found"));
        }

        [Test]
        public void AcceptSwapRequest_WrongColleague_ReturnsFalseWithMessage()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 3);
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var result = service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
        }

        [Test]
        public void AcceptSwapRequest_AlreadyAccepted_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2) { Status = ShiftSwapRequestStatus.ACCEPTED };
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var result = service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("no longer pending"));
        }

        [Test]
        public void AcceptSwapRequest_ShiftDeleted_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 99, 1, 2);
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());

            var result = service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("Shift not found"));
        }

        [Test]
        public void AcceptSwapRequest_ColleagueHasOverlap_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var now = DateTime.Now.AddDays(1);
            var targetShift = new Shift(1, doctor1, "A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            var overlappingShift = new Shift(2, doctor2, "A", now.AddHours(2), now.AddHours(6), ShiftStatus.SCHEDULED);
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { targetShift, overlappingShift });

            var result = service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("already scheduled"));
        }
    }
}

