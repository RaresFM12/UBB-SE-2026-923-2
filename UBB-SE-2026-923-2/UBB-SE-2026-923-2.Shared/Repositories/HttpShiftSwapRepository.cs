namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// HTTP-backed implementation of <see cref="IShiftSwapRepository"/>.
    /// </summary>
    public class HttpShiftSwapRepository : IShiftSwapRepository
    {
        private const string BasePath = "api/shiftswaps";

        private readonly HttpClient httpClient;

        public HttpShiftSwapRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public int AddShiftSwapRequest(ShiftSwapRequest request)
        {
            var httpResponse = this.httpClient.PostAsJsonAsync(BasePath, request).GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
            return httpResponse.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public IReadOnlyList<ShiftSwapRequest> GetAllShiftSwapRequests()
        {
            var swapRequests = this.httpClient
                .GetFromJsonAsync<List<ShiftSwapRequest>>(BasePath)
                .GetAwaiter().GetResult();
            return swapRequests ?? new List<ShiftSwapRequest>();
        }

        public ShiftSwapRequest? GetShiftSwapRequestById(int swapId)
        {
            var httpResponse = this.httpClient.GetAsync($"{BasePath}/{swapId}").GetAwaiter().GetResult();
            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            httpResponse.EnsureSuccessStatusCode();
            return httpResponse.Content.ReadFromJsonAsync<ShiftSwapRequest>().GetAwaiter().GetResult();
        }

        public void UpdateShiftSwapRequestStatus(int swapId, string status)
        {
            var httpResponse = this.httpClient
                .PatchAsJsonAsync($"{BasePath}/{swapId}/status", new { Status = status })
                .GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
        }
    }
}