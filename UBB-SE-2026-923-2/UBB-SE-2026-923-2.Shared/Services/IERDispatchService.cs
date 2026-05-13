namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBB_SE_2026_923_2.Models;

    public interface IERDispatchService
    {
        Task<IReadOnlyList<int>> SimulateIncomingRequestsAsync(int count);

        Task<IReadOnlyList<int>> GetPendingRequestIdsAsync();

        Task<ERDispatchResult> DispatchERRequestAsync(int requestId);

        Task<ERDispatchResult> ManualOverrideAsync(int requestId, int doctorId, int nearEndMinutes);

        Task<IReadOnlyList<DoctorProfile>> GetManualOverrideCandidatesAsync(int requestId, int nearEndMinutes);
    }
}
