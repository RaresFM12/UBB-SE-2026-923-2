namespace UBB_SE_2026_923_2.Repositories
{
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;

    public interface IHangoutRepository
    {
        int AddHangout(string title, string description, System.DateTime date, int maximumParticipants);

        List<Hangout> GetAllHangouts();

        Hangout? GetHangoutById(int hangoutId);
    }
}