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
    public class DoctorAppointmentServiceLogicTests
    {
        private Mock<IAppointmentRepository> mockAppointmentRepository;
        private Mock<IStaffRepository> mockStaffRepository;
        private Mock<IShiftRepository> mockShiftRepository;
        private DoctorAppointmentService doctorAppointmentService;

        [SetUp]
        public void Setup()
        {
            this.mockAppointmentRepository = new Mock<IAppointmentRepository>();
            this.mockStaffRepository = new Mock<IStaffRepository>();
            this.mockShiftRepository = new Mock<IShiftRepository>();

            this.doctorAppointmentService = new DoctorAppointmentService(
                this.mockAppointmentRepository.Object,
                this.mockStaffRepository.Object,
                this.mockShiftRepository.Object);
        }

        [Test]
        public async Task GetUpcomingAppointmentsAsync_WhenAppointmentsExistForMultipleDoctors_ReturnsOnlyRequestedDoctorAppointments()
        {
            var requestedDoctorIdentifier = 3;
            var appointmentForRequestedDoctor = CreateAppointment(1, requestedDoctorIdentifier, DateTime.Today.AddDays(1), 9, 10, "Scheduled");
            var appointmentForDifferentDoctor = CreateAppointment(2, 8, DateTime.Today.AddDays(1), 9, 10, "Scheduled");

            this.mockAppointmentRepository
                .Setup(appointmentRepository => appointmentRepository.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment> { appointmentForDifferentDoctor, appointmentForRequestedDoctor });

            var returnedAppointments = await this.doctorAppointmentService.GetUpcomingAppointmentsAsync(requestedDoctorIdentifier, DateTime.Today, 0, 10);

            Assert.That(returnedAppointments.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetUpcomingAppointmentsAsync_WhenAppointmentsAreUnordered_ReturnsAppointmentsOrderedByDateAndStartTime()
        {
            var requestedDoctorIdentifier = 3;
            var laterAppointment = CreateAppointment(1, requestedDoctorIdentifier, DateTime.Today.AddDays(2), 11, 12, "Scheduled");
            var earlierAppointment = CreateAppointment(2, requestedDoctorIdentifier, DateTime.Today.AddDays(1), 9, 10, "Scheduled");

            this.mockAppointmentRepository
                .Setup(appointmentRepository => appointmentRepository.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment> { laterAppointment, earlierAppointment });

            var returnedAppointments = await this.doctorAppointmentService.GetUpcomingAppointmentsAsync(requestedDoctorIdentifier, DateTime.Today, 0, 10);

            Assert.That(returnedAppointments.First().Id, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllDoctorsAsync_WhenDoctorNamesAreUnordered_ReturnsDoctorsOrderedAlphabetically()
        {
            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.GetAllDoctorsAsync())
                .ReturnsAsync(new List<(int DoctorId, string FirstName, string LastName)>
                {
                    (2, "Charlie", "Zeta"),
                    (1, "Alice", "Alpha"),
                });

            var returnedDoctors = await this.doctorAppointmentService.GetAllDoctorsAsync();

            Assert.That(returnedDoctors.First().DoctorName, Is.EqualTo("Alice Alpha"));
        }

        [Test]
        public async Task GetAppointmentDetailsAsync_WhenAppointmentWithRequestedIdentifierExists_ReturnsAppointmentWithRequestedIdentifier()
        {
            var requestedAppointmentIdentifier = 5;
            var matchingAppointment = CreateAppointment(requestedAppointmentIdentifier, 1, DateTime.Today, 9, 10, "Scheduled");

            this.mockAppointmentRepository
                .Setup(appointmentRepository => appointmentRepository.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment> { matchingAppointment });

            var returnedAppointment = await this.doctorAppointmentService.GetAppointmentDetailsAsync(requestedAppointmentIdentifier);

            Assert.That(returnedAppointment!.Id, Is.EqualTo(requestedAppointmentIdentifier));
        }

        [Test]
        public async Task GetAppointmentDetailsAsync_WhenAppointmentWithRequestedIdentifierDoesNotExist_ReturnsNull()
        {
            this.mockAppointmentRepository
                .Setup(appointmentRepository => appointmentRepository.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>());

            var returnedAppointment = await this.doctorAppointmentService.GetAppointmentDetailsAsync(99);

            Assert.That(returnedAppointment, Is.Null);
        }

        [Test]
        public async Task GetAppointmentsForAdminAsync_WhenAppointmentsExistForMultipleDoctors_ReturnsOnlyRequestedDoctorAppointments()
        {
            var requestedDoctorIdentifier = 10;
            var appointmentForRequestedDoctor = CreateAppointment(1, requestedDoctorIdentifier, DateTime.Today.AddDays(2), 10, 11, "Scheduled");
            var appointmentForDifferentDoctor = CreateAppointment(2, 20, DateTime.Today.AddDays(1), 9, 10, "Scheduled");

            this.mockAppointmentRepository
                .Setup(appointmentRepository => appointmentRepository.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment> { appointmentForDifferentDoctor, appointmentForRequestedDoctor });

            var returnedAppointments = await this.doctorAppointmentService.GetAppointmentsForAdminAsync(requestedDoctorIdentifier);

            Assert.That(returnedAppointments.Count, Is.EqualTo(1));
        }

        [Test]
        public void CreateAppointmentAsync_WhenDoctorIsOffDuty_ThrowsInvalidOperationException()
        {
            var offDutyDoctor = new Doctor
            {
                StaffID = 7,
                DoctorStatus = DoctorStatus.OFF_DUTY,
            };

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.GetStaffById(7))
                .Returns(offDutyDoctor);

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await this.doctorAppointmentService.CreateAppointmentAsync("PAT-3", 7, DateTime.Today, TimeSpan.FromHours(10)));
        }

        [Test]
        public async Task CreateAppointmentAsync_WhenPatientNameHasPrefix_PersistsAppointmentWithParsedPatientIdentifier()
        {
            var availableDoctor = new Doctor
            {
                StaffID = 7,
                DoctorStatus = DoctorStatus.AVAILABLE,
            };

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.GetStaffById(7))
                .Returns(availableDoctor);

            await this.doctorAppointmentService.CreateAppointmentAsync("PAT-42", 7, DateTime.Today, TimeSpan.FromHours(10));

            this.mockAppointmentRepository.Verify(
                appointmentRepository => appointmentRepository.AddAppointmentAsync(
                    42,
                    7,
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    "Scheduled"),
                Times.Once);
        }

        [Test]
        public async Task BookAppointmentAsync_WhenExternalReferenceIsMissing_PersistsAppointmentWithParsedPatientIdentifier()
        {
            var availableDoctor = new Doctor
            {
                StaffID = 8,
                DoctorStatus = DoctorStatus.AVAILABLE,
            };

            var appointmentWithoutExternalReference = CreateAppointment(9, 8, DateTime.Today, 9, 10, "Scheduled");
            appointmentWithoutExternalReference.PatientName = "PAT-55";
            appointmentWithoutExternalReference.ExternalRefId = string.Empty;

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.GetStaffById(8))
                .Returns(availableDoctor);

            await this.doctorAppointmentService.BookAppointmentAsync(appointmentWithoutExternalReference);

            this.mockAppointmentRepository.Verify(
                appointmentRepository => appointmentRepository.AddAppointmentAsync(
                    55,
                    8,
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    "Scheduled"),
                Times.Once);
        }

        [Test]
        public void FinishAppointmentAsync_WhenAppointmentIsAlreadyFinished_ThrowsInvalidOperationException()
        {
            var alreadyFinishedAppointment = CreateAppointment(1, 2, DateTime.Today, 9, 10, "Finished");

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await this.doctorAppointmentService.FinishAppointmentAsync(alreadyFinishedAppointment));
        }

        [Test]
        public async Task FinishAppointmentAsync_WhenNoOtherScheduledAppointmentOverlaps_UpdatesDoctorStatusToAvailable()
        {
            var appointmentToFinish = CreateAppointment(1, 2, DateTime.Today, 9, 10, "Scheduled");

            this.mockAppointmentRepository
                .Setup(appointmentRepository => appointmentRepository.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment> { appointmentToFinish });

            await this.doctorAppointmentService.FinishAppointmentAsync(appointmentToFinish);

            this.mockStaffRepository.Verify(
                staffRepository => staffRepository.UpdateStatusAsync(2, "AVAILABLE"),
                Times.Once);
        }

        [Test]
        public async Task GetAppointmentsInRangeAsync_WhenAppointmentOverlapsRequestedWindow_ReturnsOverlappingAppointment()
        {
            var requestedDoctorIdentifier = 2;
            var appointmentOverlappingWindow = CreateAppointment(1, requestedDoctorIdentifier, DateTime.Today, 9, 11, "Scheduled");

            this.mockAppointmentRepository
                .Setup(appointmentRepository => appointmentRepository.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment> { appointmentOverlappingWindow });

            var returnedAppointments = await this.doctorAppointmentService.GetAppointmentsInRangeAsync(
                requestedDoctorIdentifier,
                DateTime.Today.AddHours(10),
                DateTime.Today.AddHours(12));

            Assert.That(returnedAppointments.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetShiftsForStaffInRangeAsync_WhenShiftsExistForMultipleDoctors_ReturnsOnlyRequestedDoctorShifts()
        {
            var requestedDoctorIdentifier = 2;
            var requestedDoctor = new Doctor { StaffID = requestedDoctorIdentifier };
            var differentDoctor = new Doctor { StaffID = 9 };

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift>
                {
            new Shift(1, requestedDoctor, "Ward A", DateTime.Today, DateTime.Today.AddHours(8), ShiftStatus.ACTIVE),
            new Shift(2, differentDoctor, "Ward B", DateTime.Today, DateTime.Today.AddHours(8), ShiftStatus.ACTIVE),
                });

            var returnedShifts = await this.doctorAppointmentService.GetShiftsForStaffInRangeAsync(
                requestedDoctorIdentifier,
                DateTime.Today.AddHours(-1),
                DateTime.Today.AddHours(9));

            Assert.That(returnedShifts.Count, Is.EqualTo(1));
        }

        private static Appointment CreateAppointment(
            int appointmentIdentifier,
            int doctorIdentifier,
            DateTime appointmentDate,
            int startHour,
            int endHour,
            string appointmentStatus)
        {
            return new Appointment
            {
                Id = appointmentIdentifier,
                Doctor = new Doctor
                {
                    StaffID = doctorIdentifier,
                },
                Date = appointmentDate.Date,
                StartTime = TimeSpan.FromHours(startHour),
                EndTime = TimeSpan.FromHours(endHour),
                Status = appointmentStatus,
                PatientName = "PAT-1",
                ExternalRefId = "EXT-1",
            };
        }
    }
}