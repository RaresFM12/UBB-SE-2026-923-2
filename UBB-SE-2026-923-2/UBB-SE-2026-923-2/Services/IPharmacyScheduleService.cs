namespace UBB_SE_2026_923_2.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UBB_SE_2026_923_2.Models;

public interface IPharmacyScheduleService
{
    Task<IReadOnlyList<Shift>> GetShiftsAsync(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd);

    List<Pharmacyst> GetPharmacists();
}
