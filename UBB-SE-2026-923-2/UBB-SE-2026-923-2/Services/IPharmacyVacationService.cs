using System.Collections.Generic;
using System;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Services
{
    public interface IPharmacyVacationService
    {
        IReadOnlyList<Pharmacyst> GetPharmacists();

        void RegisterVacation(int pharmacistStaffId, DateTime startDate, DateTime endDate);
    }
}
