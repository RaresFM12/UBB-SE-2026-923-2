namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    using System;
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public class ItemDetail
    {
        public int ItemID { get; private set; }

        public string ItemThumbnailImagePath { get; private set; }

        public string ItemDescription { get; private set; }

        public string ItemQuantityString
        {
            get => $"Quantity: {this.ItemQuantity}";
        }

        public string ItemFinalPriceString
        {
            get => $"{this.ItemFinalPrice:0.00} RON";
        }

        public int ItemQuantity { get; private set; }

        public float ItemFinalPrice { get; private set; }

        public ItemDetail(int itemID, string imagePath, string description, int quantity, float finalPrice)
        {
            this.ItemID = itemID;
            this.ItemThumbnailImagePath = imagePath;
            this.ItemDescription = description;
            this.ItemQuantity = quantity;
            this.ItemFinalPrice = finalPrice;
        }
    }

    public class NonEditDetailViewModel : INonEditViewModel
    {
        private readonly IOrderService orderService;

        public List<ItemDetail> OrderItems { get; private set; }

        public string TotalPriceString { get; private set; }

        public string StatusString { get; private set; }

        public DateOnly PickUpDate { get; private set; }

        public string PickUpDateString
        {
            get => this.PickUpDate.ToString("yyyy.MM.dd");
        }

        public NonEditDetailViewModel(IOrderService orderServ, int orderID)
        {
            this.orderService = orderServ;
            this.OrderItems = new();

            Order shownOrder = this.orderService.OrdersRepository.GetOrder(orderID);
            float totalPrice = 0f;

            foreach (var currentOrderEntry in shownOrder.ItemQuantitiesWithFinalPrice)
            {
                int itemID = currentOrderEntry.Key;
                int itemQuantity = currentOrderEntry.Value.Item1;
                float itemTotalPrice = currentOrderEntry.Value.Item2;

                Item currentItem = orderServ.ItemsRepository.GetItemById(itemID);

                string alteredImagePath = currentItem.ImagePath;

                string itemDescription = currentItem.Name + " - " + currentItem.Producer;

                this.OrderItems.Add(
                    new ItemDetail(itemID, alteredImagePath, itemDescription,
                                    itemQuantity, itemTotalPrice));

                totalPrice += itemTotalPrice;
            }

            this.TotalPriceString = totalPrice.ToString("0.00") + " RON";

            if (!shownOrder.IsExpired && !shownOrder.IsCompleted)
            {
                this.StatusString = "Incomplete";
            }
            else if (shownOrder.IsExpired)
            {
                this.StatusString = "Expired";
            }
            else
            {
                this.StatusString = "Complete";
            }

            this.PickUpDate = shownOrder.PickUpDate;
        }
    }
}
