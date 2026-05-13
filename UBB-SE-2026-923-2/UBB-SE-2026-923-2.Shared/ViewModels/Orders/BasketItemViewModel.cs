namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class BasketItemViewModel : INotifyPropertyChanged, IEquatable<BasketItemViewModel>
    {
        private const int MinimumQuantity = 0;
        private const float PriceChangeTolerance = 0.0001f;
        private const int PercentageFactor = 100;

        private float finalPriceBeforeDiscount;
        private float finalPriceAfterDiscount;
        private int quantity;

        public int ItemId { get; }

        public string ItemThumbnailImagePath { get; }

        public string ItemName { get; }

        public string ItemProducer { get; }

        public float InitialPricePerBox { get; }

        public float BaseItemDiscount { get; }

        public float ExtraItemDiscount { get; }

        public float ItemActiveDiscount => 1 - ((1 - this.BaseItemDiscount) * (1 - this.ExtraItemDiscount));

        public float ItemActiveUserDiscount { get; }

        public int ItemQuantityInBasket
        {
            get => this.quantity;
            set
            {
                int safeValue = Math.Max(MinimumQuantity, value);

                if (this.quantity == safeValue)
                {
                    return;
                }

                this.quantity = safeValue;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.ItemQuantityString));
            }
        }

        public float FinalPriceBeforeDiscount
        {
            get => this.finalPriceBeforeDiscount;
            private set
            {
                if (Math.Abs(this.finalPriceBeforeDiscount - value) < PriceChangeTolerance)
                {
                    return;
                }

                this.finalPriceBeforeDiscount = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.ItemFinalPriceString));
            }
        }

        public float FinalPriceAfterDiscount
        {
            get => this.finalPriceAfterDiscount;
            private set
            {
                if (Math.Abs(this.finalPriceAfterDiscount - value) < PriceChangeTolerance)
                {
                    return;
                }

                this.finalPriceAfterDiscount = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.ItemFinalDiscountedPriceString));
            }
        }

        public string ItemDescription => $"{this.ItemName} - {this.ItemProducer}";

        public string ItemQuantityString => $"Quantity: {this.ItemQuantityInBasket}";

        public string ItemDiscountString => $"-{(int)Math.Round(this.ItemActiveDiscount * PercentageFactor)}%";

        public string ItemUserDiscountString => $"-{(int)Math.Round(this.ItemActiveUserDiscount * PercentageFactor)}%";

        public string ItemFinalPriceString => $"{this.FinalPriceBeforeDiscount:0.00} RON";

        public string ItemFinalDiscountedPriceString => $"{this.FinalPriceAfterDiscount:0.00} RON";

        public BasketItemViewModel(
            int itemId,
            string imagePath,
            string name,
            string producer,
            int quantity,
            float baseItemDiscount,
            float extraItemDiscount,
            float userDiscount,
            float initialPrice)
        {
            this.ItemId = itemId;
            this.ItemThumbnailImagePath = imagePath;
            this.ItemName = name;
            this.ItemProducer = producer;
            this.InitialPricePerBox = initialPrice;

            this.BaseItemDiscount = baseItemDiscount;
            this.ExtraItemDiscount = extraItemDiscount;
            this.ItemActiveUserDiscount = userDiscount;

            this.quantity = Math.Max(MinimumQuantity, quantity);
        }

        public void SetFinalPrices(float finalPriceBefore, float finalPriceAfter)
        {
            this.FinalPriceBeforeDiscount = finalPriceBefore;
            this.FinalPriceAfterDiscount = finalPriceAfter;
        }

        public bool Equals(BasketItemViewModel? other)
        {
            if (other is null)
            {
                return false;
            }

            return this.ItemId == other.ItemId;
        }

        public override bool Equals(object? obj) => this.Equals(obj as BasketItemViewModel);

        public override int GetHashCode() => this.ItemId.GetHashCode();

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
