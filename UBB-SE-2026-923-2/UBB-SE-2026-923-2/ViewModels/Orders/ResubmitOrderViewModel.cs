namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    using System;
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public class ResubmitOrderViewModel : IResubmitOrderViewModel
    {
        private readonly IOrderService orderService;

        public int ShownOrderID { get; set; }

        public List<ItemDetail> OrderItems { get; private set; }

        public string TotalPriceString { get; private set; }

        public ResubmitOrderViewModel(IOrderService injectedOrderService, int currOrderID)
        {
            this.orderService = injectedOrderService;
            this.ShownOrderID = currOrderID;

            Order currOrder = this.orderService.OrdersRepository.GetOrder(currOrderID);
            Dictionary<int, Tuple<int, float>> itemsInOrder = currOrder.ItemQuantitiesWithFinalPrice;
            this.OrderItems = new();

            foreach (KeyValuePair<int, Tuple<int, float>> orderItemEntry in itemsInOrder)
            {
                Item currentItem = this.orderService.ItemsRepository.GetItemById(orderItemEntry.Key);

                string alteredImagePath = currentItem.ImagePath;

                ItemDetail itemRepresentation = new ItemDetail(
                        currentItem.Id,
                        alteredImagePath,
                        currentItem.Name + " - " + currentItem.Producer,
                        orderItemEntry.Value.Item1,
                        orderItemEntry.Value.Item2,
                        currentItem.Name,
                        currentItem.Producer);

                this.OrderItems.Add(itemRepresentation);
            }

            float totalPrice = 0f;

            foreach (ItemDetail item in this.OrderItems)
            {
                totalPrice += item.ItemFinalPrice;
            }

            this.TotalPriceString = totalPrice.ToString("0.00") + " RON";
        }
    }
}
