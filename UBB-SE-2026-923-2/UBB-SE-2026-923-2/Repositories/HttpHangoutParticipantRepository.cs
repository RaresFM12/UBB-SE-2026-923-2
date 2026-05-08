namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// HTTP-backed implementation of <see cref="IHangoutParticipantRepository"/>.
    /// </summary>
    public class HttpHangoutParticipantRepository : IHangoutParticipantRepository
    {
        private const string BasePath = "api/hangoutparticipants";

        private readonly HttpClient httpClient;

        public HttpHangoutParticipantRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public IReadOnlyList<(int HangoutId, int StaffId)> GetAllParticipants()
        {
            var summaries = this.httpClient
                .GetFromJsonAsync<List<HangoutParticipantSummary>>(BasePath)
                .GetAwaiter().GetResult();
            if (summaries is null)
            {
                return Array.Empty<(int, int)>();
            }

            return summaries
                .Select(participant => (participant.HangoutId, participant.StaffId))
                .ToList();
        }

        public void AddParticipant(int hangoutId, int staffId)
        {
            var payload = new { HangoutId = hangoutId, StaffId = staffId };
            var response = this.httpClient.PostAsJsonAsync(BasePath, payload).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }
    }
}
