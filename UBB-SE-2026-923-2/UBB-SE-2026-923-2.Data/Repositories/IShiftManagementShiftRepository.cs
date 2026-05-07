using System.Collections.Generic;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    public interface IShiftManagementShiftRepository
    {
        IReadOnlyList<Shift> GetAllShifts();
        void AddShift(Shift newShift);
        void UpdateShiftStatus(int shiftId, ShiftStatus status);
        void UpdateShiftStaffId(int shiftId, int newStaffId);
    }
}
