namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Shared;

    /// <summary>
    /// HTTP-backed implementation of <see cref="IUsersRepository"/>.
    /// </summary>
    public class HttpUsersRepository : IUsersRepository
    {
        private const string BasePath = "api/users";

        private readonly HttpClient httpClient;
        
        private static readonly System.Text.Json.JsonSerializerOptions jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new TupleStringBoolConverter() }
        };

        public HttpUsersRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public bool UserExists(string email)
        {
            var requestUrl = $"{BasePath}/exists?email={Uri.EscapeDataString(email)}";
            return this.httpClient.GetFromJsonAsync<bool>(requestUrl, jsonOptions).GetAwaiter().GetResult();
        }

        public bool UserExists(int userId)
        {
            return this.httpClient
                .GetFromJsonAsync<bool>($"{BasePath}/{userId}/exists", jsonOptions)
                .GetAwaiter().GetResult();
        }

        public User GetUserById(int userId)
        {
            var httpResponse = this.httpClient.GetAsync($"{BasePath}/{userId}").GetAwaiter().GetResult();
            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null!;
            }

            httpResponse.EnsureSuccessStatusCode();
            var user = httpResponse.Content.ReadFromJsonAsync<User>(jsonOptions).GetAwaiter().GetResult();
            return user!;
        }

        public User GetUserByEmail(string email)
        {
            var requestUrl = $"{BasePath}/by-email?email={Uri.EscapeDataString(email)}";
            var httpResponse = this.httpClient.GetAsync(requestUrl).GetAwaiter().GetResult();
            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null!;
            }

            httpResponse.EnsureSuccessStatusCode();
            var user = httpResponse.Content.ReadFromJsonAsync<User>(jsonOptions).GetAwaiter().GetResult();
            return user!;
        }

        public void AddUser(string email, string phoneNumber, string passwordHash, string username,
            bool discountNotifications, bool isDisabled = false, bool isAdmin = false, int loyaltyPoints = 0, string role = "Client")
        {
            var requestPayload = new
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

            var httpResponse = this.httpClient.PostAsJsonAsync(BasePath, requestPayload, jsonOptions).GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
        }

        public void UpdateUser(User user)
        {
            var httpResponse = this.httpClient
                .PutAsJsonAsync($"{BasePath}/{user.Id}", user, jsonOptions)
                .GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
        }

        public List<User> GetAllUsers()
        {
            var users = this.httpClient.GetFromJsonAsync<List<User>>(BasePath, jsonOptions).GetAwaiter().GetResult();
            return users ?? new List<User>();
        }

        public bool UserHasPeriodTracker(int userId)
        {
            return this.httpClient
                .GetFromJsonAsync<bool>($"{BasePath}/{userId}/period-tracker", jsonOptions)
                .GetAwaiter().GetResult();
        }
    }
}