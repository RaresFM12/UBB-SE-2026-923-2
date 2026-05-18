using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.IntegrationTests;

public class FakeHangoutService : IHangoutService
{
    private const int CreatedHangoutId = 1;

    public int CreateHangout(string title, string description, DateTime date, int maxParticipants, IStaff creator) => CreatedHangoutId;

    public void JoinHangout(int hangoutId, IStaff staff) { }

    public List<Hangout> GetAllHangouts() => new List<Hangout>();
}
