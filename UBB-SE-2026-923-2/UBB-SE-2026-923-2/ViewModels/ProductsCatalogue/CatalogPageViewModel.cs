namespace UBB_SE_2026_923_2.ViewModels.ProductsCatalogue
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

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

        public float FinalPrice => this.OldPrice * (1 - this.Discount);

        public string OldPriceDisplay => $"{this.OldPrice:F2} lei";

        public string FinalPriceDisplay => $"{this.FinalPrice:F2} lei";

        public string DiscountDisplay => this.HasDiscount ? $"-{Math.Round(this.Discount * 100, 2):G}%" : string.Empty;

        public string StockText =>
            this.Quantity == 0 ? "Out of stock" :
            this.Quantity < ProductCatalogueService.LowStockThreshold ? $"Only {this.Quantity} in stock" : "In stock";

        public string StockColor => this.Quantity == 0 ? "Red" : this.Quantity < ProductCatalogueService.LowStockThreshold ? "Orange" : "Green";

        public bool CanAddToCart => this.Quantity > 0;

        public bool HasDiscount => this.Discount > 0;

        public double DiscountOpacity => this.HasDiscount ? 1.0 : 0.0;

        public Item OriginalItem { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
        private readonly int pageSize = 10;
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
            get => this.isStockAll;
            set
            {
                this.isStockAll = value;
                if (value)
                {
                    this.StockFilter = null;
                }

                this.OnPropertyChanged();
            }
        }

        private bool isStockIn;

        public bool IsStockIn
        {
            get => this.isStockIn;
            set
            {
                this.isStockIn = value;
                if (value)
                {
                    this.StockFilter = ProductCatalogueService.StockFilterInStock;
                }

                this.OnPropertyChanged();
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
            get => this.selectedSortOption;
            set
            {
                this.selectedSortOption = value;
                this.OnPropertyChanged();
            }
        }

        public List<string> SortDirections { get; } = new List<string> { "Ascending", "Descending" };

        private string selectedSortDirection = "Ascending";

        public string SelectedSortDirection
        {
            get => this.selectedSortDirection;
            set
            {
                this.selectedSortDirection = value;
                this.OnPropertyChanged();
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
            this.SearchCommand = new RelayCommand(this.Search);
            this.ApplyFiltersCommand = new RelayCommand(this.ApplyFiltersFromButton);
            this.NextPageCommand = new RelayCommand(this.NextPage);
            this.PreviousPageCommand = new RelayCommand(this.PreviousPage);
            this.AddToCartCommand = new RelayCommand<UIItem>(this.AddToCart);
        }

        public void Initialize(IProductCatalogueService productService, User currentUser, IOrderService orderService)
        {
            this.productService = productService;
            this.CurrentUser = currentUser;
            this.OrderService = orderService;
            this.LoadProducts();
        }

        private void Search()
        {
            this.currentPage = 0;
            this.ApplyFilters();
        }

        private void ApplyFiltersFromButton()
        {
            this.currentPage = 0;
            this.ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (this.productService == null)
            {
                return;
            }

            var categories = this.BuildCategoryList();
            var priceRanges = this.BuildPriceRangeList();

            string sortByParam = this.SelectedSortOption == "Price" ? ProductCatalogueService.SortByPrice
                               : this.SelectedSortOption == "Newest" ? ProductCatalogueService.SortByNewest : null;
            bool sortAscendingParam = this.SelectedSortDirection == "Ascending";

            var items = this.productService.GetItems(
                this.SearchText,
                categories.Any() ? categories : null,
                priceRanges.Any() ? priceRanges : null,
                this.StockFilter,
                this.DiscountFilter,
                null,
                sortAscendingParam,
                this.currentPage,
                this.pageSize,
                sortByParam);

            var uiItems = items.Select(MapToUIItem).ToList();
            this.totalItemsOnPage = uiItems.Count;

            this.Products = new ObservableCollection<UIItem>(uiItems);
            this.PageText = $"Page {this.currentPage + 1}";

            this.IsEmptyMessageVisible = uiItems.Count == 0;
            this.EmptyMessage = uiItems.Count == 0 ? (!string.IsNullOrWhiteSpace(this.SearchText) ? "No products found." : "No products match the selected filters.") : string.Empty;
        }

        private void NextPage()
        {
            if (this.productService == null)
            {
                return;
            }

            if (this.totalItemsOnPage == this.pageSize)
            {
                this.currentPage++;
                this.ApplyFilters();
            }
        }

        private void PreviousPage()
        {
            if (this.productService == null)
            {
                return;
            }

            if (this.currentPage > 0)
            {
                this.currentPage--;
                this.ApplyFilters();
            }
        }

        private void AddToCart(UIItem item)
        {
            if (item?.OriginalItem == null)
            {
                return;
            }

            if (this.CurrentUser == null)
            {
                this.NavigateRequested?.Invoke(this, typeof(Views.Accounts.LoginView));
                return;
            }

            try
            {
                this.OrderService.AddToBasket(item.OriginalItem.Id, 1);
            }
            catch (ArgumentException)
            {
            }
        }

        private List<string> BuildCategoryList()
        {
            var list = new List<string>();
            if (this.FilterPainRelief)
            {
                list.Add("pain relief");
            }

            if (this.FilterWellness)
            {
                list.Add("wellness");
            }

            if (this.FilterColdAndFlu)
            {
                list.Add("cold and flu");
            }

            if (this.FilterAllergy)
            {
                list.Add("allergy");
            }

            if (this.FilterDigestion)
            {
                list.Add("digestion");
            }

            if (this.FilterSkincare)
            {
                list.Add("skincare");
            }

            if (this.FilterFirstAid)
            {
                list.Add("first aid");
            }

            return list;
        }

        private List<(float, float)> BuildPriceRangeList()
        {
            var list = new List<(float, float)>();
            if (this.FilterPrice0_49)
            {
                list.Add((0f, 49.99f));
            }

            if (this.FilterPrice50_99)
            {
                list.Add((50f, 99.99f));
            }

            if (this.FilterPrice100_199)
            {
                list.Add((100f, 199.99f));
            }

            if (this.FilterPrice200_499)
            {
                list.Add((200f, 499.99f));
            }

            if (this.FilterPrice500Plus)
            {
                list.Add((500f, float.MaxValue));
            }

            return list;
        }

        private void LoadProducts()
        {
            var items = this.productService.GetItems(null, page: this.currentPage, pageSize: this.pageSize);
            var uiItems = items.Select(MapToUIItem).ToList();
            this.totalItemsOnPage = uiItems.Count;
            this.Products = new ObservableCollection<UIItem>(uiItems);
            this.PageText = $"Page {this.currentPage + 1}";
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
                OriginalItem = item,
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}