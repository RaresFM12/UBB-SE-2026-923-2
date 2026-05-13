namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;

    public class ProductCatalogueService : IProductCatalogueService
    {
        public const string StockFilterInStock = "in_stock";
        public const string StockFilterLowStock = "low_stock";
        public const string SortByPrice = "price";
        public const string SortByNewest = "newest";
        public const int LowStockThreshold = 10;
        public const int DefaultPageSize = 10;

        private readonly IItemsRepository itemsRepository;

        public ProductCatalogueService(IItemsRepository itemsRepository)
        {
            this.itemsRepository = itemsRepository;
        }

        public List<Item> GetItems(
            string search,
            List<string> categories = null,
            List<(float minimum, float maximum)> priceRanges = null,
            string stockFilter = null,
            bool? discounted = null,
            List<string> substances = null,
            bool ascending = true,
            int page = 0,
            int pageSize = DefaultPageSize,
            string sortBy = null)
        {
            var items = this.SearchItems(search);
            items = this.FilterByCategory(items, categories);
            items = this.FilterByPrice(items, priceRanges);
            items = this.FilterByStock(items, stockFilter);
            items = this.FilterByDiscount(items, discounted);
            items = this.FilterBySubstance(items, substances);
            items = this.SortItems(items, sortBy, ascending);
            items = this.Paginate(items, page, pageSize);
            return items;
        }

        private List<Item> SearchItems(string productName)
        {
            var items = this.itemsRepository.GetAllItems();

            if (string.IsNullOrWhiteSpace(productName))
            {
                return items;
            }

            return items
                .Where(item => item.Name != null &&
                               item.Name.Contains(productName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private List<Item> FilterByCategory(List<Item> items, List<string> categories)
        {
            if (categories == null || !categories.Any())
            {
                return items;
            }

            return items.Where(item => categories.Contains(item.Category)).ToList();
        }

        private List<Item> FilterByPrice(List<Item> items, List<(float minimum, float maximum)> priceRanges)
        {
            if (priceRanges == null || !priceRanges.Any())
            {
                return items;
            }

            foreach (var (minimumPrice, maximumPrice) in priceRanges)
            {
                if (minimumPrice < 0 || maximumPrice < 0 || minimumPrice > maximumPrice)
                {
                    throw new ArgumentException($"{nameof(minimumPrice)} and {nameof(maximumPrice)} are not valid for a price filter");
                }
            }

            return items.Where(item =>
            {
                float finalPrice = item.Price * (1 - item.DiscountPercentage);
                return priceRanges.Any(range => finalPrice >= range.minimum && finalPrice <= range.maximum);
            }).ToList();
        }

        private List<Item> FilterByStock(List<Item> items, string stockFilter)
        {
            if (stockFilter == null)
            {
                return items;
            }

            if (stockFilter == StockFilterInStock)
            {
                return items.Where(item => item.Quantity > 0).ToList();
            }

            if (stockFilter == StockFilterLowStock)
            {
                return items.Where(item => item.Quantity > 0 && item.Quantity < LowStockThreshold).ToList();
            }

            return items;
        }

        private List<Item> FilterByDiscount(List<Item> items, bool? discounted)
        {
            if (!discounted.HasValue)
            {
                return items;
            }

            if (discounted == true)
            {
                return items.Where(item => item.DiscountPercentage > 0).ToList();
            }

            return items.Where(item => item.DiscountPercentage == 0).ToList();
        }

        private List<Item> FilterBySubstance(List<Item> items, List<string> substances)
        {
            if (substances == null || !substances.Any())
            {
                return items;
            }

            return items.Where(item =>
                substances.All(substance => item.ActiveSubstances.ContainsKey(substance))).ToList();
        }

        private List<Item> SortItems(List<Item> items, string sortBy, bool ascending)
        {
            if (sortBy == SortByPrice)
            {
                return this.SortByPriceValue(items, ascending);
            }

            if (sortBy == SortByNewest)
            {
                return this.SortByNewestDate(items, ascending);
            }

            return items;
        }

        private List<Item> SortByPriceValue(List<Item> items, bool ascending)
        {
            if (ascending)
            {
                return items.OrderBy(item => item.Price).ToList();
            }

            return items.OrderByDescending(item => item.Price).ToList();
        }

        private List<Item> SortByNewestDate(List<Item> items, bool ascending)
        {
            if (ascending)
            {
                return items.OrderBy(item => this.GetLatestValidDate(item) ?? DateOnly.MinValue).ToList();
            }

            return items.OrderByDescending(item => this.GetLatestValidDate(item) ?? DateOnly.MinValue).ToList();
        }

        private DateOnly? GetLatestValidDate(Item item)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            var validDates = item.Batches.Keys.Where(date => date > today);
            return validDates.Any() ? validDates.Max() : null;
        }

        private List<Item> Paginate(List<Item> items, int page, int pageSize)
        {
            return items.Skip(page * pageSize).Take(pageSize).ToList();
        }
    }
}