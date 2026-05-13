namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Json;

    /// <summary>
    /// HTTP-backed implementation of <see cref="INotificationRepository"/>.
    /// </summary>
    public class HttpNotificationRepository : INotificationRepository
    {
        private const string BasePath = "api/notifications";

        private readonly HttpClient httpClient;

        public HttpNotificationRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public void AddNotification(int recipientStaffId, string title, string message)
        {
            var requestPayload = new
            {
                RecipientStaffId = recipientStaffId,
                Title = title,
                Message = message,
            };
            var httpResponse = this.httpClient.PostAsJsonAsync(BasePath, requestPayload).GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
        }
    }
}