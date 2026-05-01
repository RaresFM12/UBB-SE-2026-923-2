using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using UBB_SE_2026_923_2.Command;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    public class BasketItemViewModel : INotifyPropertyChanged, IEquatable<BasketItemViewModel>
    {
        private const int MinQuantity = 0;
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
        public float ItemActiveDiscount => 1 - ((1 - BaseItemDiscount) * (1 - ExtraItemDiscount));
        public float ItemActiveUserDiscount { get; }

        public int ItemQuantityInBasket
        {
            get => quantity;
            set
            {
                int safeValue = Math.Max(MinQuantity, value);

                if (quantity == safeValue)
                {
                    return;
                }

                quantity = safeValue;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemQuantityString));
            }
        }

        public float FinalPriceBeforeDiscount
        {
            get => finalPriceBeforeDiscount;
            private set
            {
                if (Math.Abs(finalPriceBeforeDiscount - value) < PriceChangeTolerance)
                {
                    return;
                }

                finalPriceBeforeDiscount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemFinalPriceString));
            }
        }

        public float FinalPriceAfterDiscount
        {
            get => finalPriceAfterDiscount;
            private set
            {
                if (Math.Abs(finalPriceAfterDiscount - value) < PriceChangeTolerance)
                {
                    return;
                }

                finalPriceAfterDiscount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemFinalDiscountedPriceString));
            }
        }

        public string ItemDescription => $"{ItemName} - {ItemProducer}";
        public string ItemQuantityString => $"Quantity: {ItemQuantityInBasket}";
        public string ItemDiscountString => $"-{(int)Math.Round(ItemActiveDiscount * PercentageFactor)}%";
        public string ItemUserDiscountString => $"-{(int)Math.Round(ItemActiveUserDiscount * PercentageFactor)}%";
        public string ItemFinalPriceString => $"{FinalPriceBeforeDiscount:0.00} RON";
        public string ItemFinalDiscountedPriceString => $"{FinalPriceAfterDiscount:0.00} RON";

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
            ItemId = itemId;
            ItemThumbnailImagePath = imagePath;
            ItemName = name;
            ItemProducer = producer;
            InitialPricePerBox = initialPrice;

            BaseItemDiscount = baseItemDiscount;
            ExtraItemDiscount = extraItemDiscount;
            ItemActiveUserDiscount = userDiscount;

            this.quantity = Math.Max(MinQuantity, quantity);
        }

        public void SetFinalPrices(float finalPriceBefore, float finalPriceAfter)
        {
            FinalPriceBeforeDiscount = finalPriceBefore;
            FinalPriceAfterDiscount = finalPriceAfter;
        }

        public bool Equals(BasketItemViewModel other)
        {
            if (other is null)
            {
                return false;
            }

            return ItemId == other.ItemId;
        }

        public override bool Equals(object obj) => Equals(obj as BasketItemViewModel);
        public override int GetHashCode() => ItemId.GetHashCode();

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            get => totalPriceBeforeDiscount;
            set
            {
                if (totalPriceBeforeDiscount == value)
                {
                    return;
                }

                totalPriceBeforeDiscount = value;
                OnPropertyChanged();
            }
        }

        public string TotalDiscountedPriceString
        {
            get => totalPriceAfterDiscount;
            set
            {
                if (totalPriceAfterDiscount == value)
                {
                    return;
                }

                totalPriceAfterDiscount = value;
                OnPropertyChanged();
            }
        }

        public BasketViewModel(IOrderService newOrderService)
        {
            orderService = newOrderService;
            RemoveItemCommand = new RelayCommandWithOneParameter<BasketItemViewModel>(RemoveItemFromBasket);
            BasketItems = new ObservableCollection<BasketItemViewModel>();

            LoadBasketItems();
            UpdateTotalPrices();
        }

        private void LoadBasketItems()
        {
            foreach (BasketItemViewModel existingItem in BasketItems)
            {
                existingItem.PropertyChanged -= UpdateItemInBasket;
            }

            BasketItems.Clear();

            foreach (BasketItemViewModel basketItem in orderService.GetBasketItems())
            {
                basketItem.PropertyChanged += UpdateItemInBasket;
                BasketItems.Add(basketItem);
            }
        }

        private void RemoveItemFromBasket(BasketItemViewModel itemToRemove)
        {
            if (itemToRemove == null)
            {
                return;
            }

            orderService.RemoveFromBasket(itemToRemove.ItemId);
            itemToRemove.PropertyChanged -= UpdateItemInBasket;
            BasketItems.Remove(itemToRemove);

            OnBasketQuantityRemoved();
            UpdateTotalPrices();
        }

        private void UpdateItemInBasket(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(BasketItemViewModel.ItemQuantityInBasket))
            {
                return;
            }

            BasketItemViewModel itemToUpdate = (BasketItemViewModel)sender;
            orderService.RecalculateBasketItemPrices(itemToUpdate);

            if (itemToUpdate.ItemQuantityInBasket <= EmptyQuantity)
            {
                orderService.RemoveFromBasket(itemToUpdate.ItemId);
                itemToUpdate.PropertyChanged -= UpdateItemInBasket;
                BasketItems.Remove(itemToUpdate);
            }
            else
            {
                orderService.UpdateBasketItemQuantity(itemToUpdate.ItemId, itemToUpdate.ItemQuantityInBasket);
            }

            OnBasketQuantityRemoved();
            UpdateTotalPrices();
        }

        private void UpdateTotalPrices()
        {
            Tuple<float, float> totals = orderService.CalculateBasketTotalSum(BasketItems);

            TotalPriceString = $"{totals.Item1:0.00} RON";
            TotalDiscountedPriceString = $"{totals.Item2:0.00} RON";
        }

        public void GetPrescription(string prescriptionId)
        {
            orderService.ApplyPrescriptionToBasket(prescriptionId);

            LoadBasketItems();
            UpdateTotalPrices();
            OnBasketQuantityRemoved();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public delegate void QuantityChanged(int quantity);
        public event QuantityChanged BasketQuantityRemoved;

        public virtual void OnBasketQuantityRemoved()
        {
            int totalQuantity = BasketItems.Sum(item => item.ItemQuantityInBasket);
            BasketQuantityRemoved?.Invoke(totalQuantity);
        }
    }
}