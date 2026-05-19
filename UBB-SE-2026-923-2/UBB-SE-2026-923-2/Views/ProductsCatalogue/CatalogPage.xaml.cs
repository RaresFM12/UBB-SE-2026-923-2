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

    public sealed partial class CatalogPage : Page
    {
        public ICatalogPageViewModel ViewModel { get; }

        public CatalogPage()
        {
            InitializeComponent();

            this.ViewModel = new CatalogPageViewModel();

            this.DataContext = this.ViewModel;
            this.ViewModel.NavigateRequested += this.OnViewModelNavigateRequested;
        }

        private void OnViewModelNavigateRequested(object sender, Type pageType)
        {
            this.Frame.Navigate(pageType);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ValueTuple<ProductCatalogueService, User, IOrderService> tuple)
            {
                this.ViewModel.Initialize(tuple.Item1, tuple.Item2, tuple.Item3);
            }
            else if (e.Parameter is ValueTuple<IProductCatalogueService, User, IOrderService> interfaceTuple)
            {
                this.ViewModel.Initialize(interfaceTuple.Item1, interfaceTuple.Item2, interfaceTuple.Item3);
            }
        }

        private void OnProductClicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var uiItem = button?.DataContext as UIItem;
            if (uiItem?.OriginalItem == null)
            {
                return;
            }

            this.Frame.Navigate(
                typeof(ProductDetailsPage),
                (uiItem.OriginalItem, this.ViewModel.CurrentUser, this.ViewModel.OrderService));
        }

        private void OnProductImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (sender is Image image)
            {
                image.Source = new BitmapImage(new Uri("ms-appx:///Assets/StoreLogo.png"));
            }
        }
    }
}
