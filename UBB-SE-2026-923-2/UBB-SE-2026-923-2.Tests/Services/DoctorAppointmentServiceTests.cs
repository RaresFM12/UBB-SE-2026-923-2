using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.Tests.Services
{
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
            mockAppointmentRepository = new Mock<IAppointmentRepository>();
            mockStaffRepository = new Mock<IStaffRepository>();
            mockShiftRepository = new Mock<IShiftRepository>();
            service = new DoctorAppointmentService(mockAppointmentRepository.Object, mockStaffRepository.Object, mockShiftRepository.Object);
        }

        [Test]
        public async Task GetUpcomingAppointmentsAsync_NoAppointments_ReturnsEmpty()
        {
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(new List<Appointment>());
            var result = await service.GetUpcomingAppointmentsAsync(1, DateTime.Now, 0, 10);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetUpcomingAppointmentsAsync_AppointmentForDifferentDoctor_ReturnsEmpty()
        {
            var appointment = new Appointment { DoctorId = 2, Date = DateTime.Now.AddDays(1), StartTime = TimeSpan.FromHours(9), Status = "Scheduled" };
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(new List<Appointment> { appointment });

            var result = await service.GetUpcomingAppointmentsAsync(1, DateTime.Now, 0, 10);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetUpcomingAppointmentsAsync_AppointmentInWindow_ReturnsIt()
        {
            var tomorrow = DateTime.Now.AddDays(1);
            var appointment = new Appointment { Id = 1, DoctorId = 1, Date = tomorrow, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(9.5), Status = "Scheduled" };
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(new List<Appointment> { appointment });

            var result = await service.GetUpcomingAppointmentsAsync(1, DateTime.Now, 0, 10);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetUpcomingAppointmentsAsync_AppointmentOutsideWindow_ReturnsEmpty()
        {
            var farFuture = DateTime.Now.AddDays(60);
            var appointment = new Appointment { Id = 1, DoctorId = 1, Date = farFuture, StartTime = TimeSpan.FromHours(9), Status = "Scheduled" };
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(new List<Appointment> { appointment });

            var result = await service.GetUpcomingAppointmentsAsync(1, DateTime.Now, 0, 10);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetUpcomingAppointmentsAsync_Pagination_SkipsCorrectly()
        {
            var appointments = Enumerable.Range(1, 5).Select(index => new Appointment
            {
                Id = index, DoctorId = 1, Date = DateTime.Now.AddDays(index), StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10), Status = "Scheduled"
            }).ToList();
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(appointments);

            var result = await service.GetUpcomingAppointmentsAsync(1, DateTime.Now, 2, 2);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllDoctorsAsync_ReturnsSortedDoctors()
        {
            mockStaffRepository.Setup(repository => repository.GetAllDoctorsAsync()).ReturnsAsync(new List<(int, string, string)>
            {
                (1, "Zoe", "Adams"),
                (2, "Alice", "Brown"),
            });

            var result = await service.GetAllDoctorsAsync();
            Assert.That(result[0].DoctorName, Is.EqualTo("Alice Brown"));
            Assert.That(result[1].DoctorName, Is.EqualTo("Zoe Adams"));
        }

        [Test]
        public async Task GetAllDoctorsAsync_Empty_ReturnsEmpty()
        {
            mockStaffRepository.Setup(repository => repository.GetAllDoctorsAsync()).ReturnsAsync(new List<(int, string, string)>());
            var result = await service.GetAllDoctorsAsync();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetAppointmentDetailsAsync_Found_ReturnsAppointment()
        {
            var appointment = new Appointment { Id = 5, DoctorId = 1, Date = DateTime.Now, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10), Status = "Scheduled" };
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(new List<Appointment> { appointment });

            var result = await service.GetAppointmentDetailsAsync(5);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(5));
        }

        [Test]
        public async Task GetAppointmentDetailsAsync_NotFound_ReturnsNull()
        {
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(new List<Appointment>());
            var result = await service.GetAppointmentDetailsAsync(99);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAppointmentsForAdminAsync_FiltersAndOrders()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { Id = 1, DoctorId = 1, Date = DateTime.Now.AddDays(2), StartTime = TimeSpan.FromHours(10), Status = "Scheduled" },
                new Appointment { Id = 2, DoctorId = 1, Date = DateTime.Now.AddDays(1), StartTime = TimeSpan.FromHours(9), Status = "Scheduled" },
                new Appointment { Id = 3, DoctorId = 2, Date = DateTime.Now.AddDays(1), StartTime = TimeSpan.FromHours(8), Status = "Scheduled" },
            };
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(appointments);

            var result = await service.GetAppointmentsForAdminAsync(1);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Id, Is.EqualTo(2));
        }

        [Test]
        public async Task CancelAppointmentAsync_FinishedAppointment_Throws()
        {
            var appointment = new Appointment { Id = 1, Status = "Finished" };
            Assert.ThrowsAsync<InvalidOperationException>(async () => await service.CancelAppointmentAsync(appointment));
        }

        [Test]
        public async Task CancelAppointmentAsync_ScheduledAppointment_Cancels()
        {
            var appointment = new Appointment { Id = 1, Status = "Scheduled" };
            mockAppointmentRepository.Setup(repository => repository.UpdateAppointmentStatusAsync(1, "Canceled")).Returns(Task.CompletedTask);

            await service.CancelAppointmentAsync(appointment);
            Assert.That(appointment.Status, Is.EqualTo("Canceled"));
            mockAppointmentRepository.Verify(repository => repository.UpdateAppointmentStatusAsync(1, "Canceled"), Times.Once);
        }

        [Test]
        public async Task GetShiftsForStaffInRangeAsync_ReturnsFilteredShifts()
        {
            var doctor = new Doctor { StaffID = 1 };
            var now = DateTime.Now;
            var shift = new Shift(1, doctor, "Ward", now, now.AddHours(8), ShiftStatus.ACTIVE);
            var cancelledShift = new Shift(2, doctor, "Ward", now, now.AddHours(8), ShiftStatus.CANCELLED);
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift, cancelledShift });

            var result = await service.GetShiftsForStaffInRangeAsync(1, now.AddHours(-1), now.AddHours(10));
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetShiftsForStaffInRangeAsync_NoShiftRepo_ReturnsEmpty()
        {
            var serviceWithoutShiftRepository = new DoctorAppointmentService(mockAppointmentRepository.Object, mockStaffRepository.Object);
            var result = await serviceWithoutShiftRepository.GetShiftsForStaffInRangeAsync(1, DateTime.Now, DateTime.Now.AddDays(7));
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetUpcomingAppointmentsAsync_OrderedByDateThenTime()
        {
            var baseDate = DateTime.Now.AddDays(1);
            var appointments = new List<Appointment>
            {
                new Appointment { Id = 1, DoctorId = 1, Date = baseDate, StartTime = TimeSpan.FromHours(14), EndTime = TimeSpan.FromHours(15), Status = "S" },
                new Appointment { Id = 2, DoctorId = 1, Date = baseDate, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10), Status = "S" },
            };
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(appointments);

            var result = await service.GetUpcomingAppointmentsAsync(1, DateTime.Now, 0, 10);
            Assert.That(result[0].StartTime, Is.LessThan(result[1].StartTime));
        }

        [Test]
        public async Task GetUpcomingAppointmentsAsync_TakeLimitsResults()
        {
            var appointments = Enumerable.Range(1, 10).Select(index => new Appointment
            {
                Id = index, DoctorId = 1, Date = DateTime.Now.AddDays(index), StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10), Status = "Scheduled"
            }).ToList();
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(appointments);

            var result = await service.GetUpcomingAppointmentsAsync(1, DateTime.Now, 0, 3);
            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task GetUpcomingAppointmentsAsync_SkipBeyondCount_ReturnsEmpty()
        {
            var appointments = Enumerable.Range(1, 3).Select(index => new Appointment
            {
                Id = index, DoctorId = 1, Date = DateTime.Now.AddDays(index), StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10), Status = "Scheduled"
            }).ToList();
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(appointments);

            var result = await service.GetUpcomingAppointmentsAsync(1, DateTime.Now, 10, 10);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetUpcomingAppointmentsAsync_CanceledAppointment_StillReturned()
        {
            var appointment = new Appointment { Id = 1, DoctorId = 1, Date = DateTime.Now.AddDays(1), StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10), Status = "Canceled" };
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(new List<Appointment> { appointment });

            var result = await service.GetUpcomingAppointmentsAsync(1, DateTime.Now, 0, 10);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetUpcomingAppointmentsAsync_PastAppointment_Excluded()
        {
            var appointment = new Appointment { Id = 1, DoctorId = 1, Date = DateTime.Today.AddDays(-1), StartTime = TimeSpan.FromHours(9), Status = "Scheduled" };
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(new List<Appointment> { appointment });

            var result = await service.GetUpcomingAppointmentsAsync(1, DateTime.Now, 0, 10);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task CancelAppointmentAsync_CanceledAppointment_CancelsAgain()
        {
            var appointment = new Appointment { Id = 1, Status = "Canceled" };
            mockAppointmentRepository.Setup(repository => repository.UpdateAppointmentStatusAsync(1, "Canceled")).Returns(Task.CompletedTask);

            await service.CancelAppointmentAsync(appointment);
            Assert.That(appointment.Status, Is.EqualTo("Canceled"));
        }

        [Test]
        public async Task GetAllDoctorsAsync_SingleDoctor_ReturnsSingle()
        {
            mockStaffRepository.Setup(repository => repository.GetAllDoctorsAsync()).ReturnsAsync(new List<(int, string, string)>
            {
                (1, "John", "Doe"),
            });

            var result = await service.GetAllDoctorsAsync();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].DoctorName, Is.EqualTo("John Doe"));
        }

        [Test]
        public async Task GetAppointmentsForAdminAsync_NoDoctorAppointments_ReturnsEmpty()
        {
            var appointment = new Appointment { Id = 1, DoctorId = 2, Date = DateTime.Now.AddDays(1), StartTime = TimeSpan.FromHours(9), Status = "Scheduled" };
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(new List<Appointment> { appointment });

            var result = await service.GetAppointmentsForAdminAsync(1);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetShiftsForStaffInRangeAsync_ShiftOutsideRange_ReturnsEmpty()
        {
            var doctor = new Doctor { StaffID = 1 };
            var now = DateTime.Now;
            var shift = new Shift(1, doctor, "Ward", now.AddDays(10), now.AddDays(10).AddHours(8), ShiftStatus.ACTIVE);
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await service.GetShiftsForStaffInRangeAsync(1, now, now.AddDays(2));
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetShiftsForStaffInRangeAsync_DifferentStaff_ReturnsEmpty()
        {
            var doctor = new Doctor { StaffID = 2 };
            var now = DateTime.Now;
            var shift = new Shift(1, doctor, "Ward", now, now.AddHours(8), ShiftStatus.ACTIVE);
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await service.GetShiftsForStaffInRangeAsync(1, now.AddHours(-1), now.AddHours(10));
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetAppointmentDetailsAsync_MultipleAppointments_ReturnsCorrectOne()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { Id = 1, DoctorId = 1, Date = DateTime.Now, StartTime = TimeSpan.FromHours(9), Status = "Scheduled" },
                new Appointment { Id = 2, DoctorId = 1, Date = DateTime.Now, StartTime = TimeSpan.FromHours(10), Status = "Scheduled" },
                new Appointment { Id = 3, DoctorId = 2, Date = DateTime.Now, StartTime = TimeSpan.FromHours(11), Status = "Scheduled" },
            };
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(appointments);

            var result = await service.GetAppointmentDetailsAsync(2);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllDoctorsAsync_ThreeDoctors_SortedAlphabetically()
        {
            mockStaffRepository.Setup(repository => repository.GetAllDoctorsAsync()).ReturnsAsync(new List<(int, string, string)>
            {
                (1, "Charlie", "Zeta"),
                (2, "Alice", "Alpha"),
                (3, "Bob", "Beta"),
            });

            var result = await service.GetAllDoctorsAsync();
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].DoctorName, Is.EqualTo("Alice Alpha"));
        }

        [Test]
        public async Task GetAppointmentsForAdminAsync_OrderedByDateThenTime()
        {
            var baseDate = DateTime.Now.AddDays(1);
            var appointments = new List<Appointment>
            {
                new Appointment { Id = 1, DoctorId = 1, Date = baseDate.AddDays(2), StartTime = TimeSpan.FromHours(10), Status = "Scheduled" },
                new Appointment { Id = 2, DoctorId = 1, Date = baseDate, StartTime = TimeSpan.FromHours(14), Status = "Scheduled" },
                new Appointment { Id = 3, DoctorId = 1, Date = baseDate, StartTime = TimeSpan.FromHours(9), Status = "Scheduled" },
            };
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(appointments);

            var result = await service.GetAppointmentsForAdminAsync(1);
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].StartTime, Is.LessThanOrEqualTo(result[1].StartTime).Or.LessThanOrEqualTo(TimeSpan.FromHours(10)));
        }

        [Test]
        public async Task CancelAppointmentAsync_UpdatesStatusToCanceled()
        {
            var appointment = new Appointment { Id = 5, Status = "Scheduled" };
            mockAppointmentRepository.Setup(repository => repository.UpdateAppointmentStatusAsync(5, "Canceled")).Returns(Task.CompletedTask);

            await service.CancelAppointmentAsync(appointment);
            Assert.That(appointment.Status, Is.EqualTo("Canceled"));
            mockAppointmentRepository.Verify(repository => repository.UpdateAppointmentStatusAsync(5, "Canceled"), Times.Once);
        }

        [Test]
        public async Task GetUpcomingAppointmentsAsync_ExactlyAt30Days_Excluded()
        {
            var appointment = new Appointment { Id = 1, DoctorId = 1, Date = DateTime.Now.AddDays(31), StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10), Status = "Scheduled" };
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(new List<Appointment> { appointment });

            var result = await service.GetUpcomingAppointmentsAsync(1, DateTime.Now, 0, 10);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetShiftsForStaffInRangeAsync_MultipleShifts_ReturnsAll()
        {
            var doctor = new Doctor { StaffID = 1 };
            var now = DateTime.Now;
            var shifts = new List<Shift>
            {
                new Shift(1, doctor, "Ward", now, now.AddHours(4), ShiftStatus.ACTIVE),
                new Shift(2, doctor, "Ward", now.AddHours(5), now.AddHours(8), ShiftStatus.SCHEDULED),
            };
            mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(shifts);

            var result = await service.GetShiftsForStaffInRangeAsync(1, now.AddHours(-1), now.AddHours(10));
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAppointmentsForAdminAsync_ManyAppointments_ReturnsAll()
        {
            var appointments = Enumerable.Range(1, 10).Select(index => new Appointment
            {
                Id = index, DoctorId = 1, Date = DateTime.Now.AddDays(index), StartTime = TimeSpan.FromHours(9), Status = "Scheduled"
            }).ToList();
            mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync()).ReturnsAsync(appointments);

            var result = await service.GetAppointmentsForAdminAsync(1);
            Assert.That(result.Count, Is.EqualTo(10));
        }
    }
}




