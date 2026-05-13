namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// HTTP-backed implementation of <see cref="IERDispatchRepository"/>.
    /// </summary>
    public class HttpERDispatchRepository : IERDispatchRepository
    {
        private const string BasePath = "api/errequests";

        private readonly HttpClient httpClient;

        public HttpERDispatchRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public int AddRequest(string specialization, string location, string status)
        {
            var requestPayload = new { Specialization = specialization, Location = location, Status = status };
            var httpResponse = this.httpClient.PostAsJsonAsync(BasePath, requestPayload).GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
            return httpResponse.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public IReadOnlyList<ERRequest> GetAllRequests()
        {
            var erRequestsList = this.httpClient.GetFromJsonAsync<List<ERRequest>>(BasePath).GetAwaiter().GetResult();
            return erRequestsList ?? new List<ERRequest>();
        }

        public ERRequest? GetRequestById(int requestId)
        {
            var httpResponse = this.httpClient.GetAsync($"{BasePath}/{requestId}").GetAwaiter().GetResult();
            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            httpResponse.EnsureSuccessStatusCode();
            return httpResponse.Content.ReadFromJsonAsync<ERRequest>().GetAwaiter().GetResult();
        }

        public void UpdateRequestStatus(int requestId, string status, int? assignedDoctorId, string? assignedDoctorName)
        {
            var requestPayload = new
            {
                Status = status,
                AssignedDoctorId = assignedDoctorId,
                AssignedDoctorName = assignedDoctorName,
            };
            var httpResponse = this.httpClient
                .PatchAsJsonAsync($"{BasePath}/{requestId}/status", requestPayload)
                .GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
        }
    }
}