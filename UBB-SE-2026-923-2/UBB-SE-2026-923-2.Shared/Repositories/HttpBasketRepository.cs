namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using UBB_SE_2026_923_2.Models;

    public class HttpBasketRepository : IBasketRepository
    {
        private const string BasePath = "api/baskets";

        private readonly HttpClient httpClient;

        public HttpBasketRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public Dictionary<int, BasketEntry> GetBasket(int userId)
        {
            var basket = this.httpClient
                .GetFromJsonAsync<Dictionary<int, BasketEntry>>($"{BasePath}/{userId}")
                .GetAwaiter()
                .GetResult();

            return basket ?? new Dictionary<int, BasketEntry>();
        }

        public void SaveBasket(int userId, Dictionary<int, BasketEntry> basket)
        {
            var httpResponse = this.httpClient
                .PutAsJsonAsync($"{BasePath}/{userId}", basket)
                .GetAwaiter()
                .GetResult();

            httpResponse.EnsureSuccessStatusCode();
        }

        public void ClearBasket(int userId)
        {
            var httpResponse = this.httpClient
                .DeleteAsync($"{BasePath}/{userId}")
                .GetAwaiter()
                .GetResult();

            httpResponse.EnsureSuccessStatusCode();
        }
    }
}
