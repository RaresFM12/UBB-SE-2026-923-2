namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using UBB_SE_2026_923_2.Models;

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
            var evaluationsList = this.httpClient
                .GetFromJsonAsync<List<MedicalEvaluation>>(BasePath)
                .GetAwaiter().GetResult();
            return evaluationsList ?? new List<MedicalEvaluation>();
        }

        public void AddEvaluation(int doctorId, int patientId, string diagnosis, string notes, string medications, bool assumedRisk)
        {
            var requestPayload = new
            {
                DoctorId = doctorId,
                PatientId = patientId,
                Diagnosis = diagnosis,
                Notes = notes,
                Medications = medications,
                AssumedRisk = assumedRisk,
            };
            var httpResponse = this.httpClient.PostAsJsonAsync(BasePath, requestPayload).GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
        }

        public void UpdateEvaluation(int evaluationId, string diagnosis, string notes, string medications)
        {
            var requestPayload = new { Diagnosis = diagnosis, Notes = notes, Medications = medications };
            var httpResponse = this.httpClient
                .PutAsJsonAsync($"{BasePath}/{evaluationId}", requestPayload)
                .GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
        }

        public void DeleteEvaluation(int evaluationId)
        {
            var httpResponse = this.httpClient
                .DeleteAsync($"{BasePath}/{evaluationId}")
                .GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
        }
    }
}