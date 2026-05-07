namespace UBB_SE_2026_923_2.Views.Orders
{
    using System;
    using System.Collections.Generic;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Orders;

    public sealed partial class EditableOrderDetailPage : Page
    {
        private IOrderService orderService;

        public EditDetailViewModel ViewModel { get; set; }

        public EditableOrderDetailPage()
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

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private async void CompleteOrder(object sender, RoutedEventArgs e)
        {
            int orderID = this.ViewModel.ShownOrderID;
            Dictionary<int, Tuple<int, float>> updatedQuantities = new();
            foreach (var entry in this.ViewModel.OrderItems)
            {
                updatedQuantities.Add(entry.ItemID, new Tuple<int, float>(entry.ItemQuantity, entry.ItemFinalPrice));
            }

            try
            {
                this.orderService.CompleteOrder(orderID, updatedQuantities);

                ContentDialog confirmationMessage = new ContentDialog();

                confirmationMessage.XamlRoot = this.XamlRoot;
                confirmationMessage.Title = $"Order#{orderID} was completed";
                confirmationMessage.CloseButtonText = "Ok";

                this.Frame.Navigate(typeof(UBB_SE_2026_923_2.Views.Orders.OrderManagementPage), this.orderService);
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
