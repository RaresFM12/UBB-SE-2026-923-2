namespace UBB_SE_2026_923_2.Repositories
{
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;

    public interface IPharmacyShiftRepository
    {
        IReadOnlyList<Shift> GetAllShifts();

        void AddShift(Shift shift);
    }
}
