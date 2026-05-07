using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
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
            float price, int nrOfPills,
            string label = "", string description = "", string imagePath = ImagePathDefault,
            float discount = 0f)
        {
            var payload = new
            {
                Name = name,
                Producer = producer,
                Category = category,
                Price = price,
                NumberOfPills = nrOfPills,
                Label = label,
                Description = description,
                ImagePath = imagePath,
                Discount = discount,
            };
            var response = httpClient.PostAsJsonAsync(BasePath, payload).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public void AddItemWithQuantity(string name, string producer, string category,
            float price, int nrOfPills,
            int quantity, Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches,
            string label = "", string description = "", string imagePath = ImagePathDefault,
            float discount = 0f)
        {
            var payload = new
            {
                Name = name,
                Producer = producer,
                Category = category,
                Price = price,
                NumberOfPills = nrOfPills,
                Quantity = quantity,
                ActiveSubstances = activeSubstances,
                Batches = batches,
                Label = label,
                Description = description,
                ImagePath = imagePath,
                Discount = discount,
            };
            var response = httpClient.PostAsJsonAsync($"{BasePath}/with-quantity", payload).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public void RemoveItemById(int idToBeRemoved)
        {
            var response = httpClient.DeleteAsync($"{BasePath}/{idToBeRemoved}").GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public Item GetItemById(int id)
        {
            var response = httpClient.GetAsync($"{BasePath}/{id}").GetAwaiter().GetResult();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null!;
            }

            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<Item>().GetAwaiter().GetResult()!;
        }

        public List<Item> GetAllItems()
        {
            var items = httpClient.GetFromJsonAsync<List<Item>>(BasePath).GetAwaiter().GetResult();
            return items ?? new List<Item>();
        }

        public List<Item> GetItemsByName(string name)
        {
            var url = $"{BasePath}?name={Uri.EscapeDataString(name)}";
            var items = httpClient.GetFromJsonAsync<List<Item>>(url).GetAwaiter().GetResult();
            return items ?? new List<Item>();
        }

        public void UpdateItemById(Item newItem)
        {
            var response = httpClient
                .PutAsJsonAsync($"{BasePath}/{newItem.Id}", newItem)
                .GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public bool ItemExists(int id)
        {
            return httpClient
                .GetFromJsonAsync<bool>($"{BasePath}/{id}/exists")
                .GetAwaiter().GetResult();
        }

        public List<Tuple<int, string, int>> GetTop30Items()
        {
            var summaries = httpClient
                .GetFromJsonAsync<List<ItemPopularitySummary>>($"{BasePath}/top")
                .GetAwaiter().GetResult();
            if (summaries is null)
            {
                return new List<Tuple<int, string, int>>();
            }

            return summaries
                .Select(s => new Tuple<int, string, int>(s.Id, s.Name, s.OrdersCount))
                .ToList();
        }
    }
}
