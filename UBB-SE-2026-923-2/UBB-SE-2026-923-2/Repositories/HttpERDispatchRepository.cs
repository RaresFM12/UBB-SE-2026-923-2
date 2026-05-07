using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
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
            var payload = new { Specialization = specialization, Location = location, Status = status };
            var response = httpClient.PostAsJsonAsync(BasePath, payload).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public IReadOnlyList<ERRequest> GetAllRequests()
        {
            var requests = httpClient.GetFromJsonAsync<List<ERRequest>>(BasePath).GetAwaiter().GetResult();
            return requests ?? new List<ERRequest>();
        }

        public ERRequest? GetRequestById(int requestId)
        {
            var response = httpClient.GetAsync($"{BasePath}/{requestId}").GetAwaiter().GetResult();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<ERRequest>().GetAwaiter().GetResult();
        }

        public void UpdateRequestStatus(int requestId, string status, int? assignedDoctorId, string? assignedDoctorName)
        {
            var payload = new
            {
                Status = status,
                AssignedDoctorId = assignedDoctorId,
                AssignedDoctorName = assignedDoctorName,
            };
            var response = httpClient
                .PatchAsJsonAsync($"{BasePath}/{requestId}/status", payload)
                .GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }
    }
}
