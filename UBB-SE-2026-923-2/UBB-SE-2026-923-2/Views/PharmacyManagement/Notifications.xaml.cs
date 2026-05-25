namespace UBB_SE_2026_923_2.Views.PharmacyManagement
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.PharmacyManagement;
    using UBB_SE_2026_923_2.Views.ProductsCatalogue;

    public sealed partial class Notifications : Page
    {
        private NotificationsViewModel ViewModel { get; } = new NotificationsViewModel(new AdminService());

        private object? productCatalogueParameter;

        public Notifications()
        {
            this.ViewModel.PopulateNotifications();

            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ValueTuple<IProductCatalogueService, User, IOrderService> ||
                e.Parameter is ValueTuple<ProductCatalogueService, User, IOrderService>)
            {
                this.productCatalogueParameter = e.Parameter;
            }
        }

        private void OnNotificationButtonClicked(object sender, RoutedEventArgs e)
        {
            string buttonContent = (string)((Button)sender).Content;
            if (buttonContent == "Go to products")
            {
                this.Frame.Navigate(typeof(CatalogPage), this.productCatalogueParameter);
            }

            if (buttonContent == "Go fix it")
            {
                this.Frame.Navigate(typeof(EditPage));
            }
        }
    }
}
