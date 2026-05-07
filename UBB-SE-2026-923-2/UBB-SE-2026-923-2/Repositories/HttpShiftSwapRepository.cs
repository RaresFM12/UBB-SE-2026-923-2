using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
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
            var response = httpClient.PostAsJsonAsync(BasePath, request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public IReadOnlyList<ShiftSwapRequest> GetAllShiftSwapRequests()
        {
            var swaps = httpClient
                .GetFromJsonAsync<List<ShiftSwapRequest>>(BasePath)
                .GetAwaiter().GetResult();
            return swaps ?? new List<ShiftSwapRequest>();
        }

        public ShiftSwapRequest? GetShiftSwapRequestById(int swapId)
        {
            var response = httpClient.GetAsync($"{BasePath}/{swapId}").GetAwaiter().GetResult();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<ShiftSwapRequest>().GetAwaiter().GetResult();
        }

        public void UpdateShiftSwapRequestStatus(int swapId, string status)
        {
            var response = httpClient
                .PatchAsJsonAsync($"{BasePath}/{swapId}/status", new { Status = status })
                .GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }
    }
}
