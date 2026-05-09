namespace UBB_SE_2026_923_2.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Moq;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class ERDispatchServiceLogicTests
    {
        private Mock<IERDispatchRepository> mockERDispatchRepository;
        private Mock<IStaffRepository> mockStaffRepository;
        private Mock<IShiftRepository> mockShiftRepository;
        private Mock<INotificationRepository> mockNotificationRepository;
        private ERDispatchService erDispatchService;

        [SetUp]
        public void Setup()
        {
            this.mockERDispatchRepository = new Mock<IERDispatchRepository>();
            this.mockStaffRepository = new Mock<IStaffRepository>();
            this.mockShiftRepository = new Mock<IShiftRepository>();
            this.mockNotificationRepository = new Mock<INotificationRepository>();

            this.erDispatchService = new ERDispatchService(
                this.mockERDispatchRepository.Object,
                this.mockStaffRepository.Object,
                this.mockShiftRepository.Object,
                this.mockNotificationRepository.Object);
        }

        [Test]
        public async Task SimulateIncomingRequestsAsync_WhenRequestedCountIsZero_CreatesOneRequest()
        {
            SetupRosterWithAvailableDoctor(1, "John", "Doe", "Cardiology", "Ward A");

            this.mockERDispatchRepository
                .Setup(erDispatchRepository => erDispatchRepository.AddRequest(It.IsAny<string>(), It.IsAny<string>(), "PENDING"))
                .Returns(100);

            var createdRequestIdentifiers = await this.erDispatchService.SimulateIncomingRequestsAsync(0);

            Assert.That(createdRequestIdentifiers.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetPendingRequestIdsAsync_WhenRequestsHaveMixedStatuses_ReturnsOnlyPendingRequestIdentifiers()
        {
            this.mockERDispatchRepository
                .Setup(erDispatchRepository => erDispatchRepository.GetAllRequests())
                .Returns(new List<ERRequest>
                {
                    CreateRequest(1, "PENDING", "Cardiology", "Ward A"),
                    CreateRequest(2, "ASSIGNED", "Cardiology", "Ward A"),
                });

            var pendingRequestIdentifiers = await this.erDispatchService.GetPendingRequestIdsAsync();

            Assert.That(pendingRequestIdentifiers.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task DispatchERRequestAsync_WhenRequestDoesNotExist_ReturnsFailedResult()
        {
            this.mockERDispatchRepository
                .Setup(erDispatchRepository => erDispatchRepository.GetAllRequests())
                .Returns(new List<ERRequest>());

            var dispatchResult = await this.erDispatchService.DispatchERRequestAsync(99);

            Assert.That(dispatchResult.IsSuccess, Is.False);
        }

        [Test]
        public async Task DispatchERRequestAsync_WhenMatchingAvailableDoctorExists_ReturnsMatchedDoctorIdentifier()
        {
            SetupRosterWithAvailableDoctor(10, "Alice", "Smith", "Cardiology", "Ward A");

            this.mockERDispatchRepository
                .Setup(erDispatchRepository => erDispatchRepository.GetAllRequests())
                .Returns(new List<ERRequest>
                {
                    CreateRequest(1, "PENDING", "Cardiology", "Ward A"),
                });

            var dispatchResult = await this.erDispatchService.DispatchERRequestAsync(1);

            Assert.That(dispatchResult.MatchedDoctorId, Is.EqualTo(10));
        }

        [Test]
        public async Task DispatchERRequestAsync_WhenNoMatchingAvailableDoctorExists_MarksRequestAsUnmatched()
        {
            SetupRosterWithAvailableDoctor(10, "Alice", "Smith", "Neurology", "Ward A");

            this.mockERDispatchRepository
                .Setup(erDispatchRepository => erDispatchRepository.GetAllRequests())
                .Returns(new List<ERRequest>
                {
                    CreateRequest(1, "PENDING", "Cardiology", "Ward A"),
                });

            await this.erDispatchService.DispatchERRequestAsync(1);

            this.mockERDispatchRepository.Verify(
                erDispatchRepository => erDispatchRepository.UpdateRequestStatus(1, "UNMATCHED", null, null),
                Times.Once);
        }

        [Test]
        public async Task GetManualOverrideCandidatesAsync_WhenRequestDoesNotExist_ReturnsEmptyCandidateList()
        {
            this.mockERDispatchRepository
                .Setup(erDispatchRepository => erDispatchRepository.GetRequestById(99))
                .Returns((ERRequest)null);

            var overrideCandidates = await this.erDispatchService.GetManualOverrideCandidatesAsync(99, 30);

            Assert.That(overrideCandidates.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetManualOverrideCandidatesAsync_WhenDoctorInExaminationIsNearShiftEnd_ReturnsDoctorCandidate()
        {
            var doctorNearShiftEnd = CreateDoctor(10, "Alice", "Smith", "Cardiology", DoctorStatus.IN_EXAMINATION);
            var currentShift = new Shift(1, doctorNearShiftEnd, "Ward A", DateTime.Now.AddHours(-7), DateTime.Now.AddMinutes(20), ShiftStatus.ACTIVE);

            this.mockERDispatchRepository
                .Setup(erDispatchRepository => erDispatchRepository.GetRequestById(1))
                .Returns(CreateRequest(1, "PENDING", "Cardiology", "Ward A"));

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff> { doctorNearShiftEnd });

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift> { currentShift });

            var overrideCandidates = await this.erDispatchService.GetManualOverrideCandidatesAsync(1, 30);

            Assert.That(overrideCandidates.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task ManualOverrideAsync_WhenDoctorIsNotEligible_ReturnsFailedResult()
        {
            var availableDoctor = CreateDoctor(10, "Alice", "Smith", "Cardiology", DoctorStatus.AVAILABLE);
            var currentShift = new Shift(1, availableDoctor, "Ward A", DateTime.Now.AddHours(-1), DateTime.Now.AddHours(7), ShiftStatus.ACTIVE);

            this.mockERDispatchRepository
                .Setup(erDispatchRepository => erDispatchRepository.GetRequestById(1))
                .Returns(CreateRequest(1, "PENDING", "Cardiology", "Ward A"));

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff> { availableDoctor });

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift> { currentShift });

            var overrideResult = await this.erDispatchService.ManualOverrideAsync(1, 10, 30);

            Assert.That(overrideResult.IsSuccess, Is.False);
        }

        [Test]
        public async Task ManualOverrideAsync_WhenDoctorIsEligible_ReturnsSuccessfulResult()
        {
            var doctorNearShiftEnd = CreateDoctor(10, "Alice", "Smith", "Cardiology", DoctorStatus.IN_EXAMINATION);
            var currentShift = new Shift(1, doctorNearShiftEnd, "Ward A", DateTime.Now.AddHours(-7), DateTime.Now.AddMinutes(20), ShiftStatus.ACTIVE);

            this.mockERDispatchRepository
                .Setup(erDispatchRepository => erDispatchRepository.GetRequestById(1))
                .Returns(CreateRequest(1, "PENDING", "Cardiology", "Ward A"));

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff> { doctorNearShiftEnd });

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift> { currentShift });

            var overrideResult = await this.erDispatchService.ManualOverrideAsync(1, 10, 30);

            Assert.That(overrideResult.IsSuccess, Is.True);
        }

        private void SetupRosterWithAvailableDoctor(int doctorIdentifier, string firstName, string lastName, string specialization, string location)
        {
            var availableDoctor = CreateDoctor(doctorIdentifier, firstName, lastName, specialization, DoctorStatus.AVAILABLE);
            var currentShift = new Shift(1, availableDoctor, location, DateTime.Now.AddHours(-1), DateTime.Now.AddHours(7), ShiftStatus.ACTIVE);

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff> { availableDoctor });

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift> { currentShift });
        }

        private static Doctor CreateDoctor(int doctorIdentifier, string firstName, string lastName, string specialization, DoctorStatus doctorStatus)
        {
            return new Doctor(doctorIdentifier, firstName, lastName, "contract", true, specialization, "License", doctorStatus, 5);
        }

        private static ERRequest CreateRequest(int requestIdentifier, string status, string specialization, string location)
        {
            return new ERRequest
            {
                Id = requestIdentifier,
                Status = status,
                Specialization = specialization,
                Location = location,
                CreatedAt = DateTime.Now,
            };
        }
    }
}