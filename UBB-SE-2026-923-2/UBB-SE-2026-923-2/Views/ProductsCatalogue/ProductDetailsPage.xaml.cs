using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.ViewModels.ProductsCatalogue;
using UBB_SE_2026_923_2.Views.Accounts;


namespace UBB_SE_2026_923_2.Views.ProductsCatalogue
{
    public sealed partial class ProductDetailsPage : Page
    {
        public IProductDetailsPageViewModel ViewModel { get; }

        public ProductDetailsPage()
        {
            InitializeComponent();
            ViewModel = new ProductDetailsPageViewModel();
            DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ValueTuple<Item, User, IOrderService> tuple)
            {
                ViewModel.Initialize(tuple.Item1, tuple.Item2, tuple.Item3);
                LoadProductImage(tuple.Item1.ImagePath);
            }
        }

        private void LoadProductImage(string imagePath)
        {
            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                string cleanPath = imagePath.TrimStart('/');

                string fullPath = cleanPath.StartsWith("ms-appx:///")
                    ? cleanPath
                    : $"ms-appx:///{cleanPath}";

                ProductImage.Source = new BitmapImage(new Uri(fullPath));
            }
        }

        private void OnAddToBasket(object sender, RoutedEventArgs e)
        {
            var (success, navigateToLogin) = ViewModel.TryAddToBasket(QuantityBox.Text);

            if (navigateToLogin)
            {
                Frame.Navigate(typeof(LoginView));
            }
        }

        private void OnToggleStockAlert(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.ToggleStockAlert();
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