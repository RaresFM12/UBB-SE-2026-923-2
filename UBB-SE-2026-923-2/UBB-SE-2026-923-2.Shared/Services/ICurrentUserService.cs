namespace UBB_SE_2026_923_2.Services
{
    using UBB_SE_2026_923_2.Models;

    public interface ICurrentUserService
    {
        int UserId { get; set; }

        string Role { get; }

        UserRole RoleType { get; set; }

        void SetFromUser(User user);
    }
}
