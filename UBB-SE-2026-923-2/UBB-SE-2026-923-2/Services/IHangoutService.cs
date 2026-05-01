using System.Collections.Generic;
using System;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Services
{
    public interface IHangoutService
    {
        int CreateHangout(string title, string description, DateTime date, int maxParticipants, IStaff creator);
        void JoinHangout(int hangoutId, IStaff staff);
        List<Hangout> GetAllHangouts();
    }
}
