namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;

    public interface IPharmacyVacationService
    {
        IReadOnlyList<Pharmacyst> GetPharmacists();

        void RegisterVacation(int pharmacistStaffId, DateTime startDate, DateTime endDate);
    }
}
