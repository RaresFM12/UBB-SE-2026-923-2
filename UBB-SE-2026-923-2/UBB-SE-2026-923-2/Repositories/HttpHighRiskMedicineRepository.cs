namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// HTTP-backed implementation of <see cref="IHighRiskMedicineRepository"/>.
    /// </summary>
    public class HttpHighRiskMedicineRepository : IHighRiskMedicineRepository
    {
        private const string BasePath = "api/highriskmedicines";

        private readonly HttpClient httpClient;

        public HttpHighRiskMedicineRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public IReadOnlyList<(string MedicineName, string WarningMessage)> GetAllHighRiskMedicines()
        {
            var summaries = this.httpClient
                .GetFromJsonAsync<List<HighRiskMedicineSummary>>(BasePath)
                .GetAwaiter().GetResult();
            if (summaries is null)
            {
                return Array.Empty<(string, string)>();
            }

            return summaries
                .Select(medicine => (medicine.MedicineName, medicine.WarningMessage))
                .ToList();
        }
    }
}