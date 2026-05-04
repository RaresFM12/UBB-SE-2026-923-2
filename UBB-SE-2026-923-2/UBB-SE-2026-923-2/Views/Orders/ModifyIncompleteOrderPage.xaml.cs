using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.ViewModels.Orders;

namespace UBB_SE_2026_923_2.Views.Orders
{
    public sealed partial class ModifyIncompleteOrderPage : Page
    {
        private OrderService orderService;
        public ModifyIncompleteOrderViewModel ViewModel { get; set; }

        public ModifyIncompleteOrderPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var extractedArgs = (Tuple<OrderService, int>)e.Parameter;

            orderService = extractedArgs.Item1;
            int orderID = extractedArgs.Item2;
            ViewModel = new (orderService, orderID);
            DataContext = ViewModel;

            base.OnNavigatedTo(e);
        }

        private void SetPickUpDate(object sender, RoutedEventArgs e)
        {
            DateTimeOffset chosenPickUpDate = new System.DateTimeOffset(
                ViewModel.PickUpDate,
                new TimeOnly(12, 0),
                new TimeSpan(12, 0, 0));
            PickUpDateSelector.SelectedDates.Add(chosenPickUpDate);
        }

        private void CheckUnselectedDate(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs e)
        {
            if (PickUpDateSelector.SelectedDates.Count == 0)
            {
                DateTimeOffset chosenPickUpDate = new System.DateTimeOffset(
                    ViewModel.PickUpDate,
                    new TimeOnly(12, 0),
                    new TimeSpan(12, 0, 0));
                PickUpDateSelector.SelectedDates.Add(chosenPickUpDate);
            }
        }

        private void CancelChanges(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UBB_SE_2026_923_2.Views.Orders.OrderHistoryPage), orderService);
        }

        private async void ModifyOrder(object sender, RoutedEventArgs e)
        {
            Dictionary<int, Tuple<int, float>> updatedQuantities = new ();

            foreach (var entry in ViewModel.OrderItems)
            {
                updatedQuantities.Add(entry.ItemID, new Tuple<int, float>(entry.ItemQuantity, entry.ItemFinalPrice));
            }

            DateOnly selectedDate = DateOnly.FromDateTime(PickUpDateSelector.SelectedDates[0].Date);

            try
            {
                orderService.ModifyIncompleteOrder(ViewModel.CurrentOrderID, updatedQuantities, selectedDate);

                ContentDialog confirmationMessage = new ContentDialog();

                confirmationMessage.XamlRoot = this.XamlRoot;
                confirmationMessage.Title = "Order#" + ViewModel.CurrentOrderID + " was successfully modified";
                confirmationMessage.CloseButtonText = "Ok";

                Frame.Navigate(typeof(UBB_SE_2026_923_2.Views.Orders.OrderHistoryPage), orderService);
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
