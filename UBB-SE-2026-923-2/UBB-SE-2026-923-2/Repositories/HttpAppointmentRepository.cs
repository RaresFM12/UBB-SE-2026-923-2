namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// HTTP-backed implementation of <see cref="IAppointmentRepository"/> that
    /// delegates persistence to the Web API.
    /// </summary>
    public class HttpAppointmentRepository : IAppointmentRepository
    {
        private const string BasePath = "api/appointments";

        private readonly HttpClient httpClient;

        public HttpAppointmentRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<IReadOnlyList<Appointment>> GetAllAppointmentsAsync()
        {
            var appointments = await this.httpClient.GetFromJsonAsync<List<Appointment>>(BasePath);
            return appointments ?? new List<Appointment>();
        }

        public async Task AddAppointmentAsync(int patientId, int doctorId, DateTime startTime, DateTime endTime, string status)
        {
            var payload = new
            {
                PatientId = patientId,
                DoctorId = doctorId,
                StartTime = startTime,
                EndTime = endTime,
                Status = status,
            };

            var response = await this.httpClient.PostAsJsonAsync(BasePath, payload);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateAppointmentStatusAsync(int patientId, string status)
        {
            var payload = new { Status = status };
            var response = await this.httpClient.PatchAsJsonAsync($"{BasePath}/{patientId}/status", payload);
            response.EnsureSuccessStatusCode();
        }
    }
}
