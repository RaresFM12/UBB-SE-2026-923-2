using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    /// <summary>
    /// HTTP-backed implementation of <see cref="IUsersRepository"/>.
    /// </summary>
    public class HttpUsersRepository : IUsersRepository
    {
        private const string BasePath = "api/users";

        private readonly HttpClient httpClient;

        public HttpUsersRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public bool UserExists(string email)
        {
            var url = $"{BasePath}/exists?email={Uri.EscapeDataString(email)}";
            return httpClient.GetFromJsonAsync<bool>(url).GetAwaiter().GetResult();
        }

        public bool UserExists(int id)
        {
            return httpClient
                .GetFromJsonAsync<bool>($"{BasePath}/{id}/exists")
                .GetAwaiter().GetResult();
        }

        public User GetUserById(int id)
        {
            var response = httpClient.GetAsync($"{BasePath}/{id}").GetAwaiter().GetResult();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null!;
            }

            response.EnsureSuccessStatusCode();
            var user = response.Content.ReadFromJsonAsync<User>().GetAwaiter().GetResult();
            return user!;
        }

        public User GetUserByEmail(string email)
        {
            var url = $"{BasePath}/by-email?email={Uri.EscapeDataString(email)}";
            var response = httpClient.GetAsync(url).GetAwaiter().GetResult();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null!;
            }

            response.EnsureSuccessStatusCode();
            var user = response.Content.ReadFromJsonAsync<User>().GetAwaiter().GetResult();
            return user!;
        }

        public void AddUser(string email, string phoneNumber, string passwordHash, string username,
            bool discountNotifications, bool isDisabled = false, bool isAdmin = false, int loyaltyPoints = 0, string role = "Client")
        {
            var payload = new
            {
                Email = email,
                PhoneNumber = phoneNumber,
                PasswordHash = passwordHash,
                Username = username,
                DiscountNotifications = discountNotifications,
                IsDisabled = isDisabled,
                IsAdmin = isAdmin,
                LoyaltyPoints = loyaltyPoints,
                Role = role,
            };

            var response = httpClient.PostAsJsonAsync(BasePath, payload).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public void UpdateUser(User user)
        {
            var response = httpClient
                .PutAsJsonAsync($"{BasePath}/{user.Id}", user)
                .GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public List<User> GetAllUsers()
        {
            var users = httpClient.GetFromJsonAsync<List<User>>(BasePath).GetAwaiter().GetResult();
            return users ?? new List<User>();
        }

        public bool UserHasPeriodTracker(int id)
        {
            return httpClient
                .GetFromJsonAsync<bool>($"{BasePath}/{id}/period-tracker")
                .GetAwaiter().GetResult();
        }
    }
}
