namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Command;

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

        public bool Equals(BasketItemViewModel other)
        {
            if (other is null)
            {
                return false;
            }

            return this.ItemId == other.ItemId;
        }

        public override bool Equals(object obj) => this.Equals(obj as BasketItemViewModel);

        public override int GetHashCode() => this.ItemId.GetHashCode();

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BasketViewModel : INotifyPropertyChanged
    {
        private const int EmptyQuantity = 0;

        private readonly IOrderService orderService;
        private string totalPriceBeforeDiscount;
        private string totalPriceAfterDiscount;

        public ICommand RemoveItemCommand { get; }

        public ObservableCollection<BasketItemViewModel> BasketItems { get; }

        public string TotalPriceString
        {
            get => this.totalPriceBeforeDiscount;
            set
            {
                if (this.totalPriceBeforeDiscount == value)
                {
                    return;
                }

                this.totalPriceBeforeDiscount = value;
                this.OnPropertyChanged();
            }
        }

        public string TotalDiscountedPriceString
        {
            get => this.totalPriceAfterDiscount;
            set
            {
                if (this.totalPriceAfterDiscount == value)
                {
                    return;
                }

                this.totalPriceAfterDiscount = value;
                this.OnPropertyChanged();
            }
        }

        public BasketViewModel(IOrderService newOrderService)
        {
            this.orderService = newOrderService;
            this.RemoveItemCommand = new RelayCommandWithOneParameter<BasketItemViewModel>(this.RemoveItemFromBasket);
            this.BasketItems = new ObservableCollection<BasketItemViewModel>();

            this.LoadBasketItems();
            this.UpdateTotalPrices();
        }

        private void LoadBasketItems()
        {
            foreach (BasketItemViewModel existingItem in this.BasketItems)
            {
                existingItem.PropertyChanged -= this.UpdateItemInBasket;
            }

            this.BasketItems.Clear();

            foreach (BasketItemViewModel basketItem in this.orderService.GetBasketItems())
            {
                basketItem.PropertyChanged += this.UpdateItemInBasket;
                this.BasketItems.Add(basketItem);
            }
        }

        private void RemoveItemFromBasket(BasketItemViewModel itemToRemove)
        {
            if (itemToRemove == null)
            {
                return;
            }

            this.orderService.RemoveFromBasket(itemToRemove.ItemId);
            itemToRemove.PropertyChanged -= this.UpdateItemInBasket;
            this.BasketItems.Remove(itemToRemove);

            this.OnBasketQuantityRemoved();
            this.UpdateTotalPrices();
        }

        private void UpdateItemInBasket(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(BasketItemViewModel.ItemQuantityInBasket))
            {
                return;
            }

            BasketItemViewModel itemToUpdate = (BasketItemViewModel)sender;
            this.orderService.RecalculateBasketItemPrices(itemToUpdate);

            if (itemToUpdate.ItemQuantityInBasket <= EmptyQuantity)
            {
                this.orderService.RemoveFromBasket(itemToUpdate.ItemId);
                itemToUpdate.PropertyChanged -= this.UpdateItemInBasket;
                this.BasketItems.Remove(itemToUpdate);
            }
            else
            {
                this.orderService.UpdateBasketItemQuantity(itemToUpdate.ItemId, itemToUpdate.ItemQuantityInBasket);
            }

            this.OnBasketQuantityRemoved();
            this.UpdateTotalPrices();
        }

        private void UpdateTotalPrices()
        {
            Tuple<float, float> totals = this.orderService.CalculateBasketTotalSum(this.BasketItems);

            this.TotalPriceString = $"{totals.Item1:0.00} RON";
            this.TotalDiscountedPriceString = $"{totals.Item2:0.00} RON";
        }

        public void GetPrescription(string prescriptionId)
        {
            this.orderService.ApplyPrescriptionToBasket(prescriptionId);

            this.LoadBasketItems();
            this.UpdateTotalPrices();
            this.OnBasketQuantityRemoved();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public delegate void QuantityChanged(int quantity);

        public event QuantityChanged BasketQuantityRemoved;

        public virtual void OnBasketQuantityRemoved()
        {
            int totalQuantity = this.BasketItems.Sum(item => item.ItemQuantityInBasket);
            this.BasketQuantityRemoved?.Invoke(totalQuantity);
        }
    }
}