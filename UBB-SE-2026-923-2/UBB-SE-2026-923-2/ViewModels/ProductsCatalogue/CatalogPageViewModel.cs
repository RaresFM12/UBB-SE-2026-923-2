using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.ViewModels.ProductsCatalogue
{
    public class RelayCommand : ICommand
    {
        private readonly Action execute;

        public RelayCommand(Action execute) => this.execute = execute;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => this.execute();
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> execute;

        public RelayCommand(Action<T> execute) => this.execute = execute;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => this.execute((T)parameter);
    }

    public class UIItem : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public float Discount { get; set; }
        public int Quantity { get; set; }
        public float OldPrice { get; set; }

        public float FinalPrice => OldPrice * (1 - Discount);
        public string OldPriceDisplay => $"{OldPrice:F2} lei";
        public string FinalPriceDisplay => $"{FinalPrice:F2} lei";
        public string DiscountDisplay => HasDiscount ? $"-{Math.Round(Discount * 100, 2):G}%" : string.Empty;

        public string StockText =>
            Quantity == 0 ? "Out of stock" :
            Quantity < ProductCatalogueService.LowStockThreshold ? $"Only {Quantity} in stock" : "In stock";

        public string StockColor => Quantity == 0 ? "Red" : Quantity < ProductCatalogueService.LowStockThreshold ? "Orange" : "Green";

        public bool CanAddToCart => Quantity > 0;
        public bool HasDiscount => Discount > 0;
        public double DiscountOpacity => HasDiscount ? 1.0 : 0.0;
        public Item OriginalItem { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    public class CatalogPageViewModel : ICatalogPageViewModel
    {
        private IProductCatalogueService productService;
        public User CurrentUser { get; set; }
        public IOrderService OrderService { get; set; }

        public event EventHandler<Type> NavigateRequested;

        public ICommand SearchCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand AddToCartCommand { get; }

        private int currentPage = 0;
        private int pageSize = 10;
        private int totalItemsOnPage = 0;

        public string SearchText { get; set; } = string.Empty;

        public bool FilterPainRelief { get; set; }
        public bool FilterWellness { get; set; }
        public bool FilterColdAndFlu { get; set; }
        public bool FilterAllergy { get; set; }
        public bool FilterDigestion { get; set; }
        public bool FilterSkincare { get; set; }
        public bool FilterFirstAid { get; set; }

        public bool FilterPrice0_49 { get; set; }
        public bool FilterPrice50_99 { get; set; }
        public bool FilterPrice100_199 { get; set; }
        public bool FilterPrice200_499 { get; set; }
        public bool FilterPrice500Plus { get; set; }

        public string StockFilter { get; private set; } = null;
        private bool isStockAll = true;
        public bool IsStockAll
        {
            get => isStockAll;
            set
            {
                isStockAll = value;
                if (value)
                {
                    StockFilter = null;
                }

                OnPropertyChanged();
            }
        }
        private bool isStockIn;
        public bool IsStockIn
        {
            get => isStockIn;
            set
            {
                isStockIn = value;
                if (value)
                {
                    StockFilter = ProductCatalogueService.StockFilterInStock;
                }

                OnPropertyChanged();
            }
        }

        private bool isStockLow;

        public bool IsStockLow
        {
            get => this.isStockLow;
            set
            {
                this.isStockLow = value;

                if (value)
                {
                    this.StockFilter = ProductCatalogueService.StockFilterLowStock;
                }

                this.OnPropertyChanged();
            }
        }

        public bool? DiscountFilter { get; private set; } = null;

        private bool isDiscountAll = true;

        public bool IsDiscountAll
        {
            get => this.isDiscountAll;
            set
            {
                this.isDiscountAll = value;

                if (value)
                {
                    this.DiscountFilter = null;
                }

                this.OnPropertyChanged();
            }
        }

        private bool isDiscountYes;

        public bool IsDiscountYes
        {
            get => this.isDiscountYes;
            set
            {
                this.isDiscountYes = value;

                if (value)
                {
                    this.DiscountFilter = true;
                }

                this.OnPropertyChanged();
            }
        }

        private bool isDiscountNo;

        public bool IsDiscountNo
        {
            get => this.isDiscountNo;
            set
            {
                this.isDiscountNo = value;

                if (value)
                {
                    this.DiscountFilter = false;
                }

                this.OnPropertyChanged();
            }
        }
        public List<string> SortOptions { get; } = new List<string> { "Default", "Price", "Newest" };
        private string selectedSortOption = "Default";
        public string SelectedSortOption
        {
            get => selectedSortOption;
            set
            {
                selectedSortOption = value;
                OnPropertyChanged();
            }
        }

        public List<string> SortDirections { get; } = new List<string> { "Ascending", "Descending" };
        private string selectedSortDirection = "Ascending";
        public string SelectedSortDirection
        {
            get => selectedSortDirection;
            set
            {
                selectedSortDirection = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<UIItem> products = new ObservableCollection<UIItem>();
        public ObservableCollection<UIItem> Products
        {
            get => this.products;
            private set
            {
                this.products = value;
                this.OnPropertyChanged();
            }
        }

        private string pageText = "Page 1";

        public string PageText
        {
            get => this.pageText;
            private set
            {
                this.pageText = value;
                this.OnPropertyChanged();
            }
        }

        private string emptyMessage = string.Empty;

        public string EmptyMessage
        {
            get => this.emptyMessage;
            private set
            {
                this.emptyMessage = value;
                this.OnPropertyChanged();
            }
        }

        private bool isEmptyMessageVisible = false;

        public bool IsEmptyMessageVisible
        {
            get => this.isEmptyMessageVisible;
            private set
            {
                this.isEmptyMessageVisible = value;
                this.OnPropertyChanged();
            }
        }
        public CatalogPageViewModel()
        {
            SearchCommand = new RelayCommand(Search);
            ApplyFiltersCommand = new RelayCommand(ApplyFilters);
            NextPageCommand = new RelayCommand(NextPage);
            PreviousPageCommand = new RelayCommand(PreviousPage);
            AddToCartCommand = new RelayCommand<UIItem>(AddToCart);
        }

        public void Initialize(IProductCatalogueService service, User user, IOrderService orderService)
        {
            productService = service;
            CurrentUser = user;
            OrderService = orderService;
            LoadProducts();
        }
        private void Search()
        {
            currentPage = 0;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (productService == null)
            {
                return;
            }

            var categories = BuildCategoryList();
            var priceRanges = BuildPriceRangeList();

            string sortByParam = SelectedSortOption == "Price" ? ProductCatalogueService.SortByPrice
                               : SelectedSortOption == "Newest" ? ProductCatalogueService.SortByNewest : null;
            bool sortAscendingParam = SelectedSortDirection == "Ascending";

            var items = productService.GetItems(
                SearchText,
                categories.Any() ? categories : null,
                priceRanges.Any() ? priceRanges : null,
                StockFilter,
                DiscountFilter,
                null,
                sortAscendingParam,
                currentPage,
                pageSize,
                sortByParam);

            var uiItems = items.Select(MapToUIItem).ToList();
            totalItemsOnPage = uiItems.Count;

            Products = new ObservableCollection<UIItem>(uiItems);
            PageText = $"Page {currentPage + 1}";

            IsEmptyMessageVisible = uiItems.Count == 0;
            EmptyMessage = uiItems.Count == 0 ? (!string.IsNullOrWhiteSpace(SearchText) ? "No products found." : "No products match the selected filters.") : string.Empty;
        }

        private void NextPage()
        {
            if (productService == null)
            {
                return;
            }

            if (totalItemsOnPage == pageSize)
            {
                currentPage++;
                ApplyFilters();
            }
        }

        private void PreviousPage()
        {
            if (productService == null)
            {
                return;
            }

            if (currentPage > 0)
            {
                currentPage--;
                ApplyFilters();
            }
        }

        private void AddToCart(UIItem item)
        {
            if (item?.OriginalItem == null)
            {
                return;
            }

            if (CurrentUser == null)
            {
                NavigateRequested?.Invoke(this, typeof(Views.Accounts.LoginView));
                return;
            }
            try
            {
                OrderService.AddToBasket(item.OriginalItem.Id, 1);
            }
            catch (ArgumentException)
            {
            }
        }

        private List<string> BuildCategoryList()
        {
            var list = new List<string>();
            if (FilterPainRelief) list.Add("pain relief");
            if (FilterWellness) list.Add("wellness");
            if (FilterColdAndFlu) list.Add("cold and flu");
            if (FilterAllergy) list.Add("allergy");
            if (FilterDigestion) list.Add("digestion");
            if (FilterSkincare) list.Add("skincare");
            if (FilterFirstAid) list.Add("first aid");
            return list;
        }

        private List<(float, float)> BuildPriceRangeList()
        {
            var list = new List<(float, float)>();
            if (FilterPrice0_49)
            {
                list.Add((0f, 49.99f));
            }

            if (FilterPrice50_99)
            {
                list.Add((50f, 99.99f));
            }

            if (FilterPrice100_199)
            {
                list.Add((100f, 199.99f));
            }

            if (FilterPrice200_499)
            {
                list.Add((200f, 499.99f));
            }

            if (FilterPrice500Plus)
            {
                list.Add((500f, float.MaxValue));
            }

            return list;
        }

        private void LoadProducts()
        {
            var items = productService.GetItems(null, page: currentPage, pageSize: pageSize);
            var uiItems = items.Select(MapToUIItem).ToList();
            totalItemsOnPage = uiItems.Count;
            Products = new ObservableCollection<UIItem>(uiItems);
            PageText = $"Page {currentPage + 1}";
        }

        private static UIItem MapToUIItem(Item item)
        {
            string cleanPath = item.ImagePath?.TrimStart('/') ?? string.Empty;
            return new UIItem
            {
                Name = item.Name,
                OldPrice = item.Price,
                Discount = item.DiscountPercentage / 100,
                Quantity = item.Quantity,
                Image = cleanPath.StartsWith("ms-appx:///") ? cleanPath : $"ms-appx:///{cleanPath}",
                OriginalItem = item
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}