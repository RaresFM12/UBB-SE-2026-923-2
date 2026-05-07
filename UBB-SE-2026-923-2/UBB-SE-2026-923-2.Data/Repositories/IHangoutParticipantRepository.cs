using System.Collections.Generic;

namespace UBB_SE_2026_923_2.Repositories
{
    public interface IHangoutParticipantRepository
    {
        IReadOnlyList<(int HangoutId, int StaffId)> GetAllParticipants();
        void AddParticipant(int hangoutId, int staffId);
    }
}
