namespace UBB_SE_2026_923_2.Views.Orders
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Orders;

    public sealed partial class ResubmitOrderPage : Page
    {
        private IOrderService orderService;
        private ResubmitOrderViewModel viewModel;

        public ResubmitOrderPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var extractedArgs = (Tuple<IOrderService, int>)e.Parameter;

            this.orderService = extractedArgs.Item1;
            int orderID = extractedArgs.Item2;
            this.viewModel = new(this.orderService, orderID);
            this.DataContext = this.viewModel;

            base.OnNavigatedTo(e);
        }

        private void SetDefaultPickUpDate(object sender, RoutedEventArgs e)
        {
            PickUpDateSelector.MinDate = new System.DateTimeOffset(DateTime.Now.Date.AddDays(1));
            PickUpDateSelector.SelectedDates.Add(PickUpDateSelector.MinDate);
        }

        private void CheckUnselectedDate(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs e)
        {
            if (PickUpDateSelector.SelectedDates.Count == 0)
            {
                PickUpDateSelector.SelectedDates.Add(PickUpDateSelector.MinDate);
            }
        }

        private async void ResubmitOrder(object sender, RoutedEventArgs e)
        {
            DateOnly selectedDate = DateOnly.FromDateTime(PickUpDateSelector.SelectedDates[0].Date);
            int orderIDToResubmit = this.viewModel.ShownOrderID;

            try
            {
                this.orderService.ResubmitExpiredOrder(orderIDToResubmit, selectedDate);

                ContentDialog confirmationMessage = new ContentDialog();

                confirmationMessage.XamlRoot = this.XamlRoot;
                confirmationMessage.Title = "Success";
                confirmationMessage.Content = "A new order has been created identical to the previously selected expired order";
                confirmationMessage.CloseButtonText = "Ok";

                this.Frame.Navigate(typeof(UBB_SE_2026_923_2.Views.Orders.OrderHistoryPage), this.orderService);
                var result = await confirmationMessage.ShowAsync();
            }
            catch (ArgumentException exception)
            {
                ContentDialog causeOfErrorDialog = new ContentDialog();

                causeOfErrorDialog.XamlRoot = this.XamlRoot;
                causeOfErrorDialog.Title = "Error";
                causeOfErrorDialog.Content = exception.Message;
                causeOfErrorDialog.CloseButtonText = "Ok";

                var result = await causeOfErrorDialog.ShowAsync();
            }
        }

        private void NavigateToOrderHistory(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(OrderHistoryPage), this.orderService);
        }
    }
}
