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
    public class HangoutServiceLogicTests
    {
        private Mock<IHangoutRepository> mockHangoutRepository;
        private Mock<IHangoutParticipantRepository> mockHangoutParticipantRepository;
        private Mock<IAppointmentRepository> mockAppointmentRepository;
        private Mock<IStaffRepository> mockStaffRepository;
        private Mock<IEvaluationsRepository> mockEvaluationsRepository;
        private HangoutService hangoutService;

        [SetUp]
        public void Setup()
        {
            this.mockHangoutRepository = new Mock<IHangoutRepository>();
            this.mockHangoutParticipantRepository = new Mock<IHangoutParticipantRepository>();
            this.mockAppointmentRepository = new Mock<IAppointmentRepository>();
            this.mockStaffRepository = new Mock<IStaffRepository>();
            this.mockEvaluationsRepository = new Mock<IEvaluationsRepository>();

            this.hangoutService = new HangoutService(
                this.mockHangoutRepository.Object,
                this.mockHangoutParticipantRepository.Object,
                this.mockAppointmentRepository.Object,
                this.mockStaffRepository.Object,
                this.mockEvaluationsRepository.Object);

            this.mockAppointmentRepository
                .Setup(appointmentRepository => appointmentRepository.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>());

            this.mockEvaluationsRepository
                .Setup(evaluationsRepository => evaluationsRepository.GetAllEvaluations())
                .Returns(new List<MedicalEvaluation>());
        }

        [Test]
        public void CreateHangout_WhenTitleIsTooShort_ThrowsArgumentException()
        {
            var creator = CreateDoctor(1);

            Assert.Throws<ArgumentException>(
                () => this.hangoutService.CreateHangout("Tiny", "Valid description", DateTime.Now.AddDays(10), 5, creator));
        }

        [Test]
        public void CreateHangout_WhenDateIsLessThanOneWeekAway_ThrowsArgumentException()
        {
            var creator = CreateDoctor(1);

            Assert.Throws<ArgumentException>(
                () => this.hangoutService.CreateHangout("Valid title", "Valid description", DateTime.Now.AddDays(3), 5, creator));
        }

        [Test]
        public void CreateHangout_WhenCreatorHasActiveAppointmentOnHangoutDate_ThrowsInvalidOperationException()
        {
            var creator = CreateDoctor(1);
            var hangoutDate = DateTime.Now.AddDays(10);

            this.mockAppointmentRepository
                .Setup(appointmentRepository => appointmentRepository.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>
                {
                    CreateAppointment(1, creator.StaffID, hangoutDate, "Scheduled"),
                });

            Assert.Throws<InvalidOperationException>(
                () => this.hangoutService.CreateHangout("Valid title", "Valid description", hangoutDate, 5, creator));
        }

        [Test]
        public void CreateHangout_WhenDataIsValid_AddsCreatorAsParticipant()
        {
            var creator = CreateDoctor(1);
            var hangoutDate = DateTime.Now.AddDays(10);

            this.mockHangoutRepository
                .Setup(hangoutRepository => hangoutRepository.AddHangout("Valid title", "Valid description", hangoutDate, 5))
                .Returns(77);

            this.hangoutService.CreateHangout("Valid title", "Valid description", hangoutDate, 5, creator);

            this.mockHangoutParticipantRepository.Verify(
                hangoutParticipantRepository => hangoutParticipantRepository.AddParticipant(77, creator.StaffID),
                Times.Once);
        }

        [Test]
        public void JoinHangout_WhenHangoutDoesNotExist_ThrowsArgumentException()
        {
            var staffMember = CreateDoctor(1);

            this.mockHangoutRepository
                .Setup(hangoutRepository => hangoutRepository.GetHangoutById(99))
                .Returns((Hangout)null);

            Assert.Throws<ArgumentException>(() => this.hangoutService.JoinHangout(99, staffMember));
        }

        [Test]
        public void JoinHangout_WhenHangoutIsFull_ThrowsInvalidOperationException()
        {
            var staffMember = CreateDoctor(3);
            var fullHangout = CreateHangout(10, DateTime.Now.AddDays(10), 2);

            this.mockHangoutRepository
                .Setup(hangoutRepository => hangoutRepository.GetHangoutById(10))
                .Returns(fullHangout);

            this.mockHangoutParticipantRepository
                .Setup(hangoutParticipantRepository => hangoutParticipantRepository.GetAllParticipants())
                .Returns(new List<(int HangoutId, int StaffId)>
                {
                    (10, 1),
                    (10, 2),
                });

            Assert.Throws<InvalidOperationException>(() => this.hangoutService.JoinHangout(10, staffMember));
        }

        [Test]
        public void JoinHangout_WhenStaffMemberAlreadyJoined_ThrowsInvalidOperationException()
        {
            var staffMember = CreateDoctor(1);
            var hangout = CreateHangout(10, DateTime.Now.AddDays(10), 5);

            this.mockHangoutRepository
                .Setup(hangoutRepository => hangoutRepository.GetHangoutById(10))
                .Returns(hangout);

            this.mockHangoutParticipantRepository
                .Setup(hangoutParticipantRepository => hangoutParticipantRepository.GetAllParticipants())
                .Returns(new List<(int HangoutId, int StaffId)>
                {
                    (10, 1),
                });

            Assert.Throws<InvalidOperationException>(() => this.hangoutService.JoinHangout(10, staffMember));
        }

        [Test]
        public void JoinHangout_WhenStaffMemberCanJoin_AddsParticipant()
        {
            var staffMember = CreateDoctor(1);
            var hangout = CreateHangout(10, DateTime.Now.AddDays(10), 5);

            this.mockHangoutRepository
                .Setup(hangoutRepository => hangoutRepository.GetHangoutById(10))
                .Returns(hangout);

            this.mockHangoutParticipantRepository
                .Setup(hangoutParticipantRepository => hangoutParticipantRepository.GetAllParticipants())
                .Returns(new List<(int HangoutId, int StaffId)>());

            this.hangoutService.JoinHangout(10, staffMember);

            this.mockHangoutParticipantRepository.Verify(
                hangoutParticipantRepository => hangoutParticipantRepository.AddParticipant(10, 1),
                Times.Once);
        }

        [Test]
        public void GetAllHangouts_WhenParticipantsExist_PopulatesParticipantList()
        {
            var participatingDoctor = CreateDoctor(1);
            var hangout = CreateHangout(10, DateTime.Now.AddDays(10), 5);

            this.mockHangoutRepository
                .Setup(hangoutRepository => hangoutRepository.GetAllHangouts())
                .Returns(new List<Hangout> { hangout });

            this.mockHangoutParticipantRepository
                .Setup(hangoutParticipantRepository => hangoutParticipantRepository.GetAllParticipants())
                .Returns(new List<(int HangoutId, int StaffId)>
                {
                    (10, 1),
                });

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff> { participatingDoctor });

            var hangouts = this.hangoutService.GetAllHangouts();

            Assert.That(hangouts[0].ParticipantList.Count, Is.EqualTo(1));
        }

        private static Doctor CreateDoctor(int doctorIdentifier)
        {
            return new Doctor(doctorIdentifier, "John", "Doe", "contract", true, "General", "License", DoctorStatus.AVAILABLE, 5);
        }

        private static Appointment CreateAppointment(int appointmentIdentifier, int doctorIdentifier, DateTime appointmentDate, string appointmentStatus)
        {
            return new Appointment
            {
                Id = appointmentIdentifier,
                Doctor = new Doctor
                {
                    StaffID = doctorIdentifier,
                },
                Date = appointmentDate.Date,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(10),
                Status = appointmentStatus,
            };
        }

        private static Hangout CreateHangout(int hangoutIdentifier, DateTime hangoutDate, int maximumParticipants)
        {
            return new Hangout
            {
                HangoutID = hangoutIdentifier,
                Title = "Valid title",
                Description = "Valid description",
                Date = hangoutDate,
                MaxParticipants = maximumParticipants,
            };
        }
    }
}