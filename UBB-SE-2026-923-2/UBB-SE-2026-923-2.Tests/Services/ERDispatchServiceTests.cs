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
    public class ERDispatchServiceTests
    {
        private Mock<IERDispatchRepository> mockRequestRepo;
        private Mock<IStaffRepository> mockStaffRepo;
        private Mock<IShiftRepository> mockShiftRepo;
        private Mock<INotificationRepository> mockNotificationRepo;
        private ERDispatchService service;

        [SetUp]
        public void Setup()
        {
            this.mockRequestRepo = new Mock<IERDispatchRepository>();
            this.mockStaffRepo = new Mock<IStaffRepository>();
            this.mockShiftRepo = new Mock<IShiftRepository>();
            this.mockNotificationRepo = new Mock<INotificationRepository>();

            this.service = new ERDispatchService(
                this.mockRequestRepo.Object,
                this.mockStaffRepo.Object,
                this.mockShiftRepo.Object,
                this.mockNotificationRepo.Object);
        }

        private (Doctor, Shift) CreateActiveDoctor(int id, string spec, string loc, DoctorStatus status)
        {
            var doctor = new Doctor(id, "Dr", "House", "contact", true, spec, "L1", status, 10);
            var shift = new Shift(id, doctor, loc, DateTime.Now.AddHours(-2), DateTime.Now.AddHours(6), ShiftStatus.ACTIVE);
            return (doctor, shift);
        }

        [Test]
        public async Task SimulateIncomingRequestsAsync_CallsRepositoryAdd()
        {
            // Setup a doctor so there's a template to follow
            var (doc, shift) = CreateActiveDoctor(10, "Surgeon", "Ward A", DoctorStatus.AVAILABLE);
            this.mockStaffRepo.Setup(s => s.LoadAllStaff()).Returns(new List<IStaff> { doc });
            this.mockShiftRepo.Setup(s => s.GetAllShifts()).Returns(new List<Shift> { shift });

            await this.service.SimulateIncomingRequestsAsync(1);

            this.mockRequestRepo.Verify(r => r.AddRequest(It.IsAny<string>(), It.IsAny<string>(), "PENDING"), Times.AtLeastOnce);
        }

        [Test]
        public async Task DispatchERRequestAsync_MatchingDoctorFound_UpdatesStatusAndNotifies()
        {
            // Arrange
            var request = new ERRequest { Id = 1, Specialization = "Surgeon", Location = "Ward A", Status = "PENDING" };
            this.mockRequestRepo.Setup(r => r.GetAllRequests()).Returns(new List<ERRequest> { request });

            var (doc, shift) = CreateActiveDoctor(10, "Surgeon", "Ward A", DoctorStatus.AVAILABLE);
            this.mockStaffRepo.Setup(s => s.LoadAllStaff()).Returns(new List<IStaff> { doc });
            this.mockShiftRepo.Setup(s => s.GetAllShifts()).Returns(new List<Shift> { shift });

            // Act
            var result = await this.service.DispatchERRequestAsync(1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            this.mockRequestRepo.Verify(r => r.UpdateRequestStatus(1, "ASSIGNED", 10, It.IsAny<string>()), Times.Once);
            this.mockStaffRepo.Verify(s => s.UpdateStatusAsync(10, "IN_EXAMINATION"), Times.Once);
            this.mockNotificationRepo.Verify(n => n.AddNotification(10, "ER Assignment", It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task DispatchERRequestAsync_NoMatchingDoctor_SetsUnmatchedStatus()
        {
            // Arrange
            var request = new ERRequest { Id = 1, Specialization = "Cardiology", Status = "PENDING" };
            this.mockRequestRepo.Setup(r => r.GetAllRequests()).Returns(new List<ERRequest> { request });

            // Only a Surgeon is available
            var (doc, shift) = CreateActiveDoctor(10, "Surgeon", "Ward A", DoctorStatus.AVAILABLE);
            this.mockStaffRepo.Setup(s => s.LoadAllStaff()).Returns(new List<IStaff> { doc });
            this.mockShiftRepo.Setup(s => s.GetAllShifts()).Returns(new List<Shift> { shift });

            // Act
            var result = await this.service.DispatchERRequestAsync(1);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            this.mockRequestRepo.Verify(r => r.UpdateRequestStatus(1, "UNMATCHED", null, null), Times.Once);
        }

        [Test]
        public async Task ManualOverride_CandidateNearEndOfShift_AllowsAssignment()
        {
            // Arrange
            var request = new ERRequest { Id = 1, Specialization = "Surgeon", Location = "Ward A" };
            this.mockRequestRepo.Setup(r => r.GetRequestById(1)).Returns(request);

            var doc = new Doctor(10, "Busy", "Doc", "c", true, "Surgeon", "L1", DoctorStatus.IN_EXAMINATION, 5);
            var shift = new Shift(1, doc, "Ward A", DateTime.Now.AddHours(-1), DateTime.Now.AddMinutes(10), ShiftStatus.ACTIVE);

            this.mockStaffRepo.Setup(s => s.LoadAllStaff()).Returns(new List<IStaff> { doc });
            this.mockShiftRepo.Setup(s => s.GetAllShifts()).Returns(new List<Shift> { shift });

            // Act
            var result = await this.service.ManualOverrideAsync(1, 10, 15);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            this.mockRequestRepo.Verify(r => r.UpdateRequestStatus(1, "ASSIGNED", 10, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ManualOverride_DoctorTooEarlyInShift_BlocksAssignment()
        {
            // Arrange
            var request = new ERRequest { Id = 1, Specialization = "Surgeon" };
            this.mockRequestRepo.Setup(r => r.GetRequestById(1)).Returns(request);

            var (doc, shift) = CreateActiveDoctor(10, "Surgeon", "Ward A", DoctorStatus.IN_EXAMINATION);
            this.mockStaffRepo.Setup(s => s.LoadAllStaff()).Returns(new List<IStaff> { doc });
            this.mockShiftRepo.Setup(s => s.GetAllShifts()).Returns(new List<Shift> { shift });

            // Act
            var result = await this.service.ManualOverrideAsync(1, 10, 5);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("Manual override blocked"));
        }
    }
}