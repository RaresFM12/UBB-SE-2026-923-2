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
    public class DoctorAppointmentServiceTests
    {
        private Mock<IAppointmentRepository> mockAppointmentRepository;
        private Mock<IStaffRepository> mockStaffRepository;
        private Mock<IShiftRepository> mockShiftRepository;
        private DoctorAppointmentService service;

        [SetUp]
        public void Setup()
        {
            this.mockAppointmentRepository = new Mock<IAppointmentRepository>();
            this.mockStaffRepository = new Mock<IStaffRepository>();
            this.mockShiftRepository = new Mock<IShiftRepository>();
            this.service = new DoctorAppointmentService(this.mockAppointmentRepository.Object, this.mockStaffRepository.Object, this.mockShiftRepository.Object);
        }

        // --- GetUpcomingAppointmentsAsync ---
        [Test]
        public async Task GetUpcomingAppointmentsAsync_AppointmentInWindow_ReturnsIt()
        {
            var tomorrow = DateTime.Now.AddDays(1);
            var appointment = new Appointment { Id = 1, Doctor = new Doctor { StaffID = 1 }, Date = tomorrow, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(9.5), Status = "Scheduled" };
            this.mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(new List<Appointment> { appointment });

            var result = await this.service.GetUpcomingAppointmentsAsync(1, DateTime.Now, 0, 10);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetUpcomingAppointmentsAsync_AppointmentOutsideWindow_ReturnsEmpty()
        {
            var farFuture = DateTime.Now.AddDays(60);
            var appointment = new Appointment { Id = 1, Doctor = new Doctor { StaffID = 1 }, Date = farFuture, StartTime = TimeSpan.FromHours(9), Status = "Scheduled" };
            this.mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(new List<Appointment> { appointment });

            var result = await this.service.GetUpcomingAppointmentsAsync(1, DateTime.Now, 0, 10);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetUpcomingAppointmentsAsync_Pagination_SkipsCorrectly()
        {
            var appointments = Enumerable.Range(1, 5).Select(index => new Appointment
            {
                Id = index,
                Doctor = new Doctor { StaffID = 1 },
                Date = DateTime.Now.AddDays(index),
                StartTime = TimeSpan.FromHours(9),
                Status = "Scheduled"
            }).ToList();
            this.mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(appointments);

            var result = await this.service.GetUpcomingAppointmentsAsync(1, DateTime.Now, 2, 2);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        // --- GetAllDoctorsAsync ---
        [Test]
        public async Task GetAllDoctorsAsync_ThreeDoctors_SortedAlphabetically()
        {
            this.mockStaffRepository.Setup(repository => repository.GetAllDoctorsAsync()).ReturnsAsync(new List<(int, string, string)>
            {
                (1, "Charlie", "Zeta"), (2, "Alice", "Alpha"), (3, "Bob", "Beta")
            });

            var result = await this.service.GetAllDoctorsAsync();
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].DoctorName, Is.EqualTo("Alice Alpha"));
        }

        [Test]
        public async Task GetAllDoctorsAsync_Empty_ReturnsEmpty()
        {
            this.mockStaffRepository.Setup(repository => repository.GetAllDoctorsAsync()).ReturnsAsync(new List<(int, string, string)>());
            var result = await this.service.GetAllDoctorsAsync();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        // --- GetAppointmentDetailsAsync ---
        [Test]
        public async Task GetAppointmentDetailsAsync_Found_ReturnsAppointment()
        {
            var appointment = new Appointment { Id = 5, Doctor = new Doctor { StaffID = 1 }, Date = DateTime.Now, StartTime = TimeSpan.FromHours(9), Status = "Scheduled" };
            this.mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(new List<Appointment> { appointment });

            var result = await this.service.GetAppointmentDetailsAsync(5);
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(5));
        }

        [Test]
        public async Task GetAppointmentDetailsAsync_NotFound_ReturnsNull()
        {
            this.mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(new List<Appointment>());
            var result = await this.service.GetAppointmentDetailsAsync(99);
            Assert.That(result, Is.Null);
        }

        // --- GetAppointmentsForAdminAsync ---
        [Test]
        public async Task GetAppointmentsForAdminAsync_FiltersAndOrders()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { Id = 1, Doctor = new Doctor { StaffID = 1 }, Date = DateTime.Now.AddDays(2), StartTime = TimeSpan.FromHours(10), Status = "Scheduled" },
                new Appointment { Id = 2, Doctor = new Doctor { StaffID = 1 }, Date = DateTime.Now.AddDays(1), StartTime = TimeSpan.FromHours(9), Status = "Scheduled" },
                new Appointment { Id = 3, Doctor = new Doctor { StaffID = 2 }, Date = DateTime.Now.AddDays(1), StartTime = TimeSpan.FromHours(8), Status = "Scheduled" }
            };
            this.mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(appointments);

            var result = await this.service.GetAppointmentsForAdminAsync(1);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Id, Is.EqualTo(2));
        }

        // --- CancelAppointmentAsync ---
        [Test]
        public async Task CancelAppointmentAsync_FinishedAppointment_Throws()
        {
            var appointment = new Appointment { Id = 1, Status = "Finished" };
            Assert.ThrowsAsync<InvalidOperationException>(async () => await this.service.CancelAppointmentAsync(appointment));
        }

        [Test]
        public async Task CancelAppointmentAsync_ScheduledAppointment_Cancels()
        {
            var appointment = new Appointment { Id = 1, Status = "Scheduled" };
            this.mockAppointmentRepository.Setup(repository => repository.UpdateAppointmentStatusAsync(1, "Canceled")).Returns(Task.CompletedTask);

            await this.service.CancelAppointmentAsync(appointment);
            Assert.That(appointment.Status, Is.EqualTo("Canceled"));
        }

        // --- GetShiftsForStaffInRangeAsync ---
        [Test]
        public async Task GetShiftsForStaffInRangeAsync_ReturnsFilteredShifts()
        {
            var doctor = new Doctor { StaffID = 1 };
            var now = DateTime.Now;
            var shift = new Shift(1, doctor, "Ward", now, now.AddHours(8), ShiftStatus.ACTIVE);
            var cancelledShift = new Shift(2, doctor, "Ward", now, now.AddHours(8), ShiftStatus.CANCELLED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift, cancelledShift });

            var result = await this.service.GetShiftsForStaffInRangeAsync(1, now.AddHours(-1), now.AddHours(10));
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetShiftsForStaffInRangeAsync_NoShiftRepo_ReturnsEmpty()
        {
            var serviceWithoutShiftRepository = new DoctorAppointmentService(this.mockAppointmentRepository.Object, this.mockStaffRepository.Object);
            var result = await serviceWithoutShiftRepository.GetShiftsForStaffInRangeAsync(1, DateTime.Now, DateTime.Now.AddDays(7));
            Assert.That(result.Count, Is.EqualTo(0));
        }

        // --- NEW TESTS FOR GAP FILLING ---

        [Test]
        public void CreateAppointmentAsync_DoctorOffDuty_ThrowsException()
        {
            var offDutyDoctor = new Doctor { StaffID = 1, DoctorStatus = DoctorStatus.OFF_DUTY };
            this.mockStaffRepository.Setup(repo => repo.GetStaffById(1)).Returns(offDutyDoctor);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await this.service.CreateAppointmentAsync("123", 1, DateTime.Now, TimeSpan.FromHours(10)));

            Assert.That(ex.Message, Does.Contain("OFF_DUTY"));
        }

        [Test]
        public async Task CreateAppointmentAsync_ValidData_PersistsAppointment()
        {
            var onDutyDoctor = new Doctor { StaffID = 1, DoctorStatus = DoctorStatus.AVAILABLE };
            this.mockStaffRepository.Setup(repo => repo.GetStaffById(1)).Returns(onDutyDoctor);

            await this.service.CreateAppointmentAsync("123", 1, DateTime.Now, TimeSpan.FromHours(10));

            this.mockAppointmentRepository.Verify(repo => repo.AddAppointmentAsync(
                It.IsAny<int>(), 1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), "Scheduled"), Times.Once);
        }

        [Test]
        public async Task FinishAppointmentAsync_ValidAppointment_UpdatesStatus()
        {
            var appointment = new Appointment { Id = 99, Doctor = new Doctor { StaffID = 1 }, Status = "Scheduled", Date = DateTime.Now, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10) };
            this.mockAppointmentRepository.Setup(repo => repo.GetAllAppointmentsAsync()).ReturnsAsync(new List<Appointment>());

            await this.service.FinishAppointmentAsync(appointment);

            this.mockAppointmentRepository.Verify(repo => repo.UpdateAppointmentStatusAsync(99, "Finished"), Times.Once);
            Assert.That(appointment.Status, Is.EqualTo("Finished"));
        }

        // --- BookAppointmentAsync ---
        [Test]
        public async Task BookAppointmentAsync_DoctorOffDuty_ThrowsException()
        {
            var offDutyDoctor = new Doctor { StaffID = 1, DoctorStatus = DoctorStatus.OFF_DUTY };
            this.mockStaffRepository.Setup(repository => repository.GetStaffById(1)).Returns(offDutyDoctor);

            var appointmentToBook = new Appointment
            {
                PatientName = "123",
                Doctor = new Doctor { StaffID = 1 },
                Date = DateTime.Now,
                StartTime = TimeSpan.FromHours(9),
            };

            Assert.ThrowsAsync<InvalidOperationException>(async () => await this.service.BookAppointmentAsync(appointmentToBook));
        }

        [Test]
        public async Task BookAppointmentAsync_ValidAppointment_PersistsSuccessfully()
        {
            var availableDoctor = new Doctor { StaffID = 1, DoctorStatus = DoctorStatus.AVAILABLE };
            this.mockStaffRepository.Setup(repository => repository.GetStaffById(1)).Returns(availableDoctor);

            var appointmentToBook = new Appointment
            {
                PatientName = "123",
                Doctor = new Doctor { StaffID = 1 },
                Date = DateTime.Now,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(9.5),
                Status = "Scheduled",
            };

            await this.service.BookAppointmentAsync(appointmentToBook);

            this.mockAppointmentRepository.Verify(repository => repository.AddAppointmentAsync(
                It.IsAny<int>(), 1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), "Scheduled"), Times.Once);
        }

        // --- GetAppointmentsInRangeAsync ---
        [Test]
        public async Task GetAppointmentsInRangeAsync_AppointmentsInRange_ReturnsFiltered()
        {
            var rangeStart = DateTime.Now;
            var rangeEnd = DateTime.Now.AddDays(7);
            var appointmentInRange = new Appointment
            {
                Id = 1,
                Doctor = new Doctor { StaffID = 1 },
                Date = DateTime.Now.AddDays(1),
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(10),
                Status = "Scheduled",
            };
            var appointmentOutOfRange = new Appointment
            {
                Id = 2,
                Doctor = new Doctor { StaffID = 1 },
                Date = DateTime.Now.AddDays(30),
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(10),
                Status = "Scheduled",
            };
            this.mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment> { appointmentInRange, appointmentOutOfRange });

            var result = await this.service.GetAppointmentsInRangeAsync(1, rangeStart, rangeEnd);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAppointmentsInRangeAsync_NoAppointmentsForDoctor_ReturnsEmpty()
        {
            var appointmentForOtherDoctor = new Appointment
            {
                Id = 1,
                Doctor = new Doctor { StaffID = 2 },
                Date = DateTime.Now.AddDays(1),
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(10),
                Status = "Scheduled",
            };
            this.mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment> { appointmentForOtherDoctor });

            var result = await this.service.GetAppointmentsInRangeAsync(1, DateTime.Now, DateTime.Now.AddDays(7));

            Assert.That(result.Count, Is.EqualTo(0));
        }
    }
}
