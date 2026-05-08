namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// HTTP-backed implementation of <see cref="IOrdersRepository"/>.
    /// </summary>
    public class HttpOrdersRepository : IOrdersRepository
    {
        private const string BasePath = "api/orders";

        private readonly HttpClient httpClient;

        public HttpOrdersRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public int AddOrder(int clientId, DateOnly pickUpDate, bool isCompleted = false, bool isExpired = false)
        {
            var requestPayload = new
            {
                ClientId = clientId,
                PickUpDate = pickUpDate,
                IsCompleted = isCompleted,
                IsExpired = isExpired,
            };
            var httpResponse = this.httpClient.PostAsJsonAsync(BasePath, requestPayload).GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
            return httpResponse.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public void RemoveOrder(int orderIdToBeRemoved)
        {
            var httpResponse = this.httpClient.DeleteAsync($"{BasePath}/{orderIdToBeRemoved}").GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
        }

        public Order GetOrder(int orderId)
        {
            var httpResponse = this.httpClient.GetAsync($"{BasePath}/{orderId}").GetAwaiter().GetResult();
            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null!;
            }

            httpResponse.EnsureSuccessStatusCode();
            return httpResponse.Content.ReadFromJsonAsync<Order>().GetAwaiter().GetResult()!;
        }

        public List<Order> GetAllOrders()
        {
            var orders = this.httpClient.GetFromJsonAsync<List<Order>>(BasePath).GetAwaiter().GetResult();
            return orders ?? new List<Order>();
        }

        public List<Order> GetOrdersOfClient(int clientId)
        {
            var orders = this.httpClient
                .GetFromJsonAsync<List<Order>>($"{BasePath}?clientId={clientId}")
                .GetAwaiter().GetResult();
            return orders ?? new List<Order>();
        }

        public void UpdateOrder(Order newOrder)
        {
            var httpResponse = this.httpClient
                .PutAsJsonAsync($"{BasePath}/{newOrder.Id}", newOrder)
                .GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
        }

        public bool OrderExists(int orderId)
        {
            return this.httpClient
                .GetFromJsonAsync<bool>($"{BasePath}/{orderId}/exists")
                .GetAwaiter().GetResult();
        }
    }
}