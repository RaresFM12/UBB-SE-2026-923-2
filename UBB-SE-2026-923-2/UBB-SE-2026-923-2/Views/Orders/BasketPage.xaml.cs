using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.ViewModels.Orders;

namespace UBB_SE_2026_923_2.Views.Orders
{
    public sealed partial class BasketPage : Page
    {
        private OrderService basketServ;

        public BasketViewModel ViewModel { get; private set; }

        public BasketPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            basketServ = e.Parameter as OrderService ?? new OrderService();
            ViewModel = new BasketViewModel(basketServ);
            DataContext = ViewModel;

            ViewModel.BasketQuantityRemoved -= HandleCheckoutButton;
            ViewModel.BasketQuantityRemoved += HandleCheckoutButton;

            Bindings?.Update();
            ViewModel.OnBasketQuantityRemoved();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.BasketQuantityRemoved -= HandleCheckoutButton;
            }

            base.OnNavigatedFrom(e);
        }

        private void NavigateToCheckout(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CheckoutPage), basketServ);
        }

        private void HandleCheckoutButton(int quantity)
        {
            CheckoutButton.Visibility = quantity > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void EnterPrescriptionID(object sender, RoutedEventArgs e)
        {
            PrescriptionWarning.Visibility = Visibility.Collapsed;

            string prescriptionId = PrescriptionInputBox.Text;

            try
            {
                ViewModel.GetPrescription(prescriptionId);
                Bindings?.Update();
            }
            catch (ArgumentException exception)
            {
                PrescriptionWarning.Text = exception.Message;
                PrescriptionWarning.Visibility = Visibility.Visible;
            }
        }
    }
}