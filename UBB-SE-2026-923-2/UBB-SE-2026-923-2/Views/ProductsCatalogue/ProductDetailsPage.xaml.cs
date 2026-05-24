namespace UBB_SE_2026_923_2.Views.ProductsCatalogue
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media.Imaging;
    using Microsoft.UI.Xaml.Navigation;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.ProductsCatalogue;
    using UBB_SE_2026_923_2.Views.Accounts;

    public sealed partial class ProductDetailsPage : Page
    {
        public IProductDetailsPageViewModel ViewModel { get; }

        public ProductDetailsPage()
        {
            InitializeComponent();
            this.ViewModel = new ProductDetailsPageViewModel();
            this.DataContext = this.ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ValueTuple<Item, User, IOrderService> tuple)
            {
                this.ViewModel.Initialize(tuple.Item1, tuple.Item2, tuple.Item3);
                this.LoadProductImage(tuple.Item1.ImagePath);
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

        private void OnProductImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ProductImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/StoreLogo.png"));
        }

        private void OnAddToBasket(object sender, RoutedEventArgs e)
        {
            var (success, navigateToLogin) = this.ViewModel.TryAddToBasket(QuantityBox.Text);

            if (navigateToLogin)
            {
                this.Frame.Navigate(typeof(LoginView));
            }
        }

        private void OnToggleStockAlert(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            this.ViewModel.ToggleStockAlert();
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    }
}
