using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Services
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        string Role { get; }
        UserRole RoleType { get; set; }
    }
}
