using System;
using System.Collections.Generic;
using System.Linq;

namespace UBB_SE_2026_923_2.Web.ViewModels
{
    public class CatalogueIndexViewModel
    {
        public List<CatalogueItemViewModel> Products { get; set; } = new List<CatalogueItemViewModel>();

        public string SearchText { get; set; }
        public int CurrentPage { get; set; } = 0;
        public int PageSize { get; set; } = 12;
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage => CurrentPage > 0;

        public List<string> SelectedCategories { get; set; } = new List<string>();
        public List<string> SelectedPriceRanges { get; set; } = new List<string>();
        public string StockFilter { get; set; } = "all";
        public string DiscountFilter { get; set; } = "all";

        public string SortBy { get; set; } = "default";
        public string SortDirection { get; set; } = "asc";
    }

    public class CatalogueItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Producer { get; set; }
        public string Category { get; set; }
        public string Label { get; set; }
        public int NumberOfPills { get; set; }

        public string SubstancesText { get; set; }

        public float OldPrice { get; set; }
        public float DiscountPercentage { get; set; }
        public float FinalPrice => OldPrice * (1 - (DiscountPercentage / 100));
        public int Quantity { get; set; }
        public string ImagePath { get; set; }
        public string Description { get; set; }

        public bool HasDiscount => DiscountPercentage > 0;

        public string StockText => Quantity == 0 ? "Out of stock" :
                                   Quantity < 10 ? $"Only {Quantity} in stock" : "In stock";

        public string StockColorClass => Quantity == 0 ? "text-danger" :
                                         Quantity < 10 ? "text-warning" : "text-success";

        public bool CanAddToCart => Quantity > 0;

        public bool ShowStockAlertButton { get; set; }

        public bool IsStockAlertActive { get; set; }

        public string StockAlertButtonText => this.IsStockAlertActive
            ? "Unsubscribe from stock alert"
            : "Notify when in stock";
    }
}