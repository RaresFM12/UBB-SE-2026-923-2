namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// HTTP-backed implementation of <see cref="IHangoutRepository"/>.
    /// </summary>
    public class HttpHangoutRepository : IHangoutRepository
    {
        private const string BasePath = "api/hangouts";

        private readonly HttpClient httpClient;

        public HttpHangoutRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public int AddHangout(string title, string description, DateTime date, int maximumParticipants)
        {
            var requestPayload = new
            {
                Title = title,
                Description = description,
                Date = date,
                MaxParticipants = maximumParticipants,
            };

            var httpResponse = this.httpClient.PostAsJsonAsync(BasePath, requestPayload).GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
            return httpResponse.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public List<Hangout> GetAllHangouts()
        {
            var hangoutsList = this.httpClient.GetFromJsonAsync<List<Hangout>>(BasePath).GetAwaiter().GetResult();
            return hangoutsList ?? new List<Hangout>();
        }

        public Hangout? GetHangoutById(int hangoutId)
        {
            var httpResponse = this.httpClient.GetAsync($"{BasePath}/{hangoutId}").GetAwaiter().GetResult();
            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            httpResponse.EnsureSuccessStatusCode();
            return httpResponse.Content.ReadFromJsonAsync<Hangout>().GetAwaiter().GetResult();
        }
    }
}