using System;
using System.Collections.Generic;

namespace UBB_SE_2026_923_2.Models
{
    public class Order : IEquatable<Order>
    {
        public const int OrderExpirationDays = 7;

        public int Id { get; private set; }
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
        public Dictionary<int, Tuple<int, float>> ItemQuantitiesWithFinalPrice { get; private set; }
        public Order(int id, int clientId, DateOnly pickUpDate,
                     bool isCompleted = false, bool isExpired = false)
        {
            Id = id;
            ClientId = clientId;
            PickUpDate = pickUpDate;
            IsCompleted = isCompleted;
            IsExpired = isExpired;
            ItemQuantitiesWithFinalPrice = new Dictionary<int, Tuple<int, float>>();
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
