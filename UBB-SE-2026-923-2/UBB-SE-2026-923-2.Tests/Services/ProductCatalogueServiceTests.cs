namespace UBB_SE_2026_923_2.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using Moq;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class ProductCatalogueServiceLogicTests
    {
        private Mock<IItemsRepository> mockItemsRepository;
        private ProductCatalogueService productCatalogueService;

        [SetUp]
        public void Setup()
        {
            this.mockItemsRepository = new Mock<IItemsRepository>();
            this.productCatalogueService = new ProductCatalogueService(this.mockItemsRepository.Object);
        }

        [Test]
        public void GetItems_WhenSearchTextHasDifferentCasing_ReturnsMatchingItems()
        {
            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item>
                {
                    CreateItem(1, "Paracetamol", "Painkillers", 10, 5, 0),
                    CreateItem(2, "Ibuprofen", "Painkillers", 15, 5, 0),
                });

            var matchingItems = this.productCatalogueService.GetItems("PARA");

            Assert.That(matchingItems.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetItems_WhenCategoryFilterIsUsed_ReturnsOnlyItemsFromSelectedCategory()
        {
            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item>
                {
                    CreateItem(1, "Paracetamol", "Painkillers", 10, 5, 0),
                    CreateItem(2, "Vitamin C", "Vitamins", 15, 5, 0),
                });

            var matchingItems = this.productCatalogueService.GetItems(
                string.Empty,
                categories: new List<string> { "Vitamins" });

            Assert.That(matchingItems[0].Category, Is.EqualTo("Vitamins"));
        }

        [Test]
        public void GetItems_WhenPriceRangeIsInvalid_ThrowsArgumentException()
        {
            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item>());

            Assert.Throws<ArgumentException>(
                () => this.productCatalogueService.GetItems(
                    string.Empty,
                    priceRanges: new List<(float minimum, float maximum)> { (20, 10) }));
        }

        [Test]
        public void GetItems_WhenLowStockFilterIsUsed_ReturnsOnlyLowStockItems()
        {
            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item>
                {
                    CreateItem(1, "Low stock item", "Category", 10, 5, 0),
                    CreateItem(2, "Out of stock item", "Category", 15, 0, 0),
                    CreateItem(3, "Enough stock item", "Category", 20, 20, 0),
                });

            var matchingItems = this.productCatalogueService.GetItems(
                string.Empty,
                stockFilter: ProductCatalogueService.StockFilterLowStock);

            Assert.That(matchingItems.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetItems_WhenDiscountedFilterIsTrue_ReturnsOnlyDiscountedItems()
        {
            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item>
                {
                    CreateItem(1, "Discounted item", "Category", 10, 5, 0.2f),
                    CreateItem(2, "Full price item", "Category", 15, 5, 0),
                });

            var matchingItems = this.productCatalogueService.GetItems(
                string.Empty,
                discounted: true);

            Assert.That(matchingItems[0].Id, Is.EqualTo(1));
        }

        [Test]
        public void GetItems_WhenSubstanceFilterIsUsed_ReturnsOnlyItemsContainingAllRequestedSubstances()
        {
            var itemWithBothSubstances = CreateItem(1, "Complex medicine", "Category", 10, 5, 0);
            itemWithBothSubstances.ActiveSubstances = new Dictionary<string, float>
            {
                { "Substance A", 1 },
                { "Substance B", 2 },
            };

            var itemWithOneSubstance = CreateItem(2, "Simple medicine", "Category", 15, 5, 0);
            itemWithOneSubstance.ActiveSubstances = new Dictionary<string, float>
            {
                { "Substance A", 1 },
            };

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item> { itemWithBothSubstances, itemWithOneSubstance });

            var matchingItems = this.productCatalogueService.GetItems(
                string.Empty,
                substances: new List<string> { "Substance A", "Substance B" });

            Assert.That(matchingItems.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetItems_WhenSortingByPriceDescending_ReturnsMostExpensiveItemFirst()
        {
            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item>
                {
                    CreateItem(1, "Cheap item", "Category", 10, 5, 0),
                    CreateItem(2, "Expensive item", "Category", 50, 5, 0),
                });

            var sortedItems = this.productCatalogueService.GetItems(
                string.Empty,
                ascending: false,
                sortBy: ProductCatalogueService.SortByPrice);

            Assert.That(sortedItems[0].Id, Is.EqualTo(2));
        }

        [Test]
        public void GetItems_WhenPaginationIsUsed_ReturnsRequestedPageItems()
        {
            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item>
                {
                    CreateItem(1, "First item", "Category", 10, 5, 0),
                    CreateItem(2, "Second item", "Category", 15, 5, 0),
                    CreateItem(3, "Third item", "Category", 20, 5, 0),
                });

            var paginatedItems = this.productCatalogueService.GetItems(
                string.Empty,
                page: 1,
                pageSize: 2);

            Assert.That(paginatedItems[0].Id, Is.EqualTo(3));
        }

        private static Item CreateItem(
            int itemIdentifier,
            string itemName,
            string category,
            float price,
            int quantity,
            float discountPercentage)
        {
            return new Item
            {
                Id = itemIdentifier,
                Name = itemName,
                Producer = "Producer",
                Category = category,
                Price = price,
                Quantity = quantity,
                DiscountPercentage = discountPercentage,
                ActiveSubstances = new Dictionary<string, float>(),
                Batches = new Dictionary<DateOnly, int>
                {
                    { DateOnly.FromDateTime(DateTime.Today.AddDays(10)), quantity },
                },
            };
        }
    }
}