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
    public class HangoutServiceTests
    {
        private Mock<IHangoutRepository> mockHangoutRepository;
        private Mock<IHangoutParticipantRepository> mockParticipantRepository;
        private Mock<IAppointmentRepository> mockAppointmentRepository;
        private Mock<IStaffRepository> mockStaffRepository;
        private Mock<IEvaluationsRepository> mockEvaluationsRepository;
        private HangoutService service;
        private Doctor doctor1;

        [SetUp]
        public void Setup()
        {
            this.mockHangoutRepository = new Mock<IHangoutRepository>();
            this.mockParticipantRepository = new Mock<IHangoutParticipantRepository>();
            this.mockAppointmentRepository = new Mock<IAppointmentRepository>();
            this.mockStaffRepository = new Mock<IStaffRepository>();
            this.mockEvaluationsRepository = new Mock<IEvaluationsRepository>();
            this.service = new HangoutService(
                this.mockHangoutRepository.Object,
                this.mockParticipantRepository.Object,
                this.mockAppointmentRepository.Object,
                this.mockStaffRepository.Object,
                this.mockEvaluationsRepository.Object);

            this.doctor1 = new Doctor(1, "John", "Doe", "c", true, "Gen", "L1", DoctorStatus.AVAILABLE, 5);
            this.mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>());
            this.mockEvaluationsRepository.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation>());
        }

        // --- CreateHangout Tests ---
        [Test]
        public void CreateHangout_TitleNull_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                this.service.CreateHangout(null, "desc", DateTime.Now.AddDays(10), 5, this.doctor1));
        }

        [Test]
        public void CreateHangout_DateTooSoon_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                this.service.CreateHangout("ValidTitle", "desc", DateTime.Now.AddDays(3), 5, this.doctor1));
        }

        [Test]
        public void CreateHangout_ConflictingAppointment_Throws()
        {
            var hangoutDate = DateTime.Now.AddDays(10);
            this.mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>
                {
                    new Appointment { Doctor = new Doctor { StaffID = 1 }, Date = hangoutDate.Date, Status = "Scheduled", StartTime = TimeSpan.FromHours(9) },
                });

            Assert.Throws<InvalidOperationException>(() =>
                this.service.CreateHangout("ValidTitle", "desc", hangoutDate, 5, this.doctor1));
        }

        [Test]
        public void CreateHangout_MedicalEvalOnDate_Throws()
        {
            var hangoutDate = DateTime.Now.AddDays(10);
            this.mockEvaluationsRepository.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation>
            {
                new MedicalEvaluation { Evaluator = this.doctor1, EvaluationDate = hangoutDate.Date },
            });

            Assert.Throws<InvalidOperationException>(() =>
                this.service.CreateHangout("ValidTitle", "desc", hangoutDate, 5, this.doctor1));
        }

        [Test]
        public void CreateHangout_Valid_ReturnsId()
        {
            this.mockHangoutRepository.Setup(repository => repository.AddHangout(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(42);

            var result = this.service.CreateHangout("ValidTitle", "desc", DateTime.Now.AddDays(10), 5, this.doctor1);
            Assert.That(result, Is.EqualTo(42));
            this.mockParticipantRepository.Verify(repository => repository.AddParticipant(42, 1), Times.Once);
        }

        // --- JoinHangout Tests ---
        [Test]
        public void JoinHangout_HangoutNotFound_Throws()
        {
            this.mockHangoutRepository.Setup(repository => repository.GetHangoutById(1)).Returns((Hangout)null);
            Assert.Throws<ArgumentException>(() => this.service.JoinHangout(1, this.doctor1));
        }

        [Test]
        public void JoinHangout_AlreadyJoined_Throws()
        {
            var hangout = new Hangout(1, "Title", "Desc", DateTime.Now.AddDays(10), 10);
            this.mockHangoutRepository.Setup(repository => repository.GetHangoutById(1)).Returns(hangout);
            this.mockParticipantRepository.Setup(repository => repository.GetAllParticipants()).Returns(new List<(int, int)> { (1, 1) });

            Assert.Throws<InvalidOperationException>(() => this.service.JoinHangout(1, this.doctor1));
        }

        [Test]
        public void JoinHangout_ConflictingAppointment_Throws()
        {
            var hangoutDate = DateTime.Now.AddDays(10);
            var hangout = new Hangout(1, "Title", "Desc", hangoutDate, 10);
            this.mockHangoutRepository.Setup(repository => repository.GetHangoutById(1)).Returns(hangout);
            this.mockParticipantRepository.Setup(repository => repository.GetAllParticipants()).Returns(new List<(int, int)>());
            this.mockAppointmentRepository.Setup(repository => repository.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>
                {
                    new Appointment { Doctor = new Doctor { StaffID = 1 }, Date = hangoutDate.Date, Status = "Scheduled", StartTime = TimeSpan.FromHours(9) },
                });

            Assert.Throws<InvalidOperationException>(() => this.service.JoinHangout(1, this.doctor1));
        }

        [Test]
        public void JoinHangout_Valid_AddsParticipant()
        {
            var hangout = new Hangout(1, "Title", "Desc", DateTime.Now.AddDays(10), 10);
            this.mockHangoutRepository.Setup(repository => repository.GetHangoutById(1)).Returns(hangout);
            this.mockParticipantRepository.Setup(repository => repository.GetAllParticipants()).Returns(new List<(int, int)>());

            this.service.JoinHangout(1, this.doctor1);
            this.mockParticipantRepository.Verify(repository => repository.AddParticipant(1, 1), Times.Once);
        }

        // --- GetAllHangouts Tests ---
        [Test]
        public void GetAllHangouts_ReturnsHangoutsWithParticipants()
        {
            var hangout = new Hangout(1, "Title", "Desc", DateTime.Now.AddDays(10), 10);
            this.mockHangoutRepository.Setup(repository => repository.GetAllHangouts()).Returns(new List<Hangout> { hangout });
            this.mockParticipantRepository.Setup(repository => repository.GetAllParticipants()).Returns(new List<(int, int)> { (1, 1) });
            this.mockStaffRepository.Setup(repository => repository.LoadAllStaff()).Returns(new List<IStaff> { this.doctor1 });

            var result = this.service.GetAllHangouts();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].ParticipantList.Count, Is.EqualTo(1));
        }
    }
}