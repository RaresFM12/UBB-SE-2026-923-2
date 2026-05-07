using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    /// <summary>
    /// HTTP-backed implementation of <see cref="IEvaluationsRepository"/>.
    /// </summary>
    public class HttpEvaluationsRepository : IEvaluationsRepository
    {
        private const string BasePath = "api/evaluations";

        private readonly HttpClient httpClient;

        public HttpEvaluationsRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public IReadOnlyList<MedicalEvaluation> GetAllEvaluations()
        {
            var evaluations = httpClient
                .GetFromJsonAsync<List<MedicalEvaluation>>(BasePath)
                .GetAwaiter().GetResult();
            return evaluations ?? new List<MedicalEvaluation>();
        }

        public void AddEvaluation(int doctorId, int patientId, string diagnosis, string notes, string medications, bool assumedRisk)
        {
            var payload = new
            {
                DoctorId = doctorId,
                PatientId = patientId,
                Diagnosis = diagnosis,
                Notes = notes,
                Medications = medications,
                AssumedRisk = assumedRisk,
            };
            var response = httpClient.PostAsJsonAsync(BasePath, payload).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public void UpdateEvaluation(int evaluationId, string diagnosis, string notes, string medications)
        {
            var payload = new { Diagnosis = diagnosis, Notes = notes, Medications = medications };
            var response = httpClient
                .PutAsJsonAsync($"{BasePath}/{evaluationId}", payload)
                .GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public void DeleteEvaluation(int evaluationId)
        {
            var response = httpClient
                .DeleteAsync($"{BasePath}/{evaluationId}")
                .GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }
    }
}
