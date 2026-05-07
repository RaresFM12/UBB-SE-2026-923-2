namespace UBB_SE_2026_923_2.Repositories
{
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;

    public interface IShiftManagementStaffRepository
    {
        List<IStaff> LoadAllStaff();

        void UpdateStaffAvailability(int staffId, bool isAvailable, DoctorStatus status = DoctorStatus.OFF_DUTY);
    }
}