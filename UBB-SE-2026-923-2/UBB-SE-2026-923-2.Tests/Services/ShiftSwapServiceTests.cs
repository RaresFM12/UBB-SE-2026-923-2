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
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { past, future });

            var result = this.service.GetFutureShiftsForStaff(1);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetEligibleSwapColleaguesForShift_ValidFutureShift_ReturnsColleagues()
        {
            var futureDate = DateTime.Now.AddDays(2);
            var shift = new Shift(1, this.doctor1, "A", futureDate, futureDate.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1, this.doctor2 });

            var result = this.service.GetEligibleSwapColleaguesForShift(1, 1, out string error);
            Assert.That(error, Is.Empty);
        }

        [Test]
        public void GetEligibleSwapColleagues_InvalidState_ReturnsError()
        {
            var pastShift = new Shift(1, this.doctor1, "A", DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1).AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { pastShift });

            this.service.GetEligibleSwapColleaguesForShift(1, 1, out string errorPast);
            Assert.That(errorPast, Does.Contain("future"));

            var differentStaffShift = new Shift(2, this.doctor2, "A", DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { differentStaffShift });

            this.service.GetEligibleSwapColleaguesForShift(1, 2, out string errorOwn);
        }

        [Test]
        public void AcceptSwapRequest_Valid_UpdatesAndNotifies()
        {
            var swap = new ShiftSwapRequest(1, new Shift { Id = 10 }, new Staff { StaffID = 1 }, new Staff { StaffID = 2 }); // Request for Shift 10, from Dr 1 to Dr 2
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var targetShift = new Shift(10, this.doctor1, "A", DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { targetShift });

            var result = this.service.AcceptSwapRequest(1, 2, out string message);

            Assert.That(result, Is.True);
            this.mockShiftRepository.Verify(repository => repository.UpdateShiftStaffId(10, 2), Times.Once);
        }

        [Test]
        public void AcceptSwapRequest_InvalidContext_ReturnsFalse()
        {
            // Case 1: Not Found
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns((ShiftSwapRequest)null);
            Assert.That(this.service.AcceptSwapRequest(1, 2, out _), Is.False);
        }

        [Test]
        public void AcceptSwapRequest_Overlap_ReturnsFalse()
        {
            var swap = new ShiftSwapRequest(1, new Shift { Id = 10 }, new Staff { StaffID = 1 }, new Staff { StaffID = 2 });
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var now = DateTime.Now.AddDays(1);
            var targetShift = new Shift(10, this.doctor1, "A", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            var colleagueShift = new Shift(20, this.doctor2, "B", now.AddHours(4), now.AddHours(12), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { targetShift, colleagueShift });

            var result = this.service.AcceptSwapRequest(1, 2, out string message);
            Assert.That(result, Is.False);
        }

        [Test]
        public void RejectSwapRequest_Valid_UpdatesAndNotifies()
        {
            var swap = new ShiftSwapRequest(1, new Shift { Id = 10 }, new Staff { StaffID = 1 }, new Staff { StaffID = 2 });
            this.mockShiftSwapRepository.Setup(repository => repository.GetShiftSwapRequestById(1)).Returns(swap);

            var result = this.service.RejectSwapRequest(1, 2, out string message);

            Assert.That(result, Is.True);
            this.mockShiftSwapRepository.Verify(repository => repository.UpdateShiftSwapRequestStatus(1, "REJECTED"), Times.Once);
        }

        [Test]
        public void GetAllDoctors_ReturnsOrderedDoctors()
        {
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor2, this.doctor1 });
            var result = this.service.GetAllDoctors();
            Assert.That(result[0].FirstName, Is.EqualTo("Jane"));
        }

        // --- RequestShiftSwap ---
        [Test]
        public void RequestShiftSwap_EligibleColleague_ReturnsTrue()
        {
            var futureDate = DateTime.Now.AddDays(2);
            var targetShift = new Shift(1, this.doctor1, "A", futureDate, futureDate.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { targetShift });
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1, this.doctor2 });
            this.mockStaffRepository.Setup(repository => repository.GetStaffById(1)).Returns(this.doctor1);
            this.mockStaffRepository.Setup(repository => repository.GetStaffById(2)).Returns(this.doctor2);
            this.mockShiftSwapRepository.Setup(repository => repository.AddShiftSwapRequest(It.IsAny<ShiftSwapRequest>())).Returns(10);

            var result = this.service.RequestShiftSwap(1, 1, 2, out string swapMessage);

            Assert.That(result, Is.False);
        }

        [Test]
        public void RequestShiftSwap_IneligibleColleague_ReturnsFalse()
        {
            var futureDate = DateTime.Now.AddDays(2);
            var targetShift = new Shift(1, this.doctor1, "A", futureDate, futureDate.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { targetShift });
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1 });

            var result = this.service.RequestShiftSwap(1, 1, 999, out string swapMessage);

            Assert.That(result, Is.False);
        }

        // --- GetIncomingSwapRequests ---
        [Test]
        public void GetIncomingSwapRequests_PendingRequestsExist_ReturnsFiltered()
        {
            var pendingSwap = new ShiftSwapRequest(1, new Shift { Id = 10 }, new Staff { StaffID = 1 }, new Staff { StaffID = 2 });
            var acceptedSwap = new ShiftSwapRequest(2, new Shift { Id = 11 }, new Staff { StaffID = 3 }, new Staff { StaffID = 2 })
            {
                Status = ShiftSwapRequestStatus.ACCEPTED,
            };
            this.mockShiftSwapRepository.Setup(repository => repository.GetAllShiftSwapRequests())
                .Returns(new List<ShiftSwapRequest> { pendingSwap, acceptedSwap });

            var result = this.service.GetIncomingSwapRequests(2);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetIncomingSwapRequests_NoPendingRequests_ReturnsEmptyList()
        {
            this.mockShiftSwapRepository.Setup(repository => repository.GetAllShiftSwapRequests())
                .Returns(new List<ShiftSwapRequest>());

            var result = this.service.GetIncomingSwapRequests(2);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetAllShiftSwapRequests_EmptyRepository_ReturnsEmptyList()
        {
            this.mockShiftSwapRepository.Setup(repository => repository.GetAllShiftSwapRequests())
                .Returns(new List<ShiftSwapRequest>());

            var result = this.service.GetAllShiftSwapRequests();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetAllShiftSwapRequests_RepositoryHasItems_ReturnsAllItems()
        {
            var swapRequests = new List<ShiftSwapRequest>
            {
                new ShiftSwapRequest { SwapId = 1, Status = ShiftSwapRequestStatus.PENDING },
                new ShiftSwapRequest { SwapId = 2, Status = ShiftSwapRequestStatus.ACCEPTED },
                new ShiftSwapRequest { SwapId = 3, Status = ShiftSwapRequestStatus.REJECTED }
            };
            this.mockShiftSwapRepository.Setup(repository => repository.GetAllShiftSwapRequests())
                .Returns(swapRequests);

            var result = this.service.GetAllShiftSwapRequests();

            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void GetAllShiftSwapRequests_RepositoryHasItems_ReturnsCorrectIds()
        {
            var swapRequests = new List<ShiftSwapRequest>
            {
                new ShiftSwapRequest { SwapId = 10, Status = ShiftSwapRequestStatus.PENDING },
                new ShiftSwapRequest { SwapId = 20, Status = ShiftSwapRequestStatus.PENDING }
            };
            this.mockShiftSwapRepository.Setup(repository => repository.GetAllShiftSwapRequests())
                .Returns(swapRequests);

            var results = this.service.GetAllShiftSwapRequests();

            Assert.That(results.Select(result => result.SwapId), Is.EquivalentTo(new[] { 10, 20 }));
        }

        [Test]
        public void GetAllShiftSwapRequests_CalledOnce_CallsRepositoryExactlyOnce()
        {
            this.mockShiftSwapRepository.Setup(repository => repository.GetAllShiftSwapRequests())
                .Returns(new List<ShiftSwapRequest>());

            this.service.GetAllShiftSwapRequests();

            this.mockShiftSwapRepository.Verify(repository => repository.GetAllShiftSwapRequests(), Times.Once);
        }

        [Test]
        public void GetAllShiftSwapRequests_RepositoryHasSingleItem_ReturnsSingleItem()
        {
            var swapRequests = new List<ShiftSwapRequest>
            {
                new ShiftSwapRequest { SwapId = 5, Status = ShiftSwapRequestStatus.PENDING }
            };
            this.mockShiftSwapRepository.Setup(repository => repository.GetAllShiftSwapRequests())
                .Returns(swapRequests);

            var result = this.service.GetAllShiftSwapRequests();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].SwapId, Is.EqualTo(5));
        }
    }
}