using UBB_SE_2026_923_2.Models;

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
            var payload = new
            {
                ShiftId = request.Shift.Id,
                RequesterId = request.Requester.StaffID,
                ColleagueId = request.Colleague.StaffID,
                RequestedAt = request.RequestedAt == default ? DateTime.UtcNow : request.RequestedAt,
                Status = request.Status,
            };

            var httpResponse = this.httpClient.PostAsJsonAsync(BasePath, payload).GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
            return httpResponse.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public IReadOnlyList<ShiftSwapRequest> GetAllShiftSwapRequests()
        {
            var json = this.httpClient.GetStringAsync(BasePath).GetAwaiter().GetResult();
            var dtos = System.Text.Json.JsonSerializer.Deserialize<List<SwapDto>>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<SwapDto>();

            var staff = this.httpClient
                .GetFromJsonAsync<List<Staff>>("api/staff")  // ← check your actual route
                .GetAwaiter().GetResult() ?? new List<Staff>();

            var shifts = this.httpClient
                .GetFromJsonAsync<List<Shift>>("api/shifts") // ← check your actual route
                .GetAwaiter().GetResult() ?? new List<Shift>();

            return dtos.Select(d => new ShiftSwapRequest
            {
                SwapId = d.SwapId,
                RequestedAt = d.RequestedAt,
                Status = d.Status,
                Shift = shifts.FirstOrDefault(shiftNew => shiftNew.Id == d.ShiftId) ?? new Shift(),
                Requester = staff.FirstOrDefault(staffNew => staffNew.StaffID == d.RequesterId) as Staff ?? new Staff(),
                Colleague = staff.FirstOrDefault(staffNew => staffNew.StaffID == d.ColleagueId) as Staff ?? new Staff()
            }).ToList();
        }

        public ShiftSwapRequest? GetShiftSwapRequestById(int swapId)
        {
            var httpResponse = this.httpClient.GetAsync($"{BasePath}/{swapId}").GetAwaiter().GetResult();
            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                return null;

            httpResponse.EnsureSuccessStatusCode();

            var json = httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var dto = System.Text.Json.JsonSerializer.Deserialize<SwapDto>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto == null) return null;

            var shift = this.httpClient
                .GetFromJsonAsync<Shift>($"api/shifts/{dto.ShiftId}")
                .GetAwaiter().GetResult();

            var requester = this.httpClient
                .GetFromJsonAsync<Staff>($"api/staff/{dto.RequesterId}")
                .GetAwaiter().GetResult();

            var colleague = this.httpClient
                .GetFromJsonAsync<Staff>($"api/staff/{dto.ColleagueId}")
                .GetAwaiter().GetResult();

            return new ShiftSwapRequest
            {
                SwapId = dto.SwapId,
                RequestedAt = dto.RequestedAt,
                Status = dto.Status,
                Shift = shift ?? new Shift(),
                Requester = requester ?? new Staff(),
                Colleague = colleague ?? new Staff()
            };
        }

        public void UpdateShiftSwapRequestStatus(int swapId, string status)
        {
            var httpResponse = this.httpClient
                .PatchAsJsonAsync($"{BasePath}/{swapId}/status", new { Status = status })
                .GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
        }

        private class SwapDto
        {
            public int SwapId { get; set; }
            public DateTime RequestedAt { get; set; }
            public ShiftSwapRequestStatus Status { get; set; }
            public int ShiftId { get; set; }
            public int RequesterId { get; set; }
            public int ColleagueId { get; set; }
        }
    }
}
