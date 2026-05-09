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
    public class ShiftSwapServiceLogicTests
    {
        private Mock<IStaffRepository> mockStaffRepository;
        private Mock<IShiftRepository> mockShiftRepository;
        private Mock<IShiftSwapRepository> mockShiftSwapRepository;
        private Mock<INotificationRepository> mockNotificationRepository;
        private ShiftSwapService shiftSwapService;

        [SetUp]
        public void Setup()
        {
            this.mockStaffRepository = new Mock<IStaffRepository>();
            this.mockShiftRepository = new Mock<IShiftRepository>();
            this.mockShiftSwapRepository = new Mock<IShiftSwapRepository>();
            this.mockNotificationRepository = new Mock<INotificationRepository>();

            this.shiftSwapService = new ShiftSwapService(
                this.mockStaffRepository.Object,
                this.mockShiftRepository.Object,
                this.mockShiftSwapRepository.Object,
                this.mockNotificationRepository.Object);
        }

        [Test]
        public void GetFutureShiftsForStaff_WhenStaffHasPastAndFutureShifts_ReturnsOnlyFutureShifts()
        {
            var doctor = CreateDoctor(1, "Cardiology");

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift>
                {
                    new Shift(1, doctor, "Ward A", DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1).AddHours(8), ShiftStatus.ACTIVE),
                    new Shift(2, doctor, "Ward A", DateTime.Now.AddDays(2), DateTime.Now.AddDays(2).AddHours(8), ShiftStatus.ACTIVE),
                });

            var futureShifts = this.shiftSwapService.GetFutureShiftsForStaff(1);

            Assert.That(futureShifts.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetEligibleSwapColleaguesForShift_WhenShiftDoesNotBelongToRequester_ReturnsErrorMessage()
        {
            var shiftOwner = CreateDoctor(1, "Cardiology");

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift>
                {
                    new Shift(5, shiftOwner, "Ward A", DateTime.Now.AddDays(2), DateTime.Now.AddDays(2).AddHours(8), ShiftStatus.SCHEDULED),
                });

            this.shiftSwapService.GetEligibleSwapColleaguesForShift(9, 5, out var errorMessage);

            Assert.That(errorMessage, Is.EqualTo("You can only request swap for your own shift."));
        }

        [Test]
        public void GetEligibleSwapColleaguesForShift_WhenColleagueHasSameSpecializationAndNoOverlap_ReturnsColleague()
        {
            var requesterDoctor = CreateDoctor(1, "Cardiology");
            var colleagueDoctor = CreateDoctor(2, "cardiology");
            var requesterShift = new Shift(5, requesterDoctor, "Ward A", DateTime.Now.AddDays(2), DateTime.Now.AddDays(2).AddHours(8), ShiftStatus.SCHEDULED);

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift> { requesterShift });

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff> { requesterDoctor, colleagueDoctor });

            var eligibleColleagues = this.shiftSwapService.GetEligibleSwapColleaguesForShift(1, 5, out var errorMessage);

            Assert.That(eligibleColleagues.Count, Is.EqualTo(1));
        }

        [Test]
        public void RequestShiftSwap_WhenColleagueIsEligible_CreatesShiftSwapRequest()
        {
            var requesterDoctor = CreateDoctor(1, "Cardiology");
            var colleagueDoctor = CreateDoctor(2, "Cardiology");
            var requesterShift = new Shift(5, requesterDoctor, "Ward A", DateTime.Now.AddDays(2), DateTime.Now.AddDays(2).AddHours(8), ShiftStatus.SCHEDULED);

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift> { requesterShift });

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff> { requesterDoctor, colleagueDoctor });

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.GetStaffById(1))
                .Returns(requesterDoctor);

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.GetStaffById(2))
                .Returns(colleagueDoctor);

            this.mockShiftSwapRepository
                .Setup(shiftSwapRepository => shiftSwapRepository.AddShiftSwapRequest(It.IsAny<ShiftSwapRequest>()))
                .Returns(10);

            this.shiftSwapService.RequestShiftSwap(1, 5, 2, out var requestMessage);

            this.mockShiftSwapRepository.Verify(
                shiftSwapRepository => shiftSwapRepository.AddShiftSwapRequest(It.IsAny<ShiftSwapRequest>()),
                Times.Once);
        }

        [Test]
        public void RequestShiftSwap_WhenSelectedColleagueIsNotEligible_ReturnsFalse()
        {
            var requesterDoctor = CreateDoctor(1, "Cardiology");
            var colleagueDoctor = CreateDoctor(2, "Neurology");
            var requesterShift = new Shift(5, requesterDoctor, "Ward A", DateTime.Now.AddDays(2), DateTime.Now.AddDays(2).AddHours(8), ShiftStatus.SCHEDULED);

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift> { requesterShift });

            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff> { requesterDoctor, colleagueDoctor });

            var requestResult = this.shiftSwapService.RequestShiftSwap(1, 5, 2, out var requestMessage);

            Assert.That(requestResult, Is.False);
        }

        [Test]
        public void GetIncomingSwapRequests_WhenRequestsHaveDifferentColleagues_ReturnsOnlyPendingRequestsForRequestedColleague()
        {
            var requesterDoctor = CreateDoctor(1, "Cardiology");
            var requestedColleagueDoctor = CreateDoctor(2, "Cardiology");
            var differentColleagueDoctor = CreateDoctor(3, "Cardiology");
            var requestedShift = new Shift(5, requesterDoctor, "Ward A", DateTime.Now.AddDays(2), DateTime.Now.AddDays(2).AddHours(8), ShiftStatus.SCHEDULED);

            this.mockShiftSwapRepository
                .Setup(shiftSwapRepository => shiftSwapRepository.GetAllShiftSwapRequests())
                .Returns(new List<ShiftSwapRequest>
                {
                    new ShiftSwapRequest(1, requestedShift, requesterDoctor, requestedColleagueDoctor),
                    new ShiftSwapRequest(2, requestedShift, requesterDoctor, differentColleagueDoctor),
                });

            var incomingRequests = this.shiftSwapService.GetIncomingSwapRequests(2);

            Assert.That(incomingRequests.Count, Is.EqualTo(1));
        }

        [Test]
        public void AcceptSwapRequest_WhenSwapRequestIsValid_UpdatesShiftStaffIdentifier()
        {
            var requesterDoctor = CreateDoctor(1, "Cardiology");
            var colleagueDoctor = CreateDoctor(2, "Cardiology");
            var requestedShift = new Shift(5, requesterDoctor, "Ward A", DateTime.Now.AddDays(2), DateTime.Now.AddDays(2).AddHours(8), ShiftStatus.SCHEDULED);
            var swapRequest = new ShiftSwapRequest(10, requestedShift, requesterDoctor, colleagueDoctor);

            this.mockShiftSwapRepository
                .Setup(shiftSwapRepository => shiftSwapRepository.GetShiftSwapRequestById(10))
                .Returns(swapRequest);

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift> { requestedShift });

            this.shiftSwapService.AcceptSwapRequest(10, 2, out var acceptMessage);

            this.mockShiftRepository.Verify(
                shiftRepository => shiftRepository.UpdateShiftStaffId(5, 2),
                Times.Once);
        }

        [Test]
        public void AcceptSwapRequest_WhenSwapRequestDoesNotBelongToColleague_ReturnsFalse()
        {
            var requesterDoctor = CreateDoctor(1, "Cardiology");
            var colleagueDoctor = CreateDoctor(2, "Cardiology");
            var requestedShift = new Shift(5, requesterDoctor, "Ward A", DateTime.Now.AddDays(2), DateTime.Now.AddDays(2).AddHours(8), ShiftStatus.SCHEDULED);
            var swapRequest = new ShiftSwapRequest(10, requestedShift, requesterDoctor, colleagueDoctor);

            this.mockShiftSwapRepository
                .Setup(shiftSwapRepository => shiftSwapRepository.GetShiftSwapRequestById(10))
                .Returns(swapRequest);

            var acceptResult = this.shiftSwapService.AcceptSwapRequest(10, 99, out var acceptMessage);

            Assert.That(acceptResult, Is.False);
        }

        [Test]
        public void GetAllDoctors_WhenStaffContainsDoctorsAndPharmacists_ReturnsOnlyDoctorsOrderedByFirstName()
        {
            this.mockStaffRepository
                .Setup(staffRepository => staffRepository.LoadAllStaff())
                .Returns(new List<IStaff>
                {
                    CreateDoctor(2, "Neurology", "Charlie", "Zeta"),
                    CreatePharmacist(3, "General"),
                    CreateDoctor(1, "Cardiology", "Alice", "Alpha"),
                });

            var doctors = this.shiftSwapService.GetAllDoctors();

            Assert.That(doctors[0].FirstName, Is.EqualTo("Alice"));
        }

        [Test]
        public void RejectSwapRequest_WhenSwapRequestIsValid_UpdatesSwapRequestStatusToRejected()
        {
            var requesterDoctor = CreateDoctor(1, "Cardiology");
            var colleagueDoctor = CreateDoctor(2, "Cardiology");
            var requestedShift = new Shift(5, requesterDoctor, "Ward A", DateTime.Now.AddDays(2), DateTime.Now.AddDays(2).AddHours(8), ShiftStatus.SCHEDULED);
            var swapRequest = new ShiftSwapRequest(10, requestedShift, requesterDoctor, colleagueDoctor);

            this.mockShiftSwapRepository
                .Setup(shiftSwapRepository => shiftSwapRepository.GetShiftSwapRequestById(10))
                .Returns(swapRequest);

            this.shiftSwapService.RejectSwapRequest(10, 2, out var rejectMessage);

            this.mockShiftSwapRepository.Verify(
                shiftSwapRepository => shiftSwapRepository.UpdateShiftSwapRequestStatus(10, "REJECTED"),
                Times.Once);
        }

        private static Doctor CreateDoctor(int doctorIdentifier, string specialization)
        {
            return new Doctor(doctorIdentifier, "John", "Doe", "contract", true, specialization, "License", DoctorStatus.AVAILABLE, 5);
        }

        private static Doctor CreateDoctor(int doctorIdentifier, string specialization, string firstName, string lastName)
        {
            return new Doctor(doctorIdentifier, firstName, lastName, "contract", true, specialization, "License", DoctorStatus.AVAILABLE, 5);
        }

        private static Pharmacyst CreatePharmacist(int pharmacistIdentifier, string certification)
        {
            var pharmacist = new Pharmacyst(pharmacistIdentifier, "Alice", "Smith", "contract", true);
            pharmacist.Certification = certification;
            return pharmacist;
        }
    }
}