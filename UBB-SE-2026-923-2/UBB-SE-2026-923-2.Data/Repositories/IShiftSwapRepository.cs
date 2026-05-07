namespace UBB_SE_2026_923_2.Repositories
{
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;

    public interface IShiftSwapRepository
    {
        int AddShiftSwapRequest(ShiftSwapRequest request);

        IReadOnlyList<ShiftSwapRequest> GetAllShiftSwapRequests();

        ShiftSwapRequest? GetShiftSwapRequestById(int swapId);

        void UpdateShiftSwapRequestStatus(int swapId, string status);
    }
}
