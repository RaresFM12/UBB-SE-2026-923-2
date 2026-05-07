namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using UBB_SE_2026_923_2.Command;
    using UBB_SE_2026_923_2.Services;

    public class CheckoutViewModel : ICheckoutViewModel
    {
        private readonly IOrderService orderService;

        public List<BasketItemViewModel> BasketItems { get; private set; }

        public string TotalPriceString { get; private set; }

        public ICommand PlaceOrderCommand { get; }

        public event Action OrderPlacedSuccessfully;

        public event Action<string> OrderPlacementFailed;

        public CheckoutViewModel(IOrderService injectedOrderService)
        {
            this.orderService = injectedOrderService;
            this.BasketItems = this.orderService.GetBasketItems();

            Tuple<float, float> totals = this.orderService.CalculateBasketTotalSum(this.BasketItems);
            this.TotalPriceString = totals.Item2.ToString("0.00") + " RON";

            this.PlaceOrderCommand = new RelayCommandWithOneParameter<DateTimeOffset>(this.ExecutePlaceOrder);
        }

        private void ExecutePlaceOrder(DateTimeOffset selectedDateOffset)
        {
            try
            {
                DateOnly selectedDate = DateOnly.FromDateTime(selectedDateOffset.Date);
                this.orderService.PlaceOrderFromBasket(selectedDate);
                this.OrderPlacedSuccessfully?.Invoke();
            }
            catch (ArgumentException exception)
            {
                this.OrderPlacementFailed?.Invoke(exception.Message);
            }
        }
    }
}