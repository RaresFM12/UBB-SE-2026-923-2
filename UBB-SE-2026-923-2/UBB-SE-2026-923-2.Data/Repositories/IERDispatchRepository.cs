using System.Collections.Generic;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    public interface IERDispatchRepository
    {
        int AddRequest(string specialization, string location, string status);
        IReadOnlyList<ERRequest> GetAllRequests();
        ERRequest? GetRequestById(int requestId);
        void UpdateRequestStatus(int requestId, string status, int? assignedDoctorId, string? assignedDoctorName);
    }
}
