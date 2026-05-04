using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using UBB_SE_2026_923_2.Repositories;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.ViewModels.Orders;

namespace UBB_SE_2026_923_2.Views.Orders
{
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

            currentOrderService = e.Parameter as IOrderService ?? new OrderService();
            viewModel = new CheckoutViewModel(currentOrderService);

            viewModel.OrderPlacedSuccessfully += OnOrderSuccess;
            viewModel.OrderPlacementFailed += OnOrderFailure;

            DataContext = viewModel;
            Bindings?.Update();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (viewModel != null)
            {
                viewModel.OrderPlacedSuccessfully -= OnOrderSuccess;
                viewModel.OrderPlacementFailed -= OnOrderFailure;
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
                DateTimeOffset selectedDate = PickUpDateSelector.SelectedDates[0];

                if (viewModel.PlaceOrderCommand.CanExecute(selectedDate))
                {
                    viewModel.PlaceOrderCommand.Execute(selectedDate);
                }
            }
        }

        private async void OnOrderSuccess()
        {
            ContentDialog confirmationMessage = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = "Your order was placed",
                CloseButtonText = "Ok"
            };

            Frame.Navigate(
                typeof(ProductsCatalogue.HomePage),
                new ProductCatalogueService(new SQLItemsRepository()));

            await confirmationMessage.ShowAsync();
        }

        private async void OnOrderFailure(string errorMessage)
        {
            ContentDialog causeOfErrorDialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = "Error",
                Content = errorMessage,
                CloseButtonText = "Ok"
            };

            await causeOfErrorDialog.ShowAsync();
        }

        private void NavigateToBasket(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BasketPage), currentOrderService);
        }
    }
}