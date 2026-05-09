namespace UBB_SE_2026_923_2.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Moq;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class ERDispatchServiceTests
    {
        private Mock<IERDispatchRepository> mockRequestRepository;
        private Mock<IStaffRepository> mockStaffRepository;
        private Mock<IShiftRepository> mockShiftRepository;
        private Mock<INotificationRepository> mockNotificationRepository;
        private ERDispatchService service;

        [SetUp]
        public void Setup()
        {
            this.mockRequestRepository = new Mock<IERDispatchRepository>();
            this.mockStaffRepository = new Mock<IStaffRepository>();
            this.mockShiftRepository = new Mock<IShiftRepository>();
            this.mockNotificationRepository = new Mock<INotificationRepository>();
            this.service = new ERDispatchService(
                this.mockRequestRepository.Object,
                this.mockStaffRepository.Object,
                this.mockShiftRepository.Object,
                this.mockNotificationRepository.Object);
        }

        // --- SimulateIncomingRequestsAsync ---
        [Test]
        public async Task SimulateIncomingRequestsAsync_PositiveCount_CreatesRequests()
        {
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff>());
            this.mockRequestRepository.Setup(repository => repository.AddRequest(It.IsAny<string>(), It.IsAny<string>(), "PENDING")).Returns(1);

            var result = await this.service.SimulateIncomingRequestsAsync(2);

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task SimulateIncomingRequestsAsync_ZeroCount_CreatesAtLeastOne()
        {
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff>());
            this.mockRequestRepository.Setup(repository => repository.AddRequest(It.IsAny<string>(), It.IsAny<string>(), "PENDING")).Returns(1);

            var result = await this.service.SimulateIncomingRequestsAsync(0);

            Assert.That(result.Count, Is.GreaterThanOrEqualTo(1));
        }

        // --- GetPendingRequestIdsAsync ---
        [Test]
        public async Task GetPendingRequestIdsAsync_PendingRequestsExist_ReturnsIds()
        {
            var pendingRequest = new ERRequest { Id = 5, Status = "PENDING", CreatedAt = DateTime.Now };
            var assignedRequest = new ERRequest { Id = 6, Status = "ASSIGNED", CreatedAt = DateTime.Now };
            this.mockRequestRepository.Setup(repository => repository.GetAllRequests())
                .Returns(new List<ERRequest> { pendingRequest, assignedRequest });

            var result = await this.service.GetPendingRequestIdsAsync();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(5));
        }

        [Test]
        public async Task GetPendingRequestIdsAsync_NoPendingRequests_ReturnsEmptyList()
        {
            this.mockRequestRepository.Setup(repository => repository.GetAllRequests()).Returns(new List<ERRequest>());

            var result = await this.service.GetPendingRequestIdsAsync();

            Assert.That(result.Count, Is.EqualTo(0));
        }

        // --- DispatchERRequestAsync ---
        [Test]
        public async Task DispatchERRequestAsync_RequestNotFound_ReturnsFailure()
        {
            this.mockRequestRepository.Setup(repository => repository.GetAllRequests()).Returns(new List<ERRequest>());

            var result = await this.service.DispatchERRequestAsync(99);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("not found"));
        }

        [Test]
        public async Task DispatchERRequestAsync_NoMatchingDoctor_ReturnsUnmatched()
        {
            var pendingRequest = new ERRequest { Id = 1, Status = "PENDING", Specialization = "Cardiology", Location = "Ward A", CreatedAt = DateTime.Now };
            this.mockRequestRepository.Setup(repository => repository.GetAllRequests()).Returns(new List<ERRequest> { pendingRequest });
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff>());

            var result = await this.service.DispatchERRequestAsync(1);

            Assert.That(result.IsSuccess, Is.False);
        }

        // --- GetManualOverrideCandidatesAsync ---
        [Test]
        public async Task GetManualOverrideCandidatesAsync_RequestNotFound_ReturnsEmptyList()
        {
            this.mockRequestRepository.Setup(repository => repository.GetRequestById(99)).Returns((ERRequest)null);

            var result = await this.service.GetManualOverrideCandidatesAsync(99, 30);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetManualOverrideCandidatesAsync_NoDoctorsInExamination_ReturnsEmptyList()
        {
            var existingRequest = new ERRequest { Id = 1, Status = "PENDING", Specialization = "Cardiology", Location = "Ward A" };
            this.mockRequestRepository.Setup(repository => repository.GetRequestById(1)).Returns(existingRequest);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff>());

            var result = await this.service.GetManualOverrideCandidatesAsync(1, 30);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        // --- ManualOverrideAsync ---
        [Test]
        public async Task ManualOverrideAsync_RequestNotFound_ReturnsFailure()
        {
            this.mockRequestRepository.Setup(repository => repository.GetRequestById(99)).Returns((ERRequest)null);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff>());

            var result = await this.service.ManualOverrideAsync(99, 1, 30);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("not found"));
        }

        [Test]
        public async Task ManualOverrideAsync_DoctorNotFound_ReturnsFailure()
        {
            var existingRequest = new ERRequest { Id = 1, Status = "PENDING", Specialization = "Cardiology", Location = "Ward A" };
            this.mockRequestRepository.Setup(repository => repository.GetRequestById(1)).Returns(existingRequest);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff>());

            var result = await this.service.ManualOverrideAsync(1, 999, 30);

            Assert.That(result.IsSuccess, Is.False);
        }
    }
}
