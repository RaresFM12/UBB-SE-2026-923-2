namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using UBB_SE_2026_923_2.Command;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public class EditDetailViewModel : INotifyPropertyChanged
    {
        private readonly IOrderService orderService;

        public ICommand RemoveItemCommand { get; set; }

        public ObservableCollection<ItemDetail> OrderItems { get; private set; }

        private string totalPriceString;

        public string TotalPriceString
        {
            get => this.totalPriceString;
            set
            {
                this.totalPriceString = value;
                this.OnPropertyChanged();
            }
        }

        public string StatusString { get; private set; }

        public DateOnly PickUpDate { get; private set; }

        public string PickUpDateString
        {
            get => this.PickUpDate.ToString("yyyy.MM.dd");
        }

        public int ShownOrderID;

        public EditDetailViewModel(IOrderService orderService, int orderID)
        {
            this.ShownOrderID = orderID;
            this.orderService = orderService;
            this.RemoveItemCommand = new RelayCommandWithOneParameter<ItemDetail>(this.RemoveItemFromUnsavedOrder);

            Order currOrder = this.orderService.OrdersRepository.GetOrder(orderID);
            Dictionary<int, Tuple<int, float>> itemsInOrder = currOrder.ItemQuantitiesWithFinalPrice;
            this.OrderItems = new();
            float totalPrice = 0f;

            foreach (KeyValuePair<int, Tuple<int, float>> orderEntry in itemsInOrder)
            {
                Item currentItem = this.orderService.ItemsRepository.GetItemById(orderEntry.Key);
                string alteredImagePath = currentItem.ImagePath;

                string itemDescription = currentItem.Name + " - " + currentItem.Producer;
                int itemQuantity = orderEntry.Value.Item1;
                float itemTotalPrice = orderEntry.Value.Item2;

                this.OrderItems.Add(
                    new ItemDetail(currentItem.Id, alteredImagePath, itemDescription,
                                    itemQuantity, itemTotalPrice));

                totalPrice += itemTotalPrice;
            }

            this.TotalPriceString = totalPrice.ToString("0.00") + " RON";

            if (!currOrder.IsExpired && !currOrder.IsCompleted)
            {
                this.StatusString = "Incomplete";
            }
            else if (currOrder.IsExpired)
            {
                this.StatusString = "Expired";
            }
            else
            {
                this.StatusString = "Complete";
            }

            this.PickUpDate = currOrder.PickUpDate;
        }

        private void RemoveItemFromUnsavedOrder(ItemDetail itemToRemove)
        {
            this.OrderItems.Remove(itemToRemove);

            this.UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            float newTotalPrice = 0f;

            foreach (ItemDetail item in this.OrderItems)
            {
                newTotalPrice += item.ItemFinalPrice;
            }

            this.TotalPriceString = newTotalPrice.ToString("0.00") + " RON";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
