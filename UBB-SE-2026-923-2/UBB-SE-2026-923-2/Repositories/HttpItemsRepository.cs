namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// HTTP-backed implementation of <see cref="IItemsRepository"/>.
    /// </summary>
    public class HttpItemsRepository : IItemsRepository
    {
        private const string BasePath = "api/items";
        private const string ImagePathDefault = "..\\..\\Assets\\placeholder.png";

        private readonly HttpClient httpClient;

        public HttpItemsRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public void AddItem(string name, string producer, string category,
            float price, int numberOfPills,
            string label = "", string description = "", string imagePath = ImagePathDefault,
            float discount = 0f)
        {
            var requestPayload = new
            {
                Name = name,
                Producer = producer,
                Category = category,
                Price = price,
                NumberOfPills = numberOfPills,
                Label = label,
                Description = description,
                ImagePath = imagePath,
                Discount = discount,
            };
            var httpResponse = this.httpClient.PostAsJsonAsync(BasePath, requestPayload).GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
        }

        public void AddItemWithQuantity(string name, string producer, string category,
            float price, int numberOfPills,
            int quantity, Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches,
            string label = "", string description = "", string imagePath = ImagePathDefault,
            float discount = 0f)
        {
            var requestPayload = new
            {
                Name = name,
                Producer = producer,
                Category = category,
                Price = price,
                NumberOfPills = numberOfPills,
                Quantity = quantity,
                ActiveSubstances = activeSubstances,
                Batches = batches,
                Label = label,
                Description = description,
                ImagePath = imagePath,
                Discount = discount,
            };
            var httpResponse = this.httpClient.PostAsJsonAsync($"{BasePath}/with-quantity", requestPayload).GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
        }

        public void RemoveItemById(int itemIdToRemove)
        {
            var httpResponse = this.httpClient.DeleteAsync($"{BasePath}/{itemIdToRemove}").GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
        }

        public Item GetItemById(int itemId)
        {
            var httpResponse = this.httpClient.GetAsync($"{BasePath}/{itemId}").GetAwaiter().GetResult();
            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null!;
            }

            httpResponse.EnsureSuccessStatusCode();
            return httpResponse.Content.ReadFromJsonAsync<Item>().GetAwaiter().GetResult()!;
        }

        public List<Item> GetAllItems()
        {
            var items = this.httpClient.GetFromJsonAsync<List<Item>>(BasePath).GetAwaiter().GetResult();
            return items ?? new List<Item>();
        }

        public List<Item> GetItemsByName(string name)
        {
            var url = $"{BasePath}?name={Uri.EscapeDataString(name)}";
            var items = this.httpClient.GetFromJsonAsync<List<Item>>(url).GetAwaiter().GetResult();
            return items ?? new List<Item>();
        }

        public void UpdateItemById(Item newItem)
        {
            var httpResponse = this.httpClient
                .PutAsJsonAsync($"{BasePath}/{newItem.Id}", newItem)
                .GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
        }

        public bool ItemExists(int itemId)
        {
            return this.httpClient
                .GetFromJsonAsync<bool>($"{BasePath}/{itemId}/exists")
                .GetAwaiter().GetResult();
        }

        public List<Tuple<int, string, int>> GetTop30Items()
        {
            var summaries = this.httpClient
                .GetFromJsonAsync<List<ItemPopularitySummary>>($"{BasePath}/top")
                .GetAwaiter().GetResult();
            if (summaries is null)
            {
                return new List<Tuple<int, string, int>>();
            }

            return summaries
                .Select(summary => new Tuple<int, string, int>(summary.Id, summary.Name, summary.OrdersCount))
                .ToList();
        }
    }
}