namespace UBB_SE_2026_923_2.Views.Orders
{
    using System;
    using System.Collections.Generic;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Orders;

    public sealed partial class ModifyIncompleteOrderPage : Page
    {
        private IOrderService orderService;

        public ModifyIncompleteOrderViewModel ViewModel { get; set; }

        public ModifyIncompleteOrderPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var extractedArgs = (Tuple<IOrderService, int>)e.Parameter;

            this.orderService = extractedArgs.Item1;
            int orderID = extractedArgs.Item2;
            this.ViewModel = new(this.orderService, orderID);
            this.DataContext = this.ViewModel;

            base.OnNavigatedTo(e);
        }

        private void SetPickUpDate(object sender, RoutedEventArgs e)
        {
            PickUpDateSelector.MinDate = new DateTimeOffset(DateTime.Now.Date.AddDays(1));
            DateTimeOffset chosenPickUpDate = new DateTimeOffset(
                this.ViewModel.PickUpDate.ToDateTime(new TimeOnly(12, 0)),
                new TimeSpan(0, 0, 0));
            PickUpDateSelector.SelectedDates.Add(chosenPickUpDate);
        }

        private void CheckUnselectedDate(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs e)
        {
            if (PickUpDateSelector.SelectedDates.Count == 0)
            {
                PickUpDateSelector.SelectedDates.Add(PickUpDateSelector.MinDate);
            }
        }

        private void CancelChanges(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UBB_SE_2026_923_2.Views.Orders.OrderHistoryPage), this.orderService);
        }

        private async void ModifyOrder(object sender, RoutedEventArgs e)
        {
            Dictionary<int, Tuple<int, float>> updatedQuantities = new();

            foreach (var entry in this.ViewModel.OrderItems)
            {
                if (entry.ItemQuantity > 0)
                {
                    updatedQuantities.Add(entry.ItemID, new Tuple<int, float>(entry.ItemQuantity, entry.ItemFinalPrice));
                }
            }

            DateOnly selectedDate = DateOnly.FromDateTime(PickUpDateSelector.SelectedDates[PickUpDateSelector.SelectedDates.Count - 1].Date);

            try
            {
                if (updatedQuantities.Count == 0)
                {
                    throw new ArgumentException("The order must contain at least one item.");
                }

                this.orderService.ModifyIncompleteOrder(this.ViewModel.CurrentOrderID, updatedQuantities, selectedDate);

                ContentDialog confirmationMessage = new ContentDialog();

                confirmationMessage.XamlRoot = this.XamlRoot;
                confirmationMessage.Title = "Order#" + this.ViewModel.CurrentOrderID + " was successfully modified";
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
    }
}
