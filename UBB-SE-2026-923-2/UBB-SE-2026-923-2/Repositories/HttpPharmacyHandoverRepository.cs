using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    /// <summary>
    /// HTTP-backed implementation of <see cref="IPharmacyHandoverRepository"/>.
    /// </summary>
    public class HttpPharmacyHandoverRepository : IPharmacyHandoverRepository
    {
        private const string BasePath = "api/pharmacyhandovers";

        private readonly HttpClient httpClient;

        public HttpPharmacyHandoverRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public IReadOnlyList<PharmacyHandover> GetAllPharmacyHandovers()
        {
            var handovers = httpClient
                .GetFromJsonAsync<List<PharmacyHandover>>(BasePath)
                .GetAwaiter().GetResult();
            return handovers ?? new List<PharmacyHandover>();
        }
    }
}
