namespace UBB_SE_2026_923_2.ViewModels.ProductsCatalogue
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public enum StockLevel
    {
        Unknown,
        OutOfStock,
        LowStock,
        InStock,
    }

    public class ProductDetailsPageViewModel : IProductDetailsPageViewModel
    {
        private Item currentItem;

        public User CurrentUser { get; private set; }

        public IOrderService OrderService { get; private set; }

        public string ProductName => this.currentItem?.Name ?? string.Empty;

        public string FinalPriceDisplay =>
            this.currentItem == null
                ? string.Empty
                : $"{this.currentItem.Price * (1 - (this.currentItem.DiscountPercentage / 100)):F2} lei";

        public string OldPriceDisplay =>
            this.HasDiscount ? $"{this.currentItem.Price:F2} lei" : string.Empty;

        public string DiscountDisplay =>
            this.HasDiscount ? $"{Math.Round(this.currentItem.DiscountPercentage, 2):G}% off" : string.Empty;

        public bool HasDiscount => (this.currentItem?.DiscountPercentage ?? 0) > 0;

        public string StockText
        {
            get
            {
                if (this.currentItem == null)
                {
                    return string.Empty;
                }

                if (this.currentItem.Quantity == 0)
                {
                    return "Out of stock";
                }

                if (this.currentItem.Quantity < ProductCatalogueService.LowStockThreshold)
                {
                    return $"Only {this.currentItem.Quantity} in stock";
                }

                return "In stock";
            }
        }

        public StockLevel CurrentStockLevel
        {
            get
            {
                if (this.currentItem == null)
                {
                    return StockLevel.Unknown;
                }

                if (this.currentItem.Quantity == 0)
                {
                    return StockLevel.OutOfStock;
                }

                if (this.currentItem.Quantity < ProductCatalogueService.LowStockThreshold)
                {
                    return StockLevel.LowStock;
                }

                return StockLevel.InStock;
            }
        }

        public bool IsAddToCartEnabled => (this.currentItem?.Quantity ?? 0) > 0;

        public bool IsQuantityBoxEnabled => (this.currentItem?.Quantity ?? 0) > 0;

        public bool IsStockAlertButtonVisible => this.currentItem != null && this.currentItem.Quantity == 0 && this.CurrentUser != null;

        public string StockAlertButtonText =>
            this.CurrentUser != null && this.currentItem != null && this.CurrentUser.StockAlerts.Contains(this.currentItem.Id)
                ? "Unsubscribe from stock alert"
                : "Notify when in stock";

        public (bool success, bool navigateToLogin) ToggleStockAlert()
        {
            if (this.CurrentUser == null)
            {
                return (false, true);
            }

            if (this.CurrentUser.StockAlerts.Contains(this.currentItem.Id))
            {
                this.CurrentUser.RemoveStockAlertFromUser(this.currentItem.Id);
            }
            else
            {
                this.CurrentUser.AddStockAlertToUser(this.currentItem.Id);
            }

            this.OrderService.UsersRepository.UpdateUser(this.CurrentUser);
            this.OnPropertyChanged(nameof(this.StockAlertButtonText));
            return (true, false);
        }

        public string DescriptionText => this.currentItem?.Description ?? string.Empty;

        public string LabelText => this.currentItem?.Label ?? string.Empty;

        public string ProducerText => this.currentItem?.Producer ?? string.Empty;

        public string CategoryText => this.currentItem?.Category ?? string.Empty;

        public string PillsText => this.currentItem?.NumberOfPills.ToString() ?? string.Empty;

        public string SubstancesText =>
            this.currentItem?.ActiveSubstances != null && this.currentItem.ActiveSubstances.Any()
                ? string.Join(", ", this.currentItem.ActiveSubstances.Select(substance => $"{substance.Key} ({substance.Value})"))
                : "None";

        public string ImagePath => this.currentItem?.ImagePath ?? string.Empty;

        private string errorText = string.Empty;

        public string ErrorText
        {
            get => this.errorText;
            private set
            {
                this.errorText = value;
                this.OnPropertyChanged();
            }
        }

        public void Initialize(Item item, User user, IOrderService orderService)
        {
            this.currentItem = item;
            this.CurrentUser = user;
            this.OrderService = orderService;

            this.OnPropertyChanged(string.Empty);
        }

        public (bool success, bool navigateToLogin) TryAddToBasket(string quantityText)
        {
            this.ErrorText = string.Empty;

            if (this.CurrentUser == null)
            {
                return (false, true);
            }

            if (!int.TryParse(quantityText, out int quantity) || quantity <= 0)
            {
                this.ErrorText = "Invalid quantity selected";
                return (false, false);
            }

            if (quantity > 50 || quantity > this.currentItem.Quantity)
            {
                this.ErrorText = "Invalid quantity selected";
                return (false, false);
            }

            try
            {
                this.OrderService.AddToBasket(this.currentItem.Id, quantity);
                return (true, false);
            }
            catch (Exception exception)
            {
                Console.WriteLine("THE REAL ERROR IS: " + exception.Message);
                this.ErrorText = "Item already in basket";
                return (false, false);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}