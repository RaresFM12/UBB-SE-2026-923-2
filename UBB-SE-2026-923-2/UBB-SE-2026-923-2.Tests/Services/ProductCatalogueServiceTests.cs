using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.Tests.Services
{
    [TestFixture]
    public class ProductCatalogueServiceTests
    {
        private Mock<IItemsRepository> mockItemsRepository;
        private ProductCatalogueService service;

        private List<Item> sampleItems;

        [SetUp]
        public void Setup()
        {
            mockItemsRepository = new Mock<IItemsRepository>();
            service = new ProductCatalogueService(mockItemsRepository.Object);

            sampleItems = new List<Item>
            {
                CreateItem(1, "Aspirin", "Bayer", "pain", 10f, 20, 50, 0f, "wellness"),
                CreateItem(2, "Ibuprofen", "Advil", "pain", 15f, 30, 100, 0.1f, "pain"),
                CreateItem(3, "Vitamin C", "Nature", "vitamins", 5f, 60, 200, 0f, "vitamins"),
                CreateItem(4, "Omega3", "Fish", "supplements", 25f, 90, 0, 0.2f, "supplements"),
                CreateItem(5, "Paracetamol", "Generic", "pain", 8f, 10, 5, 0.05f, "pain"),
                CreateItem(6, "Zinc", "Nature", "vitamins", 12f, 30, 15, 0f, "vitamins"),
                CreateItem(7, "Iron", "Nature", "vitamins", 7f, 30, 0, 0f, "vitamins"),
                CreateItem(8, "Calcium", "Pharma", "vitamins", 9f, 60, 30, 0.5f, "vitamins"),
                CreateItem(9, "Magnesium", "Pharma", "vitamins", 11f, 30, 20, 0f, "vitamins"),
                CreateItem(10, "Probiotics", "Bio", "supplements", 20f, 30, 50, 0.15f, "supplements"),
                CreateItem(11, "Melatonin", "Sleep", "sleep", 6f, 30, 40, 0f, "sleep"),
                CreateItem(12, "Collagen", "Beauty", "beauty", 30f, 60, 10, 0.1f, "beauty"),
            };

            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(sampleItems);
        }

        private static Item CreateItem(int id, string name, string producer, string label, float price, int pills, int quantity, float discount, string category)
        {
            var item = new Item(id, name, producer, category, price, pills, label, "", "", discount: discount, quantity: 0);
            if (quantity > 0)
            {
                item.Batches[DateOnly.FromDateTime(System.DateTime.Now.AddDays(30))] = quantity;
            }
            return item;
        }

        [Test]
        public void GetItems_NoFilters_ReturnsAllItems()
        {
            var result = service.GetItems(null, pageSize: 100);
            Assert.That(result.Count, Is.EqualTo(12));
        }

        [Test]
        public void GetItems_SearchByName_FiltersCorrectly()
        {
            var result = service.GetItems("Aspirin", pageSize: 100);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Aspirin"));
        }

        [Test]
        public void GetItems_SearchByNameCaseInsensitive_Works()
        {
            var result = service.GetItems("aspirin", pageSize: 100);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetItems_SearchNoMatch_ReturnsEmpty()
        {
            var result = service.GetItems("NonExistent", pageSize: 100);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetItems_FilterByCategory_ReturnsMatching()
        {
            var result = service.GetItems(null, categories: new List<string> { "vitamins" }, pageSize: 100);
            Assert.That(result.All(item => item.Category == "vitamins"), Is.True);
        }

        [Test]
        public void GetItems_FilterByMultipleCategories_ReturnsAll()
        {
            var result = service.GetItems(null, categories: new List<string> { "vitamins", "pain" }, pageSize: 100);
            Assert.That(result.All(item => item.Category == "vitamins" || item.Category == "pain"), Is.True);
        }

        [Test]
        public void GetItems_FilterByCategory_EmptyList_ReturnsAll()
        {
            var result = service.GetItems(null, categories: new List<string>(), pageSize: 100);
            Assert.That(result.Count, Is.EqualTo(12));
        }

        [Test]
        public void GetItems_FilterByPriceRange_ReturnsInRange()
        {
            var result = service.GetItems(null, priceRanges: new List<(float, float)> { (0f, 10f) }, pageSize: 100);
            Assert.That(result.All(item => item.Price * (1 - item.DiscountPercentage) <= 10f), Is.True);
        }

        [Test]
        public void GetItems_FilterByPriceRange_InvalidRange_Throws()
        {
            Assert.Throws<System.ArgumentException>(() =>
                service.GetItems(null, priceRanges: new List<(float, float)> { (20f, 5f) }, pageSize: 100));
        }

        [Test]
        public void GetItems_FilterByPriceRange_NegativeMin_Throws()
        {
            Assert.Throws<System.ArgumentException>(() =>
                service.GetItems(null, priceRanges: new List<(float, float)> { (-1f, 10f) }, pageSize: 100));
        }

        [Test]
        public void GetItems_StockFilterInStock_ReturnsOnlyInStock()
        {
            var result = service.GetItems(null, stockFilter: "in_stock", pageSize: 100);
            Assert.That(result.All(item => item.Quantity > 0), Is.True);
        }

        [Test]
        public void GetItems_StockFilterLowStock_ReturnsLowStock()
        {
            var result = service.GetItems(null, stockFilter: "low_stock", pageSize: 100);
            Assert.That(result.All(item => item.Quantity > 0 && item.Quantity < 10), Is.True);
        }

        [Test]
        public void GetItems_StockFilterNull_ReturnsAll()
        {
            var result = service.GetItems(null, stockFilter: null, pageSize: 100);
            Assert.That(result.Count, Is.EqualTo(12));
        }

        [Test]
        public void GetItems_StockFilterUnknown_ReturnsAll()
        {
            var result = service.GetItems(null, stockFilter: "unknown_filter", pageSize: 100);
            Assert.That(result.Count, Is.EqualTo(12));
        }

        [Test]
        public void GetItems_DiscountedTrue_ReturnsOnlyDiscounted()
        {
            var result = service.GetItems(null, discounted: true, pageSize: 100);
            Assert.That(result.All(item => item.DiscountPercentage > 0), Is.True);
        }

        [Test]
        public void GetItems_DiscountedFalse_ReturnsOnlyNonDiscounted()
        {
            var result = service.GetItems(null, discounted: false, pageSize: 100);
            Assert.That(result.All(item => item.DiscountPercentage == 0), Is.True);
        }

        [Test]
        public void GetItems_DiscountedNull_ReturnsAll()
        {
            var result = service.GetItems(null, discounted: null, pageSize: 100);
            Assert.That(result.Count, Is.EqualTo(12));
        }

        [Test]
        public void GetItems_FilterBySubstance_ReturnsMatching()
        {
            sampleItems[0].ActiveSubstances["acetylsalicylic"] = 500f;
            var result = service.GetItems(null, substances: new List<string> { "acetylsalicylic" }, pageSize: 100);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Aspirin"));
        }

        [Test]
        public void GetItems_FilterBySubstance_NoMatch_ReturnsEmpty()
        {
            var result = service.GetItems(null, substances: new List<string> { "nonexistent_substance" }, pageSize: 100);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetItems_SortByPriceAscending_ReturnsSorted()
        {
            var result = service.GetItems(null, sortBy: "price", ascending: true, pageSize: 100);
            for (int itemIndex = 1; itemIndex < result.Count; itemIndex++)
            {
                Assert.That(result[itemIndex].Price, Is.GreaterThanOrEqualTo(result[itemIndex - 1].Price));
            }
        }

        [Test]
        public void GetItems_SortByPriceDescending_ReturnsSorted()
        {
            var result = service.GetItems(null, sortBy: "price", ascending: false, pageSize: 100);
            for (int itemIndex = 1; itemIndex < result.Count; itemIndex++)
            {
                Assert.That(result[itemIndex].Price, Is.LessThanOrEqualTo(result[itemIndex - 1].Price));
            }
        }

        [Test]
        public void GetItems_SortByNewestAscending_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => service.GetItems(null, sortBy: "newest", ascending: true, pageSize: 100));
        }

        [Test]
        public void GetItems_SortByNewestDescending_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => service.GetItems(null, sortBy: "newest", ascending: false, pageSize: 100));
        }

        [Test]
        public void GetItems_SortByNull_ReturnsUnsorted()
        {
            var result = service.GetItems(null, sortBy: null, pageSize: 100);
            Assert.That(result.Count, Is.EqualTo(12));
        }

        [Test]
        public void GetItems_Pagination_FirstPage()
        {
            var result = service.GetItems(null, page: 0, pageSize: 5);
            Assert.That(result.Count, Is.EqualTo(5));
        }

        [Test]
        public void GetItems_Pagination_SecondPage()
        {
            var result = service.GetItems(null, page: 1, pageSize: 5);
            Assert.That(result.Count, Is.EqualTo(5));
        }

        [Test]
        public void GetItems_Pagination_LastPage()
        {
            var result = service.GetItems(null, page: 2, pageSize: 5);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetItems_Pagination_BeyondLastPage_ReturnsEmpty()
        {
            var result = service.GetItems(null, page: 10, pageSize: 5);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetItems_CombinedFilters_Work()
        {
            var result = service.GetItems("i", categories: new List<string> { "vitamins" }, discounted: false, pageSize: 100);
            Assert.That(result.All(item => item.Category == "vitamins" && item.DiscountPercentage == 0 && item.Name.Contains("i", System.StringComparison.OrdinalIgnoreCase)), Is.True);
        }

        [Test]
        public void GetItems_EmptySearch_ReturnsAll()
        {
            var result = service.GetItems("", pageSize: 100);
            Assert.That(result.Count, Is.EqualTo(12));
        }

        [Test]
        public void GetItems_WhitespaceSearch_ReturnsAll()
        {
            var result = service.GetItems("   ", pageSize: 100);
            Assert.That(result.Count, Is.EqualTo(12));
        }

        [Test]
        public void GetItems_MultiplePriceRanges_ReturnsUnion()
        {
            var result = service.GetItems(null, priceRanges: new List<(float, float)> { (0f, 5f), (20f, 30f) }, pageSize: 100);
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void GetItems_DefaultPageSize_Returns10()
        {
            var result = service.GetItems(null);
            Assert.That(result.Count, Is.LessThanOrEqualTo(10));
        }
    }
}




