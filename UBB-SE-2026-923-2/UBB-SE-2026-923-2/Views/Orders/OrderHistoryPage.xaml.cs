namespace UBB_SE_2026_923_2.Views.Orders
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Orders;

    public sealed partial class OrderHistoryPage : Page
    {
        private IOrderService currentOrderService = null!;

        public OrderHistoryViewModel ViewModel { get; private set; } = null!;

        public OrderHistoryPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is not IOrderService service)
            {
                return;
            }

            this.currentOrderService = service;
            this.ViewModel = new OrderHistoryViewModel(this.currentOrderService);
            this.DataContext = this.ViewModel;

            this.ViewModel.RedirectToDetailRequested += this.RedirectToDetailPage;
            this.ViewModel.RedirectToModifyRequested += this.RedirectToModifyPage;
            this.ViewModel.CancelConfirmationRequested += this.AskCancelOrderConfirmation;
            this.ViewModel.RedirectToResubmitRequested += this.RedirectToResubmitPage;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (this.ViewModel != null)
            {
                this.ViewModel.RedirectToDetailRequested -= this.RedirectToDetailPage;
                this.ViewModel.RedirectToModifyRequested -= this.RedirectToModifyPage;
                this.ViewModel.CancelConfirmationRequested -= this.AskCancelOrderConfirmation;
                this.ViewModel.RedirectToResubmitRequested -= this.RedirectToResubmitPage;
            }
        }

        private void RedirectToDetailPage(int orderId)
        {
            this.Frame.Navigate(
                typeof(UBB_SE_2026_923_2.Views.Orders.NonEditableOrderDetailPage),
                        new Tuple<IOrderService, int>(this.currentOrderService, orderId));
        }

        private void RedirectToModifyPage(int orderId)
        {
            this.Frame.Navigate(
                typeof(UBB_SE_2026_923_2.Views.Orders.ModifyIncompleteOrderPage),
                        new Tuple<IOrderService, int>(this.currentOrderService, orderId));
        }

        private async void AskCancelOrderConfirmation(Order currOrder)
        {
            ContentDialog dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Cancel?",
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No",
                DefaultButton = ContentDialogButton.None,
                Content = $"Do you want to cancel Order#{currOrder.Id}?",
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                this.ViewModel.CancelOrder(currOrder);
            }
        }

        private void RedirectToResubmitPage(int orderId)
        {
            this.Frame.Navigate(
                typeof(UBB_SE_2026_923_2.Views.Orders.ResubmitOrderPage),
                        new Tuple<IOrderService, int>(this.currentOrderService, orderId));
        }
    }

    public partial class OrderTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CompletedTemplate { get; set; } = null!;

        public DataTemplate IncompletedTemplate { get; set; } = null!;

        public DataTemplate ExpiredTemplate { get; set; } = null!;

        protected override DataTemplate SelectTemplateCore(object item)
        {
            Order currentOrder = (Order)item;

            if (currentOrder.IsCompleted)
            {
                return this.CompletedTemplate;
            }

            if (currentOrder.IsExpired)
            {
                return this.ExpiredTemplate;
            }

            return this.IncompletedTemplate;
        }
    }
}
