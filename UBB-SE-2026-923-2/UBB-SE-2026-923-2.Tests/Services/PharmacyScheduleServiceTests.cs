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
    public class PharmacyScheduleServiceLogicTests
    {
        private Mock<IShiftRepository> mockShiftRepository;
        private Mock<IPharmacyStaffRepository> mockPharmacyStaffRepository;
        private PharmacyScheduleService pharmacyScheduleService;

        [SetUp]
        public void Setup()
        {
            this.mockShiftRepository = new Mock<IShiftRepository>();
            this.mockPharmacyStaffRepository = new Mock<IPharmacyStaffRepository>();

            this.pharmacyScheduleService = new PharmacyScheduleService(
                this.mockShiftRepository.Object,
                this.mockPharmacyStaffRepository.Object);
        }

        [Test]
        public async Task GetShiftsAsync_WhenShiftsExistForMultipleStaffMembers_ReturnsOnlyRequestedStaffShifts()
        {
            var requestedPharmacistIdentifier = 4;
            var requestedPharmacist = CreatePharmacist(requestedPharmacistIdentifier, "Alice", "Smith");
            var differentPharmacist = CreatePharmacist(9, "Bob", "Jones");

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift>
                {
                    new Shift(1, requestedPharmacist, "Pharmacy", DateTime.Today.AddHours(8), DateTime.Today.AddHours(16), ShiftStatus.ACTIVE),
                    new Shift(2, differentPharmacist, "Pharmacy", DateTime.Today.AddHours(8), DateTime.Today.AddHours(16), ShiftStatus.ACTIVE),
                });

            var shiftsForRequestedPharmacist = await this.pharmacyScheduleService.GetShiftsAsync(
                requestedPharmacistIdentifier,
                DateTime.Today,
                DateTime.Today.AddDays(1));

            Assert.That(shiftsForRequestedPharmacist.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetShiftsAsync_WhenShiftsAreUnordered_ReturnsShiftsOrderedByStartTime()
        {
            var requestedPharmacistIdentifier = 4;
            var requestedPharmacist = CreatePharmacist(requestedPharmacistIdentifier, "Alice", "Smith");

            this.mockShiftRepository
                .Setup(shiftRepository => shiftRepository.GetAllShifts())
                .Returns(new List<Shift>
                {
                    new Shift(2, requestedPharmacist, "Pharmacy", DateTime.Today.AddHours(14), DateTime.Today.AddHours(18), ShiftStatus.ACTIVE),
                    new Shift(1, requestedPharmacist, "Pharmacy", DateTime.Today.AddHours(8), DateTime.Today.AddHours(12), ShiftStatus.ACTIVE),
                });

            var shiftsForRequestedPharmacist = await this.pharmacyScheduleService.GetShiftsAsync(
                requestedPharmacistIdentifier,
                DateTime.Today,
                DateTime.Today.AddDays(1));

            Assert.That(shiftsForRequestedPharmacist[0].Id, Is.EqualTo(1));
        }

        private static Pharmacyst CreatePharmacist(int pharmacistIdentifier, string firstName, string lastName)
        {
            return new Pharmacyst(pharmacistIdentifier, firstName, lastName, "contract", true, "", 10);
        }
    }
}