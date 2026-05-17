namespace UBB_SE_2026_923_2.IntegrationTests.Fakes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;

    public sealed class FakeUsersRepository : IUsersRepository
    {
        private readonly List<User> users = new();
        private int nextId = 1;

        public void Seed(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user.Id <= 0)
            {
                user.Id = this.nextId++;
            }
            else
            {
                this.nextId = Math.Max(this.nextId, user.Id + 1);
            }

            this.users.Add(user);
        }

        public bool UserExists(string email)
        {
            return this.users.Any(user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));
        }

        public bool UserExists(int userId)
        {
            return this.users.Any(user => user.Id == userId);
        }

        public User GetUserById(int userId)
        {
            return this.users.FirstOrDefault(user => user.Id == userId)!;
        }

        public User GetUserByEmail(string email)
        {
            return this.users.FirstOrDefault(user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))!;
        }

        public void AddUser(string email, string phoneNumber, string passwordHash, string username,
            bool discountNotifications, bool isDisabled = false, bool isAdmin = false, int loyaltyPoints = 0, string role = "Client")
        {
            var user = new User
            {
                Id = this.nextId++,
                Email = email,
                PhoneNumber = phoneNumber ?? string.Empty,
                PasswordHash = passwordHash,
                Username = username ?? string.Empty,
                DiscountNotifications = discountNotifications,
                IsDisabled = isDisabled,
                IsAdmin = isAdmin,
                LoyaltyPoints = loyaltyPoints,
                Role = role ?? "Client",
            };

            this.users.Add(user);
        }

        public void UpdateUser(User user)
        {
            int index = this.users.FindIndex(existing => existing.Id == user.Id);
            if (index >= 0)
            {
                this.users[index] = user;
                return;
            }

            this.users.Add(user);
        }

        public List<User> GetAllUsers()
        {
            return this.users.ToList();
        }

        public bool UserHasPeriodTracker(int userId)
        {
            return false;
        }
    }
}
