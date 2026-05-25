namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public class ItemDetail : INotifyPropertyChanged
    {
        private const int MinimumQuantity = 0;
        private const float PriceChangeTolerance = 0.0001f;

        private readonly float finalPricePerUnit;
        private int itemQuantity;
        private float itemFinalPrice;

        public int ItemID { get; private set; }

        public string ItemThumbnailImagePath { get; private set; }

        public string ItemDescription { get; private set; }

        public string MedicineName { get; private set; }

        public string Producer { get; private set; }

        public string ItemQuantityString
        {
            get => $"Quantity: {this.ItemQuantity}";
        }

        public string ItemFinalPriceString
        {
            get => $"{this.ItemFinalPrice:0.00} RON";
        }

        public int ItemQuantity
        {
            get => this.itemQuantity;
            set
            {
                int safeValue = Math.Max(MinimumQuantity, value);
                if (this.itemQuantity == safeValue)
                {
                    return;
                }

                this.itemQuantity = safeValue;
                this.ItemFinalPrice = this.finalPricePerUnit * safeValue;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.ItemQuantityString));
                this.OnPropertyChanged(nameof(this.ItemQuantityValue));
            }
        }

        public double ItemQuantityValue
        {
            get => this.ItemQuantity;
            set => this.ItemQuantity = (int)Math.Max(MinimumQuantity, Math.Round(value));
        }

        public float ItemFinalPrice
        {
            get => this.itemFinalPrice;
            private set
            {
                if (Math.Abs(this.itemFinalPrice - value) < PriceChangeTolerance)
                {
                    return;
                }

                this.itemFinalPrice = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.ItemFinalPriceString));
            }
        }

        public ItemDetail(
            int itemID,
            string imagePath,
            string description,
            int quantity,
            float finalPrice,
            string medicineName = "",
            string producer = "")
        {
            this.ItemID = itemID;
            this.ItemThumbnailImagePath = imagePath;
            this.ItemDescription = description;
            this.MedicineName = string.IsNullOrWhiteSpace(medicineName) ? description : medicineName;
            this.Producer = producer;
            this.finalPricePerUnit = quantity > MinimumQuantity ? finalPrice / quantity : 0f;
            this.itemQuantity = Math.Max(MinimumQuantity, quantity);
            this.itemFinalPrice = finalPrice;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class NonEditDetailViewModel : INonEditViewModel
    {
        private readonly IOrderService orderService;

        public List<ItemDetail> OrderItems { get; private set; }

        public string TotalPriceString { get; private set; }

        public string UserEmail { get; private set; }

        public string StatusString { get; private set; }

        public DateOnly PickUpDate { get; private set; }

        public string PickUpDateString
        {
            get => this.PickUpDate.ToString("yyyy.MM.dd");
        }

        public DateOnly ExpirationDate { get; private set; }

        public string ExpirationDateString
        {
            get => this.ExpirationDate.ToString("yyyy.MM.dd");
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
                                    itemQuantity, itemTotalPrice,
                                    currentItem.Name, currentItem.Producer));

                totalPrice += itemTotalPrice;
            }

            this.TotalPriceString = totalPrice.ToString("0.00") + " RON";
            this.UserEmail = this.GetClientEmail(shownOrder);

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
            this.ExpirationDate = shownOrder.PickUpDate.AddDays(Order.OrderExpirationDays);
        }

        private string GetClientEmail(Order shownOrder)
        {
            if (shownOrder.Client != null && !string.IsNullOrWhiteSpace(shownOrder.Client.Email))
            {
                return shownOrder.Client.Email;
            }

            if (shownOrder.ClientId > 0)
            {
                return this.orderService.UsersRepository.GetUserById(shownOrder.ClientId)?.Email ?? "Unknown";
            }

            if (this.orderService.ActiveUser != null)
            {
                bool activeUserOwnsOrder = this.orderService.OrdersRepository
                    .GetOrdersOfClient(this.orderService.ActiveUser.Id)
                    .Any(order => order.Id == shownOrder.Id);

                if (activeUserOwnsOrder)
                {
                    return this.orderService.ActiveUser.Email;
                }
            }

            foreach (User user in this.orderService.UsersRepository.GetAllUsers())
            {
                bool orderBelongsToUser = this.orderService.OrdersRepository
                    .GetOrdersOfClient(user.Id)
                    .Any(order => order.Id == shownOrder.Id);

                if (orderBelongsToUser)
                {
                    return user.Email;
                }
            }

            return "Unknown";
        }
    }
}
