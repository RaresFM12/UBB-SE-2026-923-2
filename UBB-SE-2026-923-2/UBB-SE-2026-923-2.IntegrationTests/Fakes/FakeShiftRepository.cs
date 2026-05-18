namespace UBB_SE_2026_923_2.IntegrationTests.Fakes
{
    using System.Collections.Generic;
    using System.Linq;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;

    public sealed class FakeShiftRepository : IShiftRepository, IShiftManagementShiftRepository, IPharmacyShiftRepository
    {
        private readonly List<Shift> shifts = new();
        private int nextId = 1;

        public IReadOnlyList<Shift> GetAllShifts() => this.shifts.ToList();

        public void AddShift(Shift newShift)
        {
            if (newShift.Id == 0)
            {
                newShift.Id = this.nextId++;
            }

            this.shifts.Add(newShift);
        }

        public void UpdateShiftStatus(int shiftId, ShiftStatus status)
        {
            var shift = this.shifts.FirstOrDefault(item => item.Id == shiftId);
            if (shift != null)
            {
                shift.Status = status;
            }
        }

        public void UpdateShiftStaffId(int shiftId, int newStaffId)
        {
            var shift = this.shifts.FirstOrDefault(item => item.Id == shiftId);
            if (shift != null)
            {
                shift.AppointedStaff.StaffID = newStaffId;
            }
        }

        public void DeleteShift(int shiftId)
        {
            var shift = this.shifts.FirstOrDefault(item => item.Id == shiftId);
            if (shift != null)
            {
                this.shifts.Remove(shift);
            }
        }

        void IPharmacyShiftRepository.AddShift(Shift shift) => this.AddShift(shift);
    }
}
