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
    public class PharmacyScheduleServiceTests
    {
        private Mock<IShiftRepository> mockShiftRepository;
        private Mock<IPharmacyStaffRepository> mockStaffRepository;
        private PharmacyScheduleService service;

        [SetUp]
        public void Setup()
        {
            this.mockShiftRepository = new Mock<IShiftRepository>();
            this.mockStaffRepository = new Mock<IPharmacyStaffRepository>();
            this.service = new PharmacyScheduleService(this.mockShiftRepository.Object, this.mockStaffRepository.Object);
        }

        [Test]
        public async Task GetShiftsAsync_NoShifts_ReturnsEmpty()
        {
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift>());
            var result = await this.service.GetShiftsAsync(1, DateTime.Now, DateTime.Now.AddDays(7));
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetShiftsAsync_ShiftsForDifferentStaff_ReturnsEmpty()
        {
            var staff = new Doctor { StaffID = 2 };
            var shift = new Shift(1, staff, "Pharmacy", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await this.service.GetShiftsAsync(1, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetShiftsAsync_ShiftInRange_ReturnsIt()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var now = DateTime.Now;
            var shift = new Shift(1, staff, "Pharmacy", now, now.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await this.service.GetShiftsAsync(1, now.AddHours(-1), now.AddHours(10));
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetShiftsAsync_ShiftOutOfRange_ReturnsEmpty()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var shift = new Shift(1, staff, "Pharmacy", DateTime.Now.AddDays(10), DateTime.Now.AddDays(10).AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await this.service.GetShiftsAsync(1, DateTime.Now, DateTime.Now.AddDays(1));
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetShiftsAsync_MultipleShifts_ReturnsOrderedByStart()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var now = DateTime.Now;
            var shift1 = new Shift(1, staff, "Pharmacy", now.AddHours(5), now.AddHours(10), ShiftStatus.SCHEDULED);
            var shift2 = new Shift(2, staff, "Pharmacy", now.AddHours(1), now.AddHours(4), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift1, shift2 });

            var result = await this.service.GetShiftsAsync(1, now, now.AddDays(1));
            Assert.That(result[0].Id, Is.EqualTo(2));
            Assert.That(result[1].Id, Is.EqualTo(1));
        }

        [Test]
        public void GetPharmacists_ReturnsFromRepo()
        {
            var pharmacists = new List<Pharmacyst>
            {
                new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5),
                new Pharmacyst(2, "C", "D", string.Empty, true, "cert2", 3),
            };
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(pharmacists);
            var result = this.service.GetPharmacists();
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetPharmacists_EmptyRepo_ReturnsEmpty()
        {
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst>());
            var result = this.service.GetPharmacists();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetShiftsAsync_ShiftStartsAtRangeEnd_NotIncluded()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var rangeEnd = new DateTime(2025, 6, 1, 8, 0, 0);
            var shift = new Shift(1, staff, "Pharmacy", rangeEnd, rangeEnd.AddHours(8), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await this.service.GetShiftsAsync(1, new DateTime(2025, 5, 31), rangeEnd);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetShiftsAsync_ShiftEndsAtRangeStart_NotIncluded()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var rangeStart = new DateTime(2025, 6, 1, 8, 0, 0);
            var shift = new Shift(1, staff, "Pharmacy", rangeStart.AddHours(-8), rangeStart, ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await this.service.GetShiftsAsync(1, rangeStart, rangeStart.AddDays(1));
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetShiftsAsync_ShiftPartiallyOverlapsStart_Included()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var rangeStart = new DateTime(2025, 6, 1, 8, 0, 0);
            var shift = new Shift(1, staff, "Pharmacy", rangeStart.AddHours(-2), rangeStart.AddHours(2), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await this.service.GetShiftsAsync(1, rangeStart, rangeStart.AddDays(1));
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetShiftsAsync_ShiftPartiallyOverlapsEnd_Included()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var rangeEnd = new DateTime(2025, 6, 2, 8, 0, 0);
            var shift = new Shift(1, staff, "Pharmacy", rangeEnd.AddHours(-2), rangeEnd.AddHours(2), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await this.service.GetShiftsAsync(1, new DateTime(2025, 6, 1), rangeEnd);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetShiftsAsync_ManyShiftsForStaff_ReturnsAllInRange()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var baseDate = new DateTime(2025, 6, 1);
            var shifts = new List<Shift>();
            for (int shiftIndex = 0; shiftIndex < 10; shiftIndex++)
            {
                shifts.Add(new Shift(shiftIndex + 1, staff, "Pharmacy", baseDate.AddHours(shiftIndex * 2), baseDate.AddHours((shiftIndex * 2) + 1), ShiftStatus.SCHEDULED));
            }

            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(shifts);

            var result = await this.service.GetShiftsAsync(1, baseDate, baseDate.AddDays(1));
            Assert.That(result.Count, Is.EqualTo(10));
        }

        [Test]
        public async Task GetShiftsAsync_VacationShift_StillReturned()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var now = DateTime.Now;
            var shift = new Shift(1, staff, "Vacation", now, now.AddHours(8), ShiftStatus.VACATION);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await this.service.GetShiftsAsync(1, now.AddHours(-1), now.AddHours(10));
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetShiftsAsync_MixedStaff_OnlyReturnsMatching()
        {
            var staff1 = new Pharmacyst { StaffID = 1 };
            var staff2 = new Pharmacyst { StaffID = 2 };
            var now = DateTime.Now;
            var shifts = new List<Shift>
            {
                new Shift(1, staff1, "Pharmacy", now, now.AddHours(8), ShiftStatus.SCHEDULED),
                new Shift(2, staff2, "Pharmacy", now, now.AddHours(8), ShiftStatus.SCHEDULED),
                new Shift(3, staff1, "Pharmacy", now.AddHours(1), now.AddHours(5), ShiftStatus.SCHEDULED),
            };
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(shifts);

            var result = await this.service.GetShiftsAsync(1, now.AddHours(-1), now.AddHours(10));
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetShiftsAsync_SameStartAndEndRange_ReturnsEmpty()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var time = new DateTime(2025, 6, 1, 12, 0, 0);
            var shift = new Shift(1, staff, "Pharmacy", time, time.AddHours(1), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await this.service.GetShiftsAsync(1, time, time);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetPharmacists_SinglePharmacist_ReturnsOne()
        {
            var pharmacists = new List<Pharmacyst> { new Pharmacyst(1, "A", "B", string.Empty, true, "cert", 5) };
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(pharmacists);
            var result = this.service.GetPharmacists();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].StaffID, Is.EqualTo(1));
        }

        [Test]
        public void GetPharmacists_ManyPharmacists_ReturnsAll()
        {
            var pharmacists = new List<Pharmacyst>();
            for (int pharmacistIndex = 1; pharmacistIndex <= 20; pharmacistIndex++)
            {
                pharmacists.Add(new Pharmacyst(pharmacistIndex, $"First{pharmacistIndex}", $"Last{pharmacistIndex}", string.Empty, true, $"cert{pharmacistIndex}", pharmacistIndex));
            }

            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(pharmacists);
            var result = this.service.GetPharmacists();
            Assert.That(result.Count, Is.EqualTo(20));
        }

        [Test]
        public async Task GetShiftsAsync_ThreeShifts_ReturnsSortedByStartTime()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var baseDate = new DateTime(2025, 6, 1);
            var shifts = new List<Shift>
            {
                new Shift(1, staff, "P", baseDate.AddHours(10), baseDate.AddHours(12), ShiftStatus.SCHEDULED),
                new Shift(2, staff, "P", baseDate.AddHours(2), baseDate.AddHours(4), ShiftStatus.SCHEDULED),
                new Shift(3, staff, "P", baseDate.AddHours(6), baseDate.AddHours(8), ShiftStatus.SCHEDULED),
            };
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(shifts);

            var result = await this.service.GetShiftsAsync(1, baseDate, baseDate.AddDays(1));
            Assert.That(result[0].Id, Is.EqualTo(2));
            Assert.That(result[1].Id, Is.EqualTo(3));
            Assert.That(result[2].Id, Is.EqualTo(1));
        }

        [Test]
        public async Task GetShiftsAsync_ShiftSpansEntireRange_Included()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var rangeStart = new DateTime(2025, 6, 1);
            var rangeEnd = new DateTime(2025, 6, 2);
            var shift = new Shift(1, staff, "P", rangeStart.AddHours(-5), rangeEnd.AddHours(5), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await this.service.GetShiftsAsync(1, rangeStart, rangeEnd);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetShiftsAsync_ShiftFullyInsideRange_Included()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var rangeStart = new DateTime(2025, 6, 1);
            var rangeEnd = new DateTime(2025, 6, 3);
            var shift = new Shift(1, staff, "P", rangeStart.AddHours(5), rangeEnd.AddHours(-5), ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await this.service.GetShiftsAsync(1, rangeStart, rangeEnd);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetPharmacists_ReturnsListType()
        {
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst>());
            var result = this.service.GetPharmacists();
            Assert.That(result, Is.InstanceOf<List<Pharmacyst>>());
        }

        [Test]
        public async Task GetShiftsAsync_CancelledShift_StillReturned()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var now = DateTime.Now;
            var shift = new Shift(1, staff, "Pharmacy", now, now.AddHours(8), ShiftStatus.CANCELLED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await this.service.GetShiftsAsync(1, now.AddHours(-1), now.AddHours(10));
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetShiftsAsync_CompletedShift_StillReturned()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var now = DateTime.Now;
            var shift = new Shift(1, staff, "Pharmacy", now, now.AddHours(8), ShiftStatus.COMPLETED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await this.service.GetShiftsAsync(1, now.AddHours(-1), now.AddHours(10));
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetShiftsAsync_ActiveShift_Returned()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var now = DateTime.Now;
            var shift = new Shift(1, staff, "Pharmacy", now, now.AddHours(8), ShiftStatus.ACTIVE);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await this.service.GetShiftsAsync(1, now.AddHours(-1), now.AddHours(10));
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetPharmacists_PreservesStaffProperties()
        {
            var pharmacist = new Pharmacyst(5, "John", "Doe", "contact", true, "PharmD", 10);
            this.mockStaffRepository.Setup(repository => repository.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            var result = this.service.GetPharmacists();
            Assert.That(result[0].FirstName, Is.EqualTo("John"));
            Assert.That(result[0].LastName, Is.EqualTo("Doe"));
        }

        [Test]
        public async Task GetShiftsAsync_ShiftExactlyMatchesRange_Included()
        {
            var staff = new Pharmacyst { StaffID = 1 };
            var start = new DateTime(2025, 6, 1, 8, 0, 0);
            var end = new DateTime(2025, 6, 1, 16, 0, 0);
            var shift = new Shift(1, staff, "Pharmacy", start, end, ShiftStatus.SCHEDULED);
            this.mockShiftRepository.Setup(repository => repository.GetAllShifts()).Returns(new List<Shift> { shift });

            var result = await this.service.GetShiftsAsync(1, start, end);
            Assert.That(result.Count, Is.EqualTo(1));
        }
    }
}
