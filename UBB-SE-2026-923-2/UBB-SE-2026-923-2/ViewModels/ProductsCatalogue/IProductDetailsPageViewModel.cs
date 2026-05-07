namespace UBB_SE_2026_923_2.ViewModels.ProductsCatalogue
{
    using System.ComponentModel;
    using Microsoft.UI.Xaml.Media;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public interface IProductDetailsPageViewModel : INotifyPropertyChanged
    {
        void Initialize(Item item, User user, IOrderService orderService);

        (bool success, bool navigateToLogin) TryAddToBasket(string quantityText);

        string ProductName { get; }

        string FinalPriceDisplay { get; }

        string OldPriceDisplay { get; }

        string DiscountDisplay { get; }

        bool HasDiscount { get; }

        string StockText { get; }

        StockLevel CurrentStockLevel { get; }

        bool IsAddToCartEnabled { get; }

        bool IsQuantityBoxEnabled { get; }

        bool IsStockAlertButtonVisible { get; }

        string StockAlertButtonText { get; }

        (bool success, bool navigateToLogin) ToggleStockAlert();

        string DescriptionText { get; }

        string LabelText { get; }

        string ProducerText { get; }

        string CategoryText { get; }

        string PillsText { get; }

        string ImagePath { get; }

        string ErrorText { get; }
    }
}