namespace UBB_SE_2026_923_2.ViewModels.ProductsCatalogue
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public interface ICatalogPageViewModel : INotifyPropertyChanged
    {
        void Initialize(IProductCatalogueService catalogueService, User user, IOrderService orderService);

        event EventHandler<Type> NavigateRequested;

        ICommand SearchCommand { get; }

        ICommand ApplyFiltersCommand { get; }

        ICommand NextPageCommand { get; }

        ICommand PreviousPageCommand { get; }

        ICommand AddToCartCommand { get; }

        ObservableCollection<UIItem> Products { get; }

        string PageText { get; }

        string EmptyMessage { get; }

        bool IsEmptyMessageVisible { get; }

        User CurrentUser { get; }

        IOrderService OrderService { get; }
    }
}