using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using UBB_SE_2026_923_2.Models;
using Syncfusion.UI.Xaml.Core;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.ViewModels.PeriodTracker
{
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

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public int Id { get; }

        public ICommand AddToBasketCommand { get; }

        public float ExtraDiscountPercentage { get; }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (name == value)
                {
                    return;
                }

                name = value;
                OnPropertyChanged();
            }
        }

        private string priceString;
        public string PriceString
        {
            get => priceString;
            set
            {
                if (priceString == value)
                {
                    return;
                }

                priceString = value;
                OnPropertyChanged();
            }
        }

        private string priceDiscountedString;
        public string PriceDiscountedString
        {
            get => priceDiscountedString;
            set
            {
                if (priceDiscountedString == value)
                {
                    return;
                }

                priceDiscountedString = value;
                OnPropertyChanged();
            }
        }

        private string priceColor;
        public string PriceColor
        {
            get => priceColor;
            set
            {
                if (priceColor == value)
                {
                    return;
                }

                priceColor = value;
                OnPropertyChanged();
            }
        }

        private string finalPriceColor;
        public string FinalPriceColor
        {
            get => finalPriceColor;
            set
            {
                if (finalPriceColor == value)
                {
                    return;
                }

                finalPriceColor = value;
                OnPropertyChanged();
            }
        }

        private string imagePath;
        public string ImagePath
        {
            get => imagePath;
            set
            {
                if (imagePath == value)
                {
                    return;
                }

                imagePath = value;
                OnPropertyChanged();
            }
        }

        public ItemViewModel(Item item, float extraDiscountPercentage, IBasketService basketService)
        {
            this.basketService = basketService;

            Id = item.Id;
            Name = item.Name;
            ExtraDiscountPercentage = extraDiscountPercentage;

            float originalPrice = item.Price;
            float finalPrice = CalculateFinalPrice(item.Price, item.DiscountPercentage, ExtraDiscountPercentage);

            if (finalPrice < originalPrice)
            {
                PriceDiscountedString = originalPrice.ToString("C", CultureInfo.CurrentCulture);
                PriceColor = DiscountedPriceColorName;
                FinalPriceColor = FinalDiscountedPriceColorName;
            }
            else
            {
                PriceDiscountedString = string.Empty;
                PriceColor = RegularPriceColorName;
                FinalPriceColor = RegularFinalPriceColorName;
            }

            PriceString = finalPrice.ToString("C", CultureInfo.CurrentCulture);
            ImagePath = BuildImagePath(item.ImagePath);

            AddToBasketCommand = new DelegateCommand(
                ignoredParameter => this.basketService.AddToBasket(Id, DefaultBasketQuantity, ExtraDiscountPercentage));
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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