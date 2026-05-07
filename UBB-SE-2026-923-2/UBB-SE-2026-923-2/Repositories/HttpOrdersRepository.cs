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
            var payload = new
            {
                ClientId = clientId,
                PickUpDate = pickUpDate,
                IsCompleted = isCompleted,
                IsExpired = isExpired,
            };
            var response = this.httpClient.PostAsJsonAsync(BasePath, payload).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public void RemoveOrder(int orderIdToBeRemoved)
        {
            var response = this.httpClient.DeleteAsync($"{BasePath}/{orderIdToBeRemoved}").GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public Order GetOrder(int orderId)
        {
            var response = this.httpClient.GetAsync($"{BasePath}/{orderId}").GetAwaiter().GetResult();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null!;
            }

            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<Order>().GetAwaiter().GetResult()!;
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
            var response = this.httpClient
                .PutAsJsonAsync($"{BasePath}/{newOrder.Id}", newOrder)
                .GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public bool OrderExists(int orderId)
        {
            return this.httpClient
                .GetFromJsonAsync<bool>($"{BasePath}/{orderId}/exists")
                .GetAwaiter().GetResult();
        }
    }
}
