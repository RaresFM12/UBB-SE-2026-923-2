namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UBB_SE_2026_923_2.Models;

    public interface IUsersRepository
    {
        bool UserExists(string email);

        bool UserExists(int userId);

        User GetUserById(int userId);

        User GetUserByEmail(string email);

        void AddUser(string email, string phoneNumber, string passwordHash, string username,
            bool discountNotifications, bool isDisabled = false, bool isAdmin = false, int loyaltyPoints = 0, string role = "Client");

        void UpdateUser(User user);

        List<User> GetAllUsers();

        bool UserHasPeriodTracker(int userId);
    }
}