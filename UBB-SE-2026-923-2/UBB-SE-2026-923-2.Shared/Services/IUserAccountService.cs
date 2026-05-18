namespace UBB_SE_2026_923_2.Services
{
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;

    public interface IUserAccountService
    {
        User? CurrentUser { get; }

        User? LoadCurrentUser(int userId);

        void Login(string email, string password);

        void Register(
            string email,
            string password,
            string confirmPassword,
            string username,
            string phoneNumber,
            string role = "Client");

        void UpdateProfile(string newUsername, string newPhoneNumber);

        void ChangePassword(string oldPassword, string newPassword, string confirmNewPassword);

        List<User> SearchUsers(string query);

        void PromoteToAdmin(User client);

        void DisableAccount(User client);

        void Logout();
    }
}