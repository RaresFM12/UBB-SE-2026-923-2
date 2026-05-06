using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.ViewModels.Orders;

namespace UBB_SE_2026_923_2.Views.Orders
{
    public sealed partial class NonEditableOrderDetailPage : Page
    {
        private IOrderService orderService;
        private NonEditDetailViewModel ViewModel { get; set; }

        public NonEditableOrderDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var extractedArgs = (Tuple<IOrderService, int>)e.Parameter;

            orderService = extractedArgs.Item1;
            int orderID = extractedArgs.Item2;
            ViewModel = new (orderService, orderID);
            DataContext = ViewModel;

            base.OnNavigatedTo(e);
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}
