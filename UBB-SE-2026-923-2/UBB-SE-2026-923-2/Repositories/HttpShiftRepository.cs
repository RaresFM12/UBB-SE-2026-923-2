using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    /// <summary>
    /// HTTP-backed implementation of all three shift-repository interfaces.
    /// Network calls are async internally; the synchronous interface methods
    /// block via GetAwaiter().GetResult() to preserve the existing contract
    /// without rewriting every consumer.
    /// </summary>
    public class HttpShiftRepository : IShiftRepository, IShiftManagementShiftRepository, IPharmacyShiftRepository
    {
        private const string BasePath = "api/shifts";

        private readonly HttpClient httpClient;

        public HttpShiftRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public IReadOnlyList<Shift> GetAllShifts()
        {
            var shifts = httpClient.GetFromJsonAsync<List<Shift>>(BasePath).GetAwaiter().GetResult();
            return shifts ?? new List<Shift>();
        }

        public void AddShift(Shift newShift)
        {
            int staffId = newShift.StaffId != 0
                ? newShift.StaffId
                : (newShift.AppointedStaff?.StaffID ?? 0);

            var payload = new
            {
                StaffId = staffId,
                Location = newShift.Location,
                StartTime = newShift.StartTime,
                EndTime = newShift.EndTime,
                Status = newShift.Status,
            };

            var response = httpClient.PostAsJsonAsync(BasePath, payload).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public void UpdateShiftStatus(int shiftId, ShiftStatus status)
        {
            var response = httpClient
                .PatchAsJsonAsync($"{BasePath}/{shiftId}/status", new { Status = status })
                .GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public void UpdateShiftStaffId(int shiftId, int newStaffId)
        {
            var response = httpClient
                .PatchAsJsonAsync($"{BasePath}/{shiftId}/staff", new { StaffId = newStaffId })
                .GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public void DeleteShift(int shiftId)
        {
            var response = httpClient.DeleteAsync($"{BasePath}/{shiftId}").GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }
    }
}
