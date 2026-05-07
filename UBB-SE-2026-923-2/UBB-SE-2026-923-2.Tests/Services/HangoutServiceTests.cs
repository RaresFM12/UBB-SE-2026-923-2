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
    public class HangoutServiceTests
    {
        private Mock<IHangoutRepository> mockHangoutRepo;
        private Mock<IHangoutParticipantRepository> mockParticipantRepo;
        private Mock<IAppointmentRepository> mockAppointmentRepo;
        private Mock<IStaffRepository> mockStaffRepo;
        private Mock<IEvaluationsRepository> mockEvalRepo;
        private HangoutService service;
        private Doctor doctor1;

        [SetUp]
        public void Setup()
        {
            mockHangoutRepo = new Mock<IHangoutRepository>();
            mockParticipantRepo = new Mock<IHangoutParticipantRepository>();
            mockAppointmentRepo = new Mock<IAppointmentRepository>();
            mockStaffRepo = new Mock<IStaffRepository>();
            mockEvalRepo = new Mock<IEvaluationsRepository>();
            service = new HangoutService(
                mockHangoutRepo.Object,
                mockParticipantRepo.Object,
                mockAppointmentRepo.Object,
                mockStaffRepo.Object,
                mockEvalRepo.Object);

            doctor1 = new Doctor(1, "John", "Doe", "c", true, "Gen", "L1", DoctorStatus.AVAILABLE, 5);
            mockAppointmentRepo.Setup(r => r.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>());
            mockEvalRepo.Setup(r => r.GetAllEvaluations()).Returns(new List<MedicalEvaluation>());
        }

        [Test]
        public void CreateHangout_TitleTooShort_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                service.CreateHangout("Hi", "desc", DateTime.Now.AddDays(10), 5, doctor1));
        }

        [Test]
        public void CreateHangout_TitleTooLong_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                service.CreateHangout(new string('A', 30), "desc", DateTime.Now.AddDays(10), 5, doctor1));
        }

        [Test]
        public void CreateHangout_TitleNull_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                service.CreateHangout(null, "desc", DateTime.Now.AddDays(10), 5, doctor1));
        }

        [Test]
        public void CreateHangout_TitleWhitespace_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                service.CreateHangout("   ", "desc", DateTime.Now.AddDays(10), 5, doctor1));
        }

        [Test]
        public void CreateHangout_DescriptionTooLong_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                service.CreateHangout("ValidTitle", new string('A', 101), DateTime.Now.AddDays(10), 5, doctor1));
        }

        [Test]
        public void CreateHangout_DateTooSoon_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                service.CreateHangout("ValidTitle", "desc", DateTime.Now.AddDays(3), 5, doctor1));
        }

        [Test]
        public void CreateHangout_ConflictingAppointment_Throws()
        {
            var hangoutDate = DateTime.Now.AddDays(10);
            mockAppointmentRepo.Setup(r => r.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>
                {
                    new Appointment { DoctorId = 1, Date = hangoutDate.Date, Status = "Scheduled", StartTime = TimeSpan.FromHours(9) }
                });

            Assert.Throws<InvalidOperationException>(() =>
                service.CreateHangout("ValidTitle", "desc", hangoutDate, 5, doctor1));
        }

        [Test]
        public void CreateHangout_MedicalEvalOnDate_Throws()
        {
            var hangoutDate = DateTime.Now.AddDays(10);
            mockEvalRepo.Setup(r => r.GetAllEvaluations()).Returns(new List<MedicalEvaluation>
            {
                new MedicalEvaluation { Evaluator = doctor1, EvaluationDate = hangoutDate.Date }
            });

            Assert.Throws<InvalidOperationException>(() =>
                service.CreateHangout("ValidTitle", "desc", hangoutDate, 5, doctor1));
        }

        [Test]
        public void CreateHangout_Valid_ReturnsId()
        {
            mockHangoutRepo.Setup(r => r.AddHangout(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(42);

            var result = service.CreateHangout("ValidTitle", "desc", DateTime.Now.AddDays(10), 5, doctor1);
            Assert.That(result, Is.EqualTo(42));
            mockParticipantRepo.Verify(r => r.AddParticipant(42, 1), Times.Once);
        }

        [Test]
        public void CreateHangout_NullDescription_Works()
        {
            mockHangoutRepo.Setup(r => r.AddHangout(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(1);
            Assert.DoesNotThrow(() => service.CreateHangout("ValidTitle", null, DateTime.Now.AddDays(10), 5, doctor1));
        }

        [Test]
        public void JoinHangout_HangoutNotFound_Throws()
        {
            mockHangoutRepo.Setup(r => r.GetHangoutById(1)).Returns((Hangout)null);
            Assert.Throws<ArgumentException>(() => service.JoinHangout(1, doctor1));
        }

        [Test]
        public void JoinHangout_HangoutFull_Throws()
        {
            var hangout = new Hangout(1, "Title", "Desc", DateTime.Now.AddDays(10), 1);
            mockHangoutRepo.Setup(r => r.GetHangoutById(1)).Returns(hangout);
            mockParticipantRepo.Setup(r => r.GetAllParticipants()).Returns(new List<(int, int)> { (1, 2) });

            Assert.Throws<InvalidOperationException>(() => service.JoinHangout(1, doctor1));
        }

        [Test]
        public void JoinHangout_AlreadyJoined_Throws()
        {
            var hangout = new Hangout(1, "Title", "Desc", DateTime.Now.AddDays(10), 10);
            mockHangoutRepo.Setup(r => r.GetHangoutById(1)).Returns(hangout);
            mockParticipantRepo.Setup(r => r.GetAllParticipants()).Returns(new List<(int, int)> { (1, 1) });

            Assert.Throws<InvalidOperationException>(() => service.JoinHangout(1, doctor1));
        }

        [Test]
        public void JoinHangout_ConflictingAppointment_Throws()
        {
            var hangoutDate = DateTime.Now.AddDays(10);
            var hangout = new Hangout(1, "Title", "Desc", hangoutDate, 10);
            mockHangoutRepo.Setup(r => r.GetHangoutById(1)).Returns(hangout);
            mockParticipantRepo.Setup(r => r.GetAllParticipants()).Returns(new List<(int, int)>());
            mockAppointmentRepo.Setup(r => r.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>
                {
                    new Appointment { DoctorId = 1, Date = hangoutDate.Date, Status = "Scheduled", StartTime = TimeSpan.FromHours(9) }
                });

            Assert.Throws<InvalidOperationException>(() => service.JoinHangout(1, doctor1));
        }

        [Test]
        public void JoinHangout_Valid_AddsParticipant()
        {
            var hangout = new Hangout(1, "Title", "Desc", DateTime.Now.AddDays(10), 10);
            mockHangoutRepo.Setup(r => r.GetHangoutById(1)).Returns(hangout);
            mockParticipantRepo.Setup(r => r.GetAllParticipants()).Returns(new List<(int, int)>());

            service.JoinHangout(1, doctor1);
            mockParticipantRepo.Verify(r => r.AddParticipant(1, 1), Times.Once);
        }

        [Test]
        public void GetAllHangouts_ReturnsHangoutsWithParticipants()
        {
            var hangout = new Hangout(1, "Title", "Desc", DateTime.Now.AddDays(10), 10);
            mockHangoutRepo.Setup(r => r.GetAllHangouts()).Returns(new List<Hangout> { hangout });
            mockParticipantRepo.Setup(r => r.GetAllParticipants()).Returns(new List<(int, int)> { (1, 1) });
            mockStaffRepo.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { doctor1 });

            var result = service.GetAllHangouts();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].ParticipantList.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetAllHangouts_Empty_ReturnsEmpty()
        {
            mockHangoutRepo.Setup(r => r.GetAllHangouts()).Returns(new List<Hangout>());
            mockParticipantRepo.Setup(r => r.GetAllParticipants()).Returns(new List<(int, int)>());
            mockStaffRepo.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff>());

            var result = service.GetAllHangouts();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void JoinHangout_MedicalEvalOnDate_Throws()
        {
            var hangoutDate = DateTime.Now.AddDays(10);
            var hangout = new Hangout(1, "Title", "Desc", hangoutDate, 10);
            mockHangoutRepo.Setup(r => r.GetHangoutById(1)).Returns(hangout);
            mockParticipantRepo.Setup(r => r.GetAllParticipants()).Returns(new List<(int, int)>());
            mockEvalRepo.Setup(r => r.GetAllEvaluations()).Returns(new List<MedicalEvaluation>
            {
                new MedicalEvaluation { Evaluator = doctor1, EvaluationDate = hangoutDate.Date }
            });

            Assert.Throws<InvalidOperationException>(() => service.JoinHangout(1, doctor1));
        }

        [Test]
        public void CreateHangout_ExactMinTitleLength_Works()
        {
            mockHangoutRepo.Setup(r => r.AddHangout(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(1);
            Assert.DoesNotThrow(() => service.CreateHangout("Hello", "desc", DateTime.Now.AddDays(10), 5, doctor1));
        }

        [Test]
        public void CreateHangout_ExactMaxTitleLength_Works()
        {
            mockHangoutRepo.Setup(r => r.AddHangout(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(1);
            Assert.DoesNotThrow(() => service.CreateHangout(new string('A', 25), "desc", DateTime.Now.AddDays(10), 5, doctor1));
        }

        [Test]
        public void CreateHangout_ExactlyOneWeekAhead_Throws()
        {
            // Must be AT LEAST 7 days ahead, so exactly 6 days should throw
            Assert.Throws<ArgumentException>(() =>
                service.CreateHangout("ValidTitle", "desc", DateTime.Now.AddDays(6), 5, doctor1));
        }
    }
}
