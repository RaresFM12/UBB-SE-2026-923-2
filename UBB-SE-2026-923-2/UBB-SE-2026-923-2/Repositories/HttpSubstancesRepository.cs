using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    /// <summary>
    /// HTTP-backed implementation of <see cref="ISubstancesRepository"/>.
    /// </summary>
    public class HttpSubstancesRepository : ISubstancesRepository
    {
        private const string BasePath = "api/substances";

        private readonly HttpClient httpClient;

        public HttpSubstancesRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public void AddSubstance(string name, float lethalDose, string description)
        {
            var payload = new { Name = name, LethalDose = lethalDose, Description = description };
            var response = httpClient.PostAsJsonAsync(BasePath, payload).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public void RemoveSubstanceByName(string name)
        {
            var response = httpClient
                .DeleteAsync($"{BasePath}/{Uri.EscapeDataString(name)}")
                .GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public Substance GetSubstanceByName(string name)
        {
            var response = httpClient
                .GetAsync($"{BasePath}/{Uri.EscapeDataString(name)}")
                .GetAwaiter().GetResult();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null!;
            }

            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<Substance>().GetAwaiter().GetResult()!;
        }

        public List<Substance> GetAllSubstances()
        {
            var substances = httpClient
                .GetFromJsonAsync<List<Substance>>(BasePath)
                .GetAwaiter().GetResult();
            return substances ?? new List<Substance>();
        }

        public void UpdateSubstanceByName(Substance substance)
        {
            var response = httpClient
                .PutAsJsonAsync($"{BasePath}/{Uri.EscapeDataString(substance.Name)}", substance)
                .GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public bool SubstanceExists(string name)
        {
            return httpClient
                .GetFromJsonAsync<bool>($"{BasePath}/{Uri.EscapeDataString(name)}/exists")
                .GetAwaiter().GetResult();
        }

        public Dictionary<string, int> GetTop30Substances()
        {
            var top = httpClient
                .GetFromJsonAsync<Dictionary<string, int>>($"{BasePath}/top")
                .GetAwaiter().GetResult();
            return top ?? new Dictionary<string, int>();
        }
    }
}
