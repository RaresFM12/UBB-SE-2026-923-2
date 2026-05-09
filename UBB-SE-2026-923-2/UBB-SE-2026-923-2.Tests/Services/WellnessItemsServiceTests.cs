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
    public class WellnessItemsServiceTests
    {
        private Mock<IItemsRepository> mockItemsRepository;
        private WellnessItemsService service;

        [SetUp]
        public void Setup()
        {
            this.mockItemsRepository = new Mock<IItemsRepository>();
            this.service = new WellnessItemsService(this.mockItemsRepository.Object);
        }

        [Test]
        public void GetWellnessItems_ValidItems_FiltersAndOrdersCorrectly()
        {
            // Folosim "quantity: 0" explicit ca să evităm erorile de tip
            var items = new List<Item>
            {
                new Item(50, "M", "P", "wellness", 10f, 1, "label", quantity: 0),
                new Item(1, "A", "P", "WEllNeSs", 10f, 1, "label", quantity: 0),
                new Item(2, "B", "P", "pain", 15f, 1, "label", quantity: 0),
                new Item(10, "Z", "P", "wellness", 10f, 1, "label", quantity: 0)
            };
            this.mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = this.service.GetWellnessItems();

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].Id, Is.EqualTo(1));
            Assert.That(result[1].Id, Is.EqualTo(10));
            Assert.That(result[2].Id, Is.EqualTo(50));
        }

        [Test]
        public void GetWellnessItems_NoMatchingItems_ReturnsEmpty()
        {
            this.mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(new List<Item>
            {
                new Item(1, "A", "P", "supplements", 10f, 1, "label", quantity: 0)
            });

            var result = this.service.GetWellnessItems();
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetWellnessItems_NullCategoryItems_AreIgnored()
        {
            var itemWithNull = new Item(1, "X", "P", "wellness", 10f, 1, "label", quantity: 0);
            itemWithNull.Category = null;

            this.mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(new List<Item> { itemWithNull });

            var result = this.service.GetWellnessItems();
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetWellnessItems_PreservesOriginalProperties()
        {
            var sourceItem = new Item(7, "Candle", "Zen", "wellness", 15.5f, 3, "label", quantity: 10);
            this.mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(new List<Item> { sourceItem });

            var result = this.service.GetWellnessItems();

            Assert.Multiple(() =>
            {
                Assert.That(result[0].Name, Is.EqualTo("Candle"));
                Assert.That(result[0].Price, Is.EqualTo(15.5f));
                Assert.That(result[0].Quantity, Is.EqualTo(10));
            });
        }
    }
}