using System;
using System.Collections.Generic;
using System.Windows.Input;
using UBB_SE_2026_923_2.Command;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.ViewModels.Orders
{
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
            orderService = injectedOrderService;
            BasketItems = orderService.GetBasketItems();

            Tuple<float, float> totals = orderService.CalculateBasketTotalSum(BasketItems);
            TotalPriceString = totals.Item2.ToString("0.00") + " RON";

            PlaceOrderCommand = new RelayCommandWithOneParameter<DateTimeOffset>(ExecutePlaceOrder);
        }

        private void ExecutePlaceOrder(DateTimeOffset selectedDateOffset)
        {
            try
            {
                DateOnly selectedDate = DateOnly.FromDateTime(selectedDateOffset.Date);
                orderService.PlaceOrderFromBasket(selectedDate);
                OrderPlacedSuccessfully?.Invoke();
            }
            catch (ArgumentException exception)
            {
                OrderPlacementFailed?.Invoke(exception.Message);
            }
        }
    }
}