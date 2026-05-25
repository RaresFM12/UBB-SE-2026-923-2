namespace UBB_SE_2026_923_2.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    public class Order : IEquatable<Order>
    {
        public const int OrderExpirationDays = 7;

        // Setter opened up so System.Text.Json can rehydrate Id over HTTP.
        public int Id { get; set; }

        public string IdString
        {
            get { return "Order#" + this.Id; }
        }

        public DateOnly PickUpDate { get; set; }

        public string PickUpDateString
        {
            get { return this.PickUpDate.ToString("yyyy.MM.dd"); }
        }

        public string ExpirationDateString
        {
            get { return this.PickUpDate.AddDays(OrderExpirationDays).ToString("yyyy.MM.dd"); }
        }

        public bool IsCompleted { get; set; }

        public bool IsExpired { get; set; }

        public int ClientId { get; set; }

        // Legacy in-memory view — not persisted. Callers use OrderItemEntries below.
        [NotMapped]
        public Dictionary<int, Tuple<int, float>> ItemQuantitiesWithFinalPrice { get; set; }

        // EF Core navigation properties — persisted via shadow FK columns.
        // JsonIgnore: the full nested entities would balloon payload size.
        [JsonIgnore]
        public User Client { get; set; } = null!;

        [JsonIgnore]
        public ICollection<OrderItem> OrderItemEntries { get; set; } = new List<OrderItem>();

        public Order()
        {
            this.ItemQuantitiesWithFinalPrice = new Dictionary<int, Tuple<int, float>>();
        }

        public Order(int identifier, User client, DateOnly pickUpDate,
                     bool isCompleted = false, bool isExpired = false)
            : this()
        {
            this.Id = identifier;
            this.Client = client;
            this.ClientId = client.Id;
            this.PickUpDate = pickUpDate;
            this.IsCompleted = isCompleted;
            this.IsExpired = isExpired;
        }

        public bool Equals(Order other)
        {
            if (other is null)
            {
                return false;
            }

            return this.Id == other.Id;
        }

        public void AddItemToOrder(int newItemId, int itemQuantity, float finalPrice)
        {
            if (this.ItemQuantitiesWithFinalPrice.ContainsKey(newItemId))
            {
                throw new ArgumentException("Item #" + newItemId + " already exists in order");
            }

            this.ItemQuantitiesWithFinalPrice[newItemId] = new Tuple<int, float>(itemQuantity, finalPrice);
        }

        public void ChangeItemInfoInOrder(int itemId, int newItemQuantity, float newFinalPrice)
        {
            if (!this.ItemQuantitiesWithFinalPrice.ContainsKey(itemId))
            {
                throw new ArgumentException("Item #" + itemId + " doesn't exist");
            }

            this.ItemQuantitiesWithFinalPrice[itemId] = new Tuple<int, float>(newItemQuantity, newFinalPrice);
        }

        public void RemoveItemFromOrder(int itemId)
        {
            if (!this.ItemQuantitiesWithFinalPrice.ContainsKey(itemId))
            {
                throw new ArgumentException("Item #" + itemId + " doesn't exist");
            }

            this.ItemQuantitiesWithFinalPrice.Remove(itemId);
        }
    }
}
