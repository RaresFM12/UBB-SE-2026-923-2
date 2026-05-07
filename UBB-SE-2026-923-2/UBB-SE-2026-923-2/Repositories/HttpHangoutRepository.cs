using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
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

        public int AddHangout(string title, string description, DateTime date, int maxParticipants)
        {
            var payload = new
            {
                Title = title,
                Description = description,
                Date = date,
                MaxParticipants = maxParticipants,
            };

            var response = httpClient.PostAsJsonAsync(BasePath, payload).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public List<Hangout> GetAllHangouts()
        {
            var hangouts = httpClient.GetFromJsonAsync<List<Hangout>>(BasePath).GetAwaiter().GetResult();
            return hangouts ?? new List<Hangout>();
        }

        public Hangout? GetHangoutById(int hangoutId)
        {
            var response = httpClient.GetAsync($"{BasePath}/{hangoutId}").GetAwaiter().GetResult();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<Hangout>().GetAwaiter().GetResult();
        }
    }
}
