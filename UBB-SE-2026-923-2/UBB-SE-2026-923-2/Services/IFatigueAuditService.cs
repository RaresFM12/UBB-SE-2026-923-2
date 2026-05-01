using System;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Services
{
    public interface IFatigueAuditService
    {
        AutoAuditResult RunAutoAudit(DateTime weekStart);

        bool ReassignShift(int shiftId, int newStaffId);
    }
}

