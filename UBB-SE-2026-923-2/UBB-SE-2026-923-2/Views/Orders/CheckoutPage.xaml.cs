namespace UBB_SE_2026_923_2.Views.Orders
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Orders;

    public sealed partial class CheckoutPage : Page
    {
        private CheckoutViewModel viewModel;
        private IOrderService currentOrderService;

        public CheckoutPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.currentOrderService = e.Parameter as IOrderService ?? new OrderService();
            this.viewModel = new CheckoutViewModel(this.currentOrderService);

            this.viewModel.OrderPlacedSuccessfully += this.OnOrderSuccess;
            this.viewModel.OrderPlacementFailed += this.OnOrderFailure;

            this.DataContext = this.viewModel;
            Bindings?.Update();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (this.viewModel != null)
            {
                this.viewModel.OrderPlacedSuccessfully -= this.OnOrderSuccess;
                this.viewModel.OrderPlacementFailed -= this.OnOrderFailure;
            }
        }

        private void SetDefaultPickUpDate(object sender, RoutedEventArgs e)
        {
            PickUpDateSelector.MinDate = new DateTimeOffset(DateTime.Now.Date.AddDays(1));

            if (PickUpDateSelector.SelectedDates.Count == 0)
            {
                PickUpDateSelector.SelectedDates.Add(PickUpDateSelector.MinDate);
            }
        }

        private void CheckUnselectedDate(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs e)
        {
            if (PickUpDateSelector.SelectedDates.Count == 0)
            {
                PickUpDateSelector.SelectedDates.Add(PickUpDateSelector.MinDate);
            }
        }

        private void PlaceOrder(object sender, RoutedEventArgs e)
        {
            if (PickUpDateSelector.SelectedDates.Count > 0)
            {
                DateTimeOffset selectedDate = PickUpDateSelector.SelectedDates[PickUpDateSelector.SelectedDates.Count - 1];

                if (this.viewModel.PlaceOrderCommand.CanExecute(selectedDate))
                {
                    this.viewModel.PlaceOrderCommand.Execute(selectedDate);
                }
            }
        }

        private async void OnOrderSuccess()
        {
            ContentDialog confirmationMessage = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Your order was placed",
                CloseButtonText = "Ok",
            };

            await confirmationMessage.ShowAsync();

            this.Frame.Navigate(
                typeof(OrderHistoryPage),
                this.currentOrderService);
        }

        private async void OnOrderFailure(string errorMessage)
        {
            ContentDialog causeOfErrorDialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Error",
                Content = errorMessage,
                CloseButtonText = "Ok",
            };

            await causeOfErrorDialog.ShowAsync();
        }

        private void NavigateToBasket(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BasketPage), this.currentOrderService);
        }
    }
}