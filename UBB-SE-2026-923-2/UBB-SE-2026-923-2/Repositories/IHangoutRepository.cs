using System.Collections.Generic;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    public interface IHangoutRepository
    {
        int AddHangout(string title, string description, System.DateTime date, int maxParticipants);
        List<Hangout> GetAllHangouts();
        Hangout? GetHangoutById(int hangoutId);
    }
}
