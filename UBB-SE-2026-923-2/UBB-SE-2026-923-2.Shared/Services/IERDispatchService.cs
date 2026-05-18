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

        // Read/create helpers used by the web GUI (the desktop never needed
        // these because its dispatch screen is fire-and-forget). They stay in
        // the service so the MVC controller never touches a repository.
        Task<IReadOnlyList<ERRequest>> GetAllRequestsAsync();

        Task<ERRequest?> GetRequestByIdAsync(int requestId);

        Task<int> CreateRequestAsync(string specialization, string location);

        Task UpdateRequestStatusAsync(int requestId, string status);

        Task<IReadOnlyList<ERDispatchResult>> DispatchAllPendingAsync();
    }
}
