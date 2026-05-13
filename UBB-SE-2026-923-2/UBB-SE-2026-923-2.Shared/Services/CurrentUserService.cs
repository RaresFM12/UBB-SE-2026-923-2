namespace UBB_SE_2026_923_2.Services
{
    using System;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public sealed class CurrentUserService : ICurrentUserService
    {
        private static UserRole roleType = UserRole.Client;
        private static int userId = 0;

        public int UserId
        {
            get => userId;
            set => userId = value;
        }

        public UserRole RoleType
        {
            get => roleType;
            set => roleType = value;
        }

        public string Role => this.RoleType.ToString();

        public void SetFromUser(User user)
        {
            if (user == null)
            {
                return;
            }

            userId = user.Id;
            if (Enum.TryParse<UserRole>(user.Role, ignoreCase: true, out var parsed))
            {
                roleType = parsed;
            }
            else
            {
                roleType = user.IsAdmin ? UserRole.Admin : UserRole.Client;
            }
        }
    }
}
