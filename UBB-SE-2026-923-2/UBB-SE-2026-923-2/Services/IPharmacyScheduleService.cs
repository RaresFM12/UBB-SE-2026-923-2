using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Services;

public interface IPharmacyScheduleService
{
    Task<IReadOnlyList<Shift>> GetShiftsAsync(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd);

    List<Pharmacyst> GetPharmacists();
}
