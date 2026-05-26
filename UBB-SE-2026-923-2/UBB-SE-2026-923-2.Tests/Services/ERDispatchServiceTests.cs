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

        [Test]
        public async Task DispatchERRequestAsync_SurgeonRequest_MatchesSurgeryDoctor()
        {
            var pendingRequest = new ERRequest { Id = 1, Status = "PENDING", Specialization = "Surgeon", Location = "Ward A", CreatedAt = DateTime.Now };
            var doctor = new Doctor(42, "Nora", "Avram", "0712000042", true, "Surgery", "MD-SURG-4242", DoctorStatus.AVAILABLE, 9);
            var shift = new Shift(1, doctor, "Ward A", DateTime.Now.AddMinutes(-15), DateTime.Now.AddHours(2), ShiftStatus.ACTIVE);

            this.mockRequestRepository.Setup(repository => repository.GetAllRequests()).Returns(new List<ERRequest> { pendingRequest });
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { doctor });
            this.mockStaffRepository.Setup(repository => repository.UpdateStatusAsync(42, DoctorStatus.IN_EXAMINATION.ToString())).Returns(Task.CompletedTask);

            var result = await this.service.DispatchERRequestAsync(1);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.MatchedDoctorId, Is.EqualTo(42));
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

        [Test]
        public async Task GetManualOverrideCandidatesAsync_SurgeonRequest_ReturnsSurgeryDoctor()
        {
            var existingRequest = new ERRequest { Id = 1, Status = "PENDING", Specialization = "Surgeon", Location = "Ward A" };
            var doctor = new Doctor(42, "Nora", "Avram", "0712000042", true, "Surgery", "MD-SURG-4242", DoctorStatus.IN_EXAMINATION, 9);
            var shift = new Shift(1, doctor, "Surgery Bay", DateTime.Now.AddMinutes(-15), DateTime.Now.AddDays(1), ShiftStatus.ACTIVE);

            this.mockRequestRepository.Setup(repository => repository.GetRequestById(1)).Returns(existingRequest);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { doctor });

            var result = await this.service.GetManualOverrideCandidatesAsync(1, 3 * 24 * 60);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].DoctorId, Is.EqualTo(42));
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

        // --- GetAllRequestsAsync (web slice) ---
        [Test]
        public async Task GetAllRequestsAsync_ReturnsEveryRequestFromRepository()
        {
            this.mockRequestRepository.Setup(repository => repository.GetAllRequests())
                .Returns(new List<ERRequest>
                {
                    new ERRequest { Id = 1, Status = "PENDING" },
                    new ERRequest { Id = 2, Status = "ASSIGNED" },
                });

            var result = await this.service.GetAllRequestsAsync();

            Assert.That(result.Count, Is.EqualTo(2));
        }

        // --- GetRequestByIdAsync (web slice) ---
        [Test]
        public async Task GetRequestByIdAsync_ExistingId_ReturnsThatRequest()
        {
            this.mockRequestRepository.Setup(repository => repository.GetRequestById(7))
                .Returns(new ERRequest { Id = 7, Specialization = "Cardiology" });

            var result = await this.service.GetRequestByIdAsync(7);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(7));
        }

        [Test]
        public async Task GetRequestByIdAsync_MissingId_ReturnsNull()
        {
            this.mockRequestRepository.Setup(repository => repository.GetRequestById(99))
                .Returns((ERRequest)null);

            var result = await this.service.GetRequestByIdAsync(99);

            Assert.That(result, Is.Null);
        }

        // --- CreateRequestAsync (web slice) ---
        [Test]
        public async Task CreateRequestAsync_CreatesPendingRequestAndReturnsNewId()
        {
            this.mockRequestRepository
                .Setup(repository => repository.AddRequest("Neurology", "Ward B", "PENDING"))
                .Returns(42);

            var newId = await this.service.CreateRequestAsync("Neurology", "Ward B");

            Assert.That(newId, Is.EqualTo(42));
            this.mockRequestRepository.Verify(
                repository => repository.AddRequest("Neurology", "Ward B", "PENDING"),
                Times.Once);
        }

        // --- UpdateRequestStatusAsync (web slice: Edit / soft-cancel) ---
        [Test]
        public async Task UpdateRequestStatusAsync_DelegatesToRepositoryWithoutDoctor()
        {
            await this.service.UpdateRequestStatusAsync(3, "CANCELLED");

            this.mockRequestRepository.Verify(
                repository => repository.UpdateRequestStatus(3, "CANCELLED", null, null),
                Times.Once);
        }

        [Test]
        public async Task UpdateRequestStatusAsync_ReopenToPending_DelegatesToRepository()
        {
            await this.service.UpdateRequestStatusAsync(3, "PENDING");

            this.mockRequestRepository.Verify(
                repository => repository.UpdateRequestStatus(3, "PENDING", null, null),
                Times.Once);
        }

        [TestCase("ASSIGNED")]
        [TestCase("UNMATCHED")]
        [TestCase("bogus")]
        public void UpdateRequestStatusAsync_EngineOwnedOrUnknownStatus_ThrowsAndDoesNotPersist(string status)
        {
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await this.service.UpdateRequestStatusAsync(3, status));

            this.mockRequestRepository.Verify(
                repository => repository.UpdateRequestStatus(
                    It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>()),
                Times.Never);
        }

        // --- DispatchAllPendingAsync (web slice: Run Dispatch button) ---
        [Test]
        public async Task DispatchAllPendingAsync_TwoPendingRequests_ReturnsOneResultPerRequest()
        {
            this.mockRequestRepository.Setup(repository => repository.GetAllRequests())
                .Returns(new List<ERRequest>
                {
                    new ERRequest { Id = 1, Status = "PENDING", Specialization = "Cardiology", Location = "Ward A", CreatedAt = DateTime.Now },
                    new ERRequest { Id = 2, Status = "PENDING", Specialization = "Neurology", Location = "Ward B", CreatedAt = DateTime.Now },
                });
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff>());

            var results = await this.service.DispatchAllPendingAsync();

            Assert.That(results.Count, Is.EqualTo(2));
        }
    }
}
