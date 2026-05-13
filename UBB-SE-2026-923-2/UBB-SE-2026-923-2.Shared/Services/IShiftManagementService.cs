namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;

    public interface IShiftManagementService
    {
        void SetShiftActive(int shiftId);

        void CancelShift(int shiftId);

        bool ValidateNoOverlap(int staffId, DateTime start, DateTime end);

        void AddShift(Shift shift);

        bool TryAddShift(IStaff staff, DateTime start, DateTime end, string location);

        bool ValidateShiftTimes(TimeSpan start, TimeSpan end);

        List<Shift> GetDailyShifts(DateTime date);

        List<Shift> GetWeeklyShifts(DateTime date);

        bool ReassignShift(Shift shift, IStaff newStaff);

        List<IStaff> GetFilteredStaff(string location, string requiredSpecializationOrCertification);

        List<IStaff> FindStaffReplacements(Shift shift);

        List<string> GetSpecializationsAndCertificationsForLocation(string location);

        float GetWeeklyHours(int staffId);

        List<Shift> GetActiveShifts();

        bool IsStaffWorkingDuring(int staffId, DateTime startTime, DateTime endTime);
    }
}
