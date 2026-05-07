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
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { past, future });

            var result = this.service.GetFutureShiftsForStaff(1);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(2));
        }

        [Test]
        public void GetFutureShiftsForStaff_DifferentStaff_ReturnsEmpty()
        {
            var shift = new Shift(1, this.doctor2, "A", DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = this.service.GetFutureShiftsForStaff(1);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetEligibleSwapColleaguesForShift_ShiftNotFound_ReturnsEmpty()
        {
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            var result = this.service.GetEligibleSwapColleaguesForShift(1, 99, out string error);
            Assert.That(result.Count, Is.EqualTo(0));
            Assert.That(error, Is.EqualTo("Shift not found."));
        }

        [Test]
        public void GetEligibleSwapColleaguesForShift_NotOwnShift_ReturnsEmpty()
        {
            var shift = new Shift(1, this.doctor2, "A", DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = this.service.GetEligibleSwapColleaguesForShift(1, 1, out string error);
            Assert.That(error, Does.Contain("own shift"));
        }

        [Test]
        public void GetEligibleSwapColleaguesForShift_PastShift_ReturnsEmpty()
        {
            var shift = new Shift(1, this.doctor1, "A", DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1).AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = this.service.GetEligibleSwapColleaguesForShift(1, 1, out string error);
            Assert.That(error, Does.Contain("future"));
        }

        [Test]
        public void AcceptSwapRequest_RequestNotFound_ReturnsFalse()
        {
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns((ShiftSwapRequest)null);
            var result = this.service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("not found"));
        }

        [Test]
        public void AcceptSwapRequest_WrongColleague_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 3);
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);
            var result = this.service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("cannot accept"));
        }

        [Test]
        public void AcceptSwapRequest_NotPending_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2) { Status = ShiftSwapRequestStatus.ACCEPTED };
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);
            var result = this.service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("no longer pending"));
        }

        [Test]
        public void AcceptSwapRequest_ShiftNotFound_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 99, 1, 2);
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            var result = this.service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("Shift not found"));
        }

        [Test]
        public void AcceptSwapRequest_ColleagueOverlap_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var now = DateTime.Now.AddDays(1);
            var targetShift = new Shift(1, this.doctor1, "A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            var colleagueShift = new Shift(2, this.doctor2, "B", now.AddHours(4), now.AddHours(12), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { targetShift, colleagueShift });

            var result = this.service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("already scheduled"));
        }

        [Test]
        public void AcceptSwapRequest_Valid_ReturnsTrue()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var now = DateTime.Now.AddDays(1);
            var targetShift = new Shift(1, this.doctor1, "A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { targetShift });

            var result = this.service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.True);
            Assert.That(message, Does.Contain("accepted"));
            this.mockShiftRepository.Verify(repository => repository.UpdateShiftStaffId(1, 2), Times.Once);
            this.mockNotificationRepository.Verify(repository => repository.AddNotification(1, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void RejectSwapRequest_RequestNotFound_ReturnsFalse()
        {
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns((ShiftSwapRequest)null);
            var result = this.service.RejectSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
        }

        [Test]
        public void RejectSwapRequest_WrongColleague_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 3);
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);
            var result = this.service.RejectSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
        }

        [Test]
        public void RejectSwapRequest_NotPending_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2) { Status = ShiftSwapRequestStatus.REJECTED };
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);
            var result = this.service.RejectSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
        }

        [Test]
        public void RejectSwapRequest_Valid_ReturnsTrue()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var result = this.service.RejectSwapRequest(1, 2, out string message);
            Assert.That(result, Is.True);
            Assert.That(message, Does.Contain("rejected"));
            this.mockNotificationRepository.Verify(repository => repository.AddNotification(1, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void GetAllDoctors_ReturnsOrderedDoctors()
        {
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor2, this.doctor1 });
            var result = this.service.GetAllDoctors();
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].FirstName, Is.EqualTo("Jane"));
            Assert.That(result[1].FirstName, Is.EqualTo("John"));
        }

        [Test]
        public void GetFutureShiftsForStaff_CancelledShift_StillReturned()
        {
            var future = new Shift(1, this.doctor1, "A", DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(8), ShiftStatus.CANCELLED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { future });

            var result = this.service.GetFutureShiftsForStaff(1);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetFutureShiftsForStaff_MultipleShifts_ReturnsSorted()
        {
            var far = new Shift(1, this.doctor1, "A", DateTime.Now.AddDays(5), DateTime.Now.AddDays(5).AddHours(8), ShiftStatus.SCHEDULED);
            var near = new Shift(2, this.doctor1, "A", DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { far, near });

            var result = this.service.GetFutureShiftsForStaff(1);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetFutureShiftsForStaff_NoShifts_ReturnsEmpty()
        {
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            var result = this.service.GetFutureShiftsForStaff(1);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetEligibleSwapColleaguesForShift_ValidShift_ReturnsColleagues()
        {
            var futureDate = DateTime.Now.AddDays(2);
            var shift = new Shift(1, this.doctor1, "A", futureDate, futureDate.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1, this.doctor2 });

            var result = this.service.GetEligibleSwapColleaguesForShift(1, 1, out string error);
            Assert.That(error, Is.Null.Or.Empty);
        }

        [Test]
        public void RejectSwapRequest_Valid_CallsRepoUpdate()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            this.service.RejectSwapRequest(1, 2, out string message);
            this.mockShiftSwapRepository.Verify(repository => repository.UpdateShiftSwapRequestStatus(1, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void AcceptSwapRequest_Valid_CallsRepoUpdate()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var now = DateTime.Now.AddDays(1);
            var targetShift = new Shift(1, this.doctor1, "A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { targetShift });

            this.service.AcceptSwapRequest(1, 2, out string message);
            this.mockShiftSwapRepository.Verify(repository => repository.UpdateShiftSwapRequestStatus(1, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void GetAllDoctors_EmptyStaff_ReturnsEmpty()
        {
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff>());
            var result = this.service.GetAllDoctors();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetAllDoctors_OnlyDoctors_ReturnsAll()
        {
            var doctor3 = new Doctor(3, "Alice", "Wonder", "c", true, "Neuro", "L3", DoctorStatus.AVAILABLE, 1);
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1, this.doctor2, doctor3 });
            var result = this.service.GetAllDoctors();
            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void AcceptSwapRequest_NotifiesRequester()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var now = DateTime.Now.AddDays(1);
            var targetShift = new Shift(1, this.doctor1, "A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { targetShift });

            this.service.AcceptSwapRequest(1, 2, out string message);
            this.mockNotificationRepository.Verify(repository => repository.AddNotification(1, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void RejectSwapRequest_NotifiesRequester()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            this.service.RejectSwapRequest(1, 2, out string message);
            this.mockNotificationRepository.Verify(repository => repository.AddNotification(1, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void AcceptSwapRequest_RequestNotFound_ReturnsFalseWithMessage()
        {
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(99)).Returns((ShiftSwapRequest)null);
            var result = this.service.AcceptSwapRequest(99, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("not found"));
        }

        [Test]
        public void AcceptSwapRequest_WrongColleague_ReturnsFalseWithMessage()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 3);
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var result = this.service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
        }

        [Test]
        public void AcceptSwapRequest_AlreadyAccepted_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2) { Status = ShiftSwapRequestStatus.ACCEPTED };
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var result = this.service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("no longer pending"));
        }

        [Test]
        public void AcceptSwapRequest_ShiftDeleted_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 99, 1, 2);
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());

            var result = this.service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("Shift not found"));
        }

        [Test]
        public void AcceptSwapRequest_ColleagueHasOverlap_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, 1, 1, 2);
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var now = DateTime.Now.AddDays(1);
            var targetShift = new Shift(1, this.doctor1, "A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            var overlappingShift = new Shift(2, this.doctor2, "A", now.AddHours(2), now.AddHours(6), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { targetShift, overlappingShift });

            var result = this.service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("already scheduled"));
        }
    }
}
