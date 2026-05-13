namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;

    public sealed class PharmacyVacationService : IPharmacyVacationService
    {
        private const int OneDay = 1;
        private const int FirstShiftId = 1;
        private const int IdIncrement = 1;
        private const int EmptyShiftCollectionCount = 0;
        private const string VacationShiftLocationLabel = "Vacation";

        private readonly IPharmacyStaffRepository staffRepository;
        private readonly IPharmacyShiftRepository shiftRepository;

        public PharmacyVacationService(IPharmacyStaffRepository staffRepository, IPharmacyShiftRepository shiftRepository)
        {
            this.staffRepository = staffRepository ?? throw new ArgumentNullException(nameof(staffRepository));
            this.shiftRepository = shiftRepository ?? throw new ArgumentNullException(nameof(shiftRepository));
        }

        public IReadOnlyList<Pharmacyst> GetPharmacists()
        {
            string ByFirstName(Pharmacyst pharmacist) => pharmacist.FirstName;
            string ByLastName(Pharmacyst pharmacist) => pharmacist.LastName;

            return this.staffRepository
                .GetPharmacists()
                .OrderBy(ByFirstName)
                .ThenBy(ByLastName)
                .ToList();
        }

        public void RegisterVacation(int pharmacistStaffId, DateTime startDate, DateTime endDate)
        {
            var start = startDate.Date;
            var endExclusive = endDate.Date.AddDays(OneDay);

            if (endDate.Date < start)
            {
                throw new ArgumentException("End date must be on or after start date.");
            }

            bool HasMatchingStaffId(Pharmacyst existingPharmacist) => existingPharmacist.StaffID == pharmacistStaffId;
            var pharmacist = this.staffRepository
                .GetPharmacists()
                .FirstOrDefault(HasMatchingStaffId)
                ?? throw new ArgumentException("Pharmacist not found.");

            bool IsForPharmacist(Shift existingShift) => existingShift.AppointedStaff.StaffID == pharmacistStaffId;
            var pharmacistShifts = this.shiftRepository.GetAllShifts().Where(IsForPharmacist).ToList();

            bool OverlapsVacationPeriod(Shift shift) => start < shift.EndTime && endExclusive > shift.StartTime;
            var overlappingShift = pharmacistShifts.FirstOrDefault(OverlapsVacationPeriod);

            if (overlappingShift is not null)
            {
                bool isExistingVacation = overlappingShift.Status == ShiftStatus.VACATION;
                throw new InvalidOperationException(isExistingVacation
                    ? "Cannot add vacation: this period overlaps an existing vacation entry."
                    : "Cannot add vacation: this period overlaps an existing shift.");
            }

            int ByShiftId(Shift shift) => shift.Id;
            var allShifts = this.shiftRepository.GetAllShifts();
            var nextId = allShifts.Count == EmptyShiftCollectionCount
                ? FirstShiftId
                : allShifts.Max(ByShiftId) + IdIncrement;

            var vacationShift = new Shift(
                nextId,
                pharmacist,
                VacationShiftLocationLabel,
                start,
                endExclusive,
                ShiftStatus.VACATION);

            this.shiftRepository.AddShift(vacationShift);
        }
    }
}
