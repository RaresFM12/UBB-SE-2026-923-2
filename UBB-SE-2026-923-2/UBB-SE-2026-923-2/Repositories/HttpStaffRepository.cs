namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Threading.Tasks;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// HTTP-backed implementation of all three staff-repository interfaces.
    /// Polymorphic Staff JSON ($type discriminator) is decoded by
    /// System.Text.Json via the JsonDerivedType attributes on Staff.
    /// </summary>
    public class HttpStaffRepository : IStaffRepository, IShiftManagementStaffRepository, IPharmacyStaffRepository
    {
        private const string BasePath = "api/staff";

        private readonly HttpClient httpClient;

        public HttpStaffRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public List<IStaff> LoadAllStaff()
        {
            var staff = this.httpClient.GetFromJsonAsync<List<Staff>>(BasePath).GetAwaiter().GetResult();
            return staff is null
                ? new List<IStaff>()
                : staff.Cast<IStaff>().ToList();
        }

        public IStaff? GetStaffById(int staffId)
        {
            var httpResponse = this.httpClient.GetAsync($"{BasePath}/{staffId}").GetAwaiter().GetResult();
            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            httpResponse.EnsureSuccessStatusCode();
            var staff = httpResponse.Content
                .ReadFromJsonAsync<Staff>()
                .GetAwaiter().GetResult();
            return staff;
        }

        public List<Pharmacyst> GetPharmacists()
        {
            var pharmacists = this.httpClient
                .GetFromJsonAsync<List<Pharmacyst>>($"{BasePath}/pharmacists")
                .GetAwaiter().GetResult();
            return pharmacists ?? new List<Pharmacyst>();
        }

        public async Task<IReadOnlyList<(int DoctorId, string FirstName, string LastName)>> GetAllDoctorsAsync()
        {
            var doctorSummaries = await this.httpClient.GetFromJsonAsync<List<DoctorSummary>>($"{BasePath}/doctors");
            if (doctorSummaries is null)
            {
                return Array.Empty<(int, string, string)>();
            }

            return doctorSummaries
                .Select(doctorSummary => (doctorSummary.DoctorId, doctorSummary.FirstName, doctorSummary.LastName))
                .ToList();
        }

        public async Task UpdateStatusAsync(int staffId, string status)
        {
            var httpResponse = await this.httpClient.PatchAsJsonAsync(
                $"{BasePath}/{staffId}/status",
                new { Status = status });
            httpResponse.EnsureSuccessStatusCode();
        }

        public void UpdateStaffAvailability(int staffId, bool isAvailable, DoctorStatus status = DoctorStatus.OFF_DUTY)
        {
            var requestPayload = new { IsAvailable = isAvailable, Status = status };
            var httpResponse = this.httpClient
                .PatchAsJsonAsync($"{BasePath}/{staffId}/availability", requestPayload)
                .GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
        }
    }
}