namespace UBB_SE_2026_923_2.Services
{
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;

    public interface IShiftSwapService
    {
        List<Shift> GetFutureShiftsForStaff(int staffId);

        List<IStaff> GetEligibleSwapColleaguesForShift(int requesterId, int shiftId, out string error);

        bool RequestShiftSwap(int requesterId, int shiftId, int colleagueId, out string message);

        List<ShiftSwapRequest> GetIncomingSwapRequests(int colleagueId);

        bool AcceptSwapRequest(int swapId, int colleagueId, out string message);

        bool RejectSwapRequest(int swapId, int colleagueId, out string message);

        List<Doctor> GetAllDoctors();

        List<ShiftSwapRequest> GetAllShiftSwapRequests();
    }
}
