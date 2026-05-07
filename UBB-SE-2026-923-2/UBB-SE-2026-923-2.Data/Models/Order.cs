using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UBB_SE_2026_923_2.Models
{
    public class Order : IEquatable<Order>
    {
        public const int OrderExpirationDays = 7;

        // Setter opened up so System.Text.Json can rehydrate Id over HTTP.
        public int Id { get; set; }
        public string IdString
        {
            get { return "Order#" + Id; }
        }
        public int ClientId { get; set; }
        public DateOnly PickUpDate { get; set; }
        public string PickUpDateString
        {
            get { return PickUpDate.ToString("yyyy.MM.dd"); }
        }
        public string ExpirationDateString
        {
            get { return PickUpDate.AddDays(OrderExpirationDays).ToString("yyyy.MM.dd"); }
        }
        public bool IsCompleted { get; set; }
        public bool IsExpired { get; set; }

        // Legacy in-memory view — not persisted. Phase 2 will migrate callers
        // onto OrderItemEntries below.
        [NotMapped]
        public Dictionary<int, Tuple<int, float>> ItemQuantitiesWithFinalPrice { get; set; }

        // ---- EF Core navigation properties (persisted) ----
        // [JsonIgnore]: ClientId already carries the FK; OrderItemEntries are
        // projected into the legacy dictionary by the server before returning.
        [JsonIgnore]
        public User? Client { get; set; }
        [JsonIgnore]
        public ICollection<OrderItem> OrderItemEntries { get; set; } = new List<OrderItem>();

        public Order()
        {
            ItemQuantitiesWithFinalPrice = new Dictionary<int, Tuple<int, float>>();
        }

        public Order(int id, int clientId, DateOnly pickUpDate,
                     bool isCompleted = false, bool isExpired = false) : this()
        {
            Id = id;
            ClientId = clientId;
            PickUpDate = pickUpDate;
            IsCompleted = isCompleted;
            IsExpired = isExpired;
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
            if (ItemQuantitiesWithFinalPrice.ContainsKey(newItemId))
            {
                throw new ArgumentException("Item #" + newItemId + " already exists in order");
            }

            ItemQuantitiesWithFinalPrice[newItemId] = new Tuple<int, float>(itemQuantity, finalPrice);
        }

        public void ChangeItemInfoInOrder(int itemId, int newItemQuantity, float newFinalPrice)
        {
            if (!ItemQuantitiesWithFinalPrice.ContainsKey(itemId))
            {
                throw new ArgumentException("Item #" + itemId + " doesn't exist");
            }

            ItemQuantitiesWithFinalPrice[itemId] = new Tuple<int, float>(newItemQuantity, newFinalPrice);
        }

        public void RemoveItemFromOrder(int itemId)
        {
            if (!ItemQuantitiesWithFinalPrice.ContainsKey(itemId))
            {
                throw new ArgumentException("Item #" + itemId + " doesn't exist");
            }

            ItemQuantitiesWithFinalPrice.Remove(itemId);
        }
    }
}
