namespace UBB_SE_2026_923_2.ViewModels.PeriodTracker
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using Syncfusion.UI.Xaml.Core;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public class ItemViewModel : INotifyPropertyChanged
    {
        private const float PercentageDivisor = 100.0f;
        private const int DefaultBasketQuantity = 1;

        private const string DiscountedPriceColorName = "Gray";
        private const string FinalDiscountedPriceColorName = "Green";
        private const string RegularPriceColorName = "Transparent";
        private const string RegularFinalPriceColorName = "Black";

        private const string PlaceholderImagePath = "ms-appx:///Assets/placeholder.png";
        private const string ApplicationPackagePrefix = "ms-appx://";
        private const char DotCharacter = '.';
        private const char SlashCharacter = '/';
        private const string WindowsSlash = "\\";
        private const string UnixSlash = "/";

        private readonly IBasketService basketService;

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public int ItemId { get; }

        public ICommand AddToBasketCommand { get; }

        public float ExtraDiscountPercentage { get; }

        private string name;

        public string Name
        {
            get => this.name;
            set
            {
                if (this.name == value)
                {
                    return;
                }

                this.name = value;
                this.OnPropertyChanged();
            }
        }

        private string priceString;

        public string PriceString
        {
            get => this.priceString;
            set
            {
                if (this.priceString == value)
                {
                    return;
                }

                this.priceString = value;
                this.OnPropertyChanged();
            }
        }

        private string priceDiscountedString;

        public string PriceDiscountedString
        {
            get => this.priceDiscountedString;
            set
            {
                if (this.priceDiscountedString == value)
                {
                    return;
                }

                this.priceDiscountedString = value;
                this.OnPropertyChanged();
            }
        }

        private string priceColor;

        public string PriceColor
        {
            get => this.priceColor;
            set
            {
                if (this.priceColor == value)
                {
                    return;
                }

                this.priceColor = value;
                this.OnPropertyChanged();
            }
        }

        private string finalPriceColor;

        public string FinalPriceColor
        {
            get => this.finalPriceColor;
            set
            {
                if (this.finalPriceColor == value)
                {
                    return;
                }

                this.finalPriceColor = value;
                this.OnPropertyChanged();
            }
        }

        private string imagePath;

        public string ImagePath
        {
            get => this.imagePath;
            set
            {
                if (this.imagePath == value)
                {
                    return;
                }

                this.imagePath = value;
                this.OnPropertyChanged();
            }
        }

        public ItemViewModel(Item item, float extraDiscountPercentage, IBasketService basketService)
        {
            this.basketService = basketService;

            this.ItemId = item.Id;
            this.Name = item.Name;
            this.ExtraDiscountPercentage = extraDiscountPercentage;

            float originalPrice = item.Price;
            float finalPrice = CalculateFinalPrice(item.Price, item.DiscountPercentage, this.ExtraDiscountPercentage);

            if (finalPrice < originalPrice)
            {
                this.PriceDiscountedString = originalPrice.ToString("C", CultureInfo.CurrentCulture);
                this.PriceColor = DiscountedPriceColorName;
                this.FinalPriceColor = FinalDiscountedPriceColorName;
            }
            else
            {
                this.PriceDiscountedString = string.Empty;
                this.PriceColor = RegularPriceColorName;
                this.FinalPriceColor = RegularFinalPriceColorName;
            }

            this.PriceString = finalPrice.ToString("C", CultureInfo.CurrentCulture);
            this.ImagePath = BuildImagePath(item.ImagePath);

            this.AddToBasketCommand = new DelegateCommand(
                ignoredParameter => this.basketService.AddToBasket(this.ItemId, DefaultBasketQuantity, this.ExtraDiscountPercentage));
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static float CalculateFinalPrice(
            float originalPrice,
            float itemDiscountPercentage,
            float extraDiscountPercentage)
        {
            float finalPrice = originalPrice;

            if (itemDiscountPercentage > 0)
            {
                float discountFactor = 1.0f - (itemDiscountPercentage / PercentageDivisor);
                finalPrice *= discountFactor;
            }

            if (extraDiscountPercentage > 0)
            {
                float discountFactor = 1.0f - (extraDiscountPercentage / PercentageDivisor);
                finalPrice *= discountFactor;
            }

            return finalPrice;
        }

        private static string BuildImagePath(string rawImagePath)
        {
            if (string.IsNullOrWhiteSpace(rawImagePath))
            {
                return PlaceholderImagePath;
            }

            if (rawImagePath.StartsWith(ApplicationPackagePrefix))
            {
                return rawImagePath;
            }

            string normalizedPath = rawImagePath
                .Replace(WindowsSlash, UnixSlash)
                .TrimStart(DotCharacter);

            if (!normalizedPath.StartsWith(UnixSlash))
            {
                normalizedPath = SlashCharacter + normalizedPath;
            }

            return ApplicationPackagePrefix + normalizedPath;
        }
    }
}