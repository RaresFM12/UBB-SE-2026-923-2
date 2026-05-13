namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using UBB_SE_2026_923_2.Command;
    using UBB_SE_2026_923_2.Services;

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