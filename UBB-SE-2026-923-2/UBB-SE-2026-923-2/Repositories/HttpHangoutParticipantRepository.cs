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
            var participantSummaries = this.httpClient
                .GetFromJsonAsync<List<HangoutParticipantSummary>>(BasePath)
                .GetAwaiter().GetResult();
            if (participantSummaries is null)
            {
                return Array.Empty<(int, int)>();
            }

            return participantSummaries
                .Select(participant => (participant.HangoutId, participant.StaffId))
                .ToList();
        }

        public void AddParticipant(int hangoutId, int staffId)
        {
            var requestPayload = new { HangoutId = hangoutId, StaffId = staffId };
            var httpResponse = this.httpClient.PostAsJsonAsync(BasePath, requestPayload).GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
        }
    }
}