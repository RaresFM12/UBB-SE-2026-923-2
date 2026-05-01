using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.ViewModels.ProductsCatalogue
{
    public enum StockLevel
    {
        Unknown,
        OutOfStock,
        LowStock,
        InStock
    }

    public class ProductDetailsPageViewModel : IProductDetailsPageViewModel
    {
        private Item currentItem;
        public User CurrentUser { get; private set; }
        public IOrderService OrderService { get; private set; }

        public string ProductName => currentItem?.Name ?? string.Empty;

        public string FinalPriceDisplay =>
            this.currentItem == null
                ? string.Empty
                : $"{this.currentItem.Price * (1 - (currentItem.DiscountPercentage / 100)):F2} lei";

        public string OldPriceDisplay =>
            HasDiscount ? $"{currentItem.Price:F2} lei" : string.Empty;

        public string DiscountDisplay =>
            HasDiscount ? $"{currentItem.DiscountPercentage}% off" : string.Empty;

        public bool HasDiscount => (currentItem?.DiscountPercentage ?? 0) > 0;

        public string StockText
        {
            get
            {
                if (currentItem == null)
                {
                    return string.Empty;
                }

                if (currentItem.Quantity == 0)
                {
                    return "Out of stock";
                }

                if (currentItem.Quantity < ProductCatalogueService.LowStockThreshold)
                {
                    return $"Only {currentItem.Quantity} in stock";
                }

                return "In stock";
            }
        }

        public StockLevel CurrentStockLevel
        {
            get
            {
                if (currentItem == null)
                {
                    return StockLevel.Unknown;
                }

                if (currentItem.Quantity == 0)
                {
                    return StockLevel.OutOfStock;
                }

                if (currentItem.Quantity < ProductCatalogueService.LowStockThreshold)
                {
                    return StockLevel.LowStock;
                }

                return StockLevel.InStock;
            }
        }

        public bool IsAddToCartEnabled => (currentItem?.Quantity ?? 0) > 0;
        public bool IsQuantityBoxEnabled => (currentItem?.Quantity ?? 0) > 0;

        public bool IsStockAlertButtonVisible => currentItem != null && currentItem.Quantity == 0 && CurrentUser != null;

        public string StockAlertButtonText =>
            CurrentUser != null && currentItem != null && CurrentUser.StockAlerts.Contains(currentItem.Id)
                ? "Unsubscribe from stock alert"
                : "Notify when in stock";

        public (bool success, bool navigateToLogin) ToggleStockAlert()
        {
            if (CurrentUser == null)
            {
                return (false, true);
            }

            if (CurrentUser.StockAlerts.Contains(currentItem.Id))
            {
                CurrentUser.RemoveStockAlertFromUser(currentItem.Id);
            }
            else
            {
                CurrentUser.AddStockAlertToUser(currentItem.Id);
            }

            OrderService.UsersRepository.UpdateUser(CurrentUser);
            OnPropertyChanged(nameof(StockAlertButtonText));
            return (true, false);
        }

        public string DescriptionText => currentItem?.Description ?? string.Empty;
        public string LabelText => currentItem?.Label ?? string.Empty;
        public string ProducerText => currentItem?.Producer ?? string.Empty;
        public string CategoryText => currentItem?.Category ?? string.Empty;
        public string PillsText => currentItem?.NumberOfPills.ToString() ?? string.Empty;

        public string SubstancesText =>
            currentItem?.ActiveSubstances != null && currentItem.ActiveSubstances.Any()
                ? string.Join(", ", currentItem.ActiveSubstances.Select(s => $"{s.Key} ({s.Value})"))
                : "None";

        public string ImagePath => currentItem?.ImagePath ?? string.Empty;

        private string errorText = string.Empty;
        public string ErrorText
        {
            get => errorText;
            private set
            {
                errorText = value;
                OnPropertyChanged();
            }
        }

        public void Initialize(Item item, User user, IOrderService orderService)
        {
            currentItem = item;
            CurrentUser = user;
            OrderService = orderService;

            OnPropertyChanged(string.Empty);
        }

        public (bool success, bool navigateToLogin) TryAddToBasket(string quantityText)
        {
            ErrorText = string.Empty;

            if (CurrentUser == null)
            {
                return (false, true);
            }

            if (!int.TryParse(quantityText, out int qty) || qty <= 0)
            {
                ErrorText = "Invalid quantity selected";
                return (false, false);
            }

            if (qty > 50 || qty > currentItem.Quantity)
            {
                ErrorText = "Invalid quantity selected";
                return (false, false);
            }

            try
            {
                OrderService.AddToBasket(currentItem.Id, qty);
                return (true, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("THE REAL ERROR IS: " + ex.Message);
                ErrorText = "Item already in basket";
                return (false, false);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}