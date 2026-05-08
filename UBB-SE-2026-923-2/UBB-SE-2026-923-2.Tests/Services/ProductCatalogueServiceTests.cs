namespace UBB_SE_2026_923_2.Tests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Moq;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class ProductCatalogueServiceTests
    {
        private Mock<IItemsRepository> mockItemsRepository;
        private ProductCatalogueService service;

        private List<Item> sampleItems;

        [SetUp]
        public void Setup()
        {
            this.mockItemsRepository = new Mock<IItemsRepository>();
            this.service = new ProductCatalogueService(this.mockItemsRepository.Object);

            this.sampleItems = new List<Item>
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

            this.mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(this.sampleItems);
        }

        private static Item CreateItem(int id, string name, string producer, string label, float price, int pills, int quantity, float discount, string category)
        {
            var item = new Item(id, name, producer, category, price, pills, label, string.Empty, string.Empty, discount: discount, quantity: 0);
            if (quantity > 0)
            {
                item.Batches[System.DateOnly.FromDateTime(System.DateTime.Now.AddDays(30))] = quantity;
            }

            return item;
        }

        // --- Search ---
        [Test]
        public void GetItems_NoFilters_ReturnsAllItems()
        {
            var result = this.service.GetItems(null, pageSize: 100);
            Assert.That(result.Count, Is.EqualTo(12));
        }

        [Test]
        public void GetItems_SearchByName_FiltersCorrectly()
        {
            var result = this.service.GetItems("Aspirin", pageSize: 100);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Aspirin"));
        }

        [Test]
        public void GetItems_SearchNoMatch_ReturnsEmpty()
        {
            var result = this.service.GetItems("NonExistent", pageSize: 100);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        // --- Categories ---
        [Test]
        public void GetItems_FilterByCategory_ReturnsMatching()
        {
            var result = this.service.GetItems(null, categories: new List<string> { "vitamins" }, pageSize: 100);
            Assert.That(result.All(item => item.Category == "vitamins"), Is.True);
        }

        [Test]
        public void GetItems_FilterByMultipleCategories_ReturnsAll()
        {
            var result = this.service.GetItems(null, categories: new List<string> { "vitamins", "pain" }, pageSize: 100);
            Assert.That(result.All(item => item.Category == "vitamins" || item.Category == "pain"), Is.True);
        }

        // --- Price ---
        [Test]
        public void GetItems_FilterByPriceRange_ReturnsInRange()
        {
            var result = this.service.GetItems(null, priceRanges: new List<(float, float)> { (0f, 10f) }, pageSize: 100);
            Assert.That(result.All(item => item.Price * (1 - item.DiscountPercentage) <= 10f), Is.True);
        }

        [Test]
        public void GetItems_FilterByPriceRange_InvalidRange_Throws()
        {
            Assert.Throws<System.ArgumentException>(() =>
                this.service.GetItems(null, priceRanges: new List<(float, float)> { (20f, 5f) }, pageSize: 100));
        }

        // --- Stock ---
        [Test]
        public void GetItems_StockFilterInStock_ReturnsOnlyInStock()
        {
            var result = this.service.GetItems(null, stockFilter: "in_stock", pageSize: 100);
            Assert.That(result.All(item => item.Quantity > 0), Is.True);
        }

        [Test]
        public void GetItems_StockFilterLowStock_ReturnsLowStock()
        {
            var result = this.service.GetItems(null, stockFilter: "low_stock", pageSize: 100);
            Assert.That(result.All(item => item.Quantity > 0 && item.Quantity < 10), Is.True);
        }

        // --- Discount ---
        [Test]
        public void GetItems_DiscountedTrue_ReturnsOnlyDiscounted()
        {
            var result = this.service.GetItems(null, discounted: true, pageSize: 100);
            Assert.That(result.All(item => item.DiscountPercentage > 0), Is.True);
        }

        // --- Substances ---
        [Test]
        public void GetItems_FilterBySubstance_ReturnsMatching()
        {
            this.sampleItems[0].ActiveSubstances["acetylsalicylic"] = 500f;
            var result = this.service.GetItems(null, substances: new List<string> { "acetylsalicylic" }, pageSize: 100);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Aspirin"));
        }

        // --- Sorting ---
        [Test]
        public void GetItems_SortByPriceDescending_ReturnsSorted()
        {
            var result = this.service.GetItems(null, sortBy: "price", ascending: false, pageSize: 100);
            for (int itemIndex = 1; itemIndex < result.Count; itemIndex++)
            {
                Assert.That(result[itemIndex].Price, Is.LessThanOrEqualTo(result[itemIndex - 1].Price));
            }
        }

        // --- Pagination & Integration ---
        [Test]
        public void GetItems_Pagination_FirstPage()
        {
            var result = this.service.GetItems(null, page: 0, pageSize: 5);
            Assert.That(result.Count, Is.EqualTo(5));
        }

        [Test]
        public void GetItems_CombinedFilters_Work()
        {
            var result = this.service.GetItems("i", categories: new List<string> { "vitamins" }, discounted: false, pageSize: 100);
            Assert.That(result.All(item => item.Category == "vitamins" && item.DiscountPercentage == 0 && item.Name.Contains("i", System.StringComparison.OrdinalIgnoreCase)), Is.True);
        }
    }
}