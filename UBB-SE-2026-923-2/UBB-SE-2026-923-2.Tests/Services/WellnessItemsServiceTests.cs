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
    public class WellnessItemsServiceTests
    {
        private Mock<IItemsRepository> mockItemsRepository;
        private WellnessItemsService service;

        [SetUp]
        public void Setup()
        {
            mockItemsRepository = new Mock<IItemsRepository>();
            service = new WellnessItemsService(mockItemsRepository.Object);
        }

        [Test]
        public void GetWellnessItems_NoItems_ReturnsEmpty()
        {
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(new List<Item>());
            var result = service.GetWellnessItems();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetWellnessItems_NoWellnessCategory_ReturnsEmpty()
        {
            var items = new List<Item>
            {
                new Item(1, "Aspirin", "Bayer", "pain", 10f, 20, quantity: 0),
                new Item(2, "Ibuprofen", "Advil", "supplements", 15f, 30, quantity: 0),
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);
            var result = service.GetWellnessItems();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetWellnessItems_SomeWellnessItems_ReturnsOnlyWellness()
        {
            var items = new List<Item>
            {
                new Item(1, "Yoga Mat", "Fit", "wellness", 20f, 1, quantity: 0),
                new Item(2, "Aspirin", "Bayer", "pain", 10f, 20, quantity: 0),
                new Item(3, "Candle", "Zen", "wellness", 5f, 1, quantity: 0),
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);
            var result = service.GetWellnessItems();
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(i => i.Category == "wellness"), Is.True);
        }

        [Test]
        public void GetWellnessItems_CaseInsensitiveCategory_MatchesWellness()
        {
            var items = new List<Item>
            {
                new Item(1, "Item1", "P", "Wellness", 10f, 1, quantity: 0),
                new Item(2, "Item2", "P", "WELLNESS", 10f, 1, quantity: 0),
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);
            var result = service.GetWellnessItems();
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetWellnessItems_OrderedById()
        {
            var items = new List<Item>
            {
                new Item(5, "E", "P", "wellness", 10f, 1, quantity: 0),
                new Item(2, "B", "P", "wellness", 10f, 1, quantity: 0),
                new Item(8, "H", "P", "wellness", 10f, 1, quantity: 0),
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);
            var result = service.GetWellnessItems();
            Assert.That(result[0].Id, Is.EqualTo(2));
            Assert.That(result[1].Id, Is.EqualTo(5));
            Assert.That(result[2].Id, Is.EqualTo(8));
        }

        [Test]
        public void GetWellnessItems_NullCategory_NotIncluded()
        {
            var item = new Item(1, "X", "P", "wellness", 10f, 1, quantity: 0);
            var itemNull = new Item(2, "Y", "P", "", 10f, 1, quantity: 0);
            itemNull.Category = null;
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(new List<Item> { item, itemNull });
            var result = service.GetWellnessItems();
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetWellnessItems_SingleWellnessItem_ReturnsOne()
        {
            var items = new List<Item> { new Item(1, "Mat", "Fit", "wellness", 20f, 1, quantity: 0) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);
            var result = service.GetWellnessItems();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Mat"));
        }

        [Test]
        public void GetWellnessItems_MixedCase_WeLlNeSs_Matches()
        {
            var items = new List<Item> { new Item(1, "X", "P", "WeLlNeSs", 10f, 1, quantity: 0) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);
            var result = service.GetWellnessItems();
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetWellnessItems_CategoryWithSpaces_NotIncluded()
        {
            var items = new List<Item> { new Item(1, "X", "P", " wellness ", 10f, 1, quantity: 0) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);
            var result = service.GetWellnessItems();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetWellnessItems_ManyItems_ReturnsAllWellness()
        {
            var items = new List<Item>();
            for (int i = 1; i <= 50; i++)
                items.Add(new Item(i, $"Item{i}", "P", i % 2 == 0 ? "wellness" : "other", 10f, 1, quantity: 0));
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);
            var result = service.GetWellnessItems();
            Assert.That(result.Count, Is.EqualTo(25));
        }

        [Test]
        public void GetWellnessItems_AllWellness_ReturnsAll()
        {
            var items = new List<Item>
            {
                new Item(1, "A", "P", "wellness", 10f, 1, quantity: 0),
                new Item(2, "B", "P", "wellness", 20f, 2, quantity: 0),
                new Item(3, "C", "P", "wellness", 30f, 3, quantity: 0),
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);
            var result = service.GetWellnessItems();
            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void GetWellnessItems_PreservesItemProperties()
        {
            var items = new List<Item> { new Item(7, "Candle", "Zen", "wellness", 15.5f, 3, quantity: 10) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);
            var result = service.GetWellnessItems();
            Assert.That(result[0].Id, Is.EqualTo(7));
            Assert.That(result[0].Name, Is.EqualTo("Candle"));
            Assert.That(result[0].Producer, Is.EqualTo("Zen"));
            Assert.That(result[0].Price, Is.EqualTo(15.5f));
        }

        [Test]
        public void GetWellnessItems_EmptyCategoryString_NotIncluded()
        {
            var items = new List<Item> { new Item(1, "X", "P", "", 10f, 1, quantity: 0) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);
            var result = service.GetWellnessItems();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetWellnessItems_CategoryWellnessSubstring_NotIncluded()
        {
            var items = new List<Item> { new Item(1, "X", "P", "wellnessplus", 10f, 1, quantity: 0) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);
            var result = service.GetWellnessItems();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetWellnessItems_OrderIsByIdAscending()
        {
            var items = new List<Item>
            {
                new Item(100, "Z", "P", "wellness", 10f, 1, quantity: 0),
                new Item(1, "A", "P", "wellness", 10f, 1, quantity: 0),
                new Item(50, "M", "P", "wellness", 10f, 1, quantity: 0),
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);
            var result = service.GetWellnessItems();
            Assert.That(result[0].Id, Is.EqualTo(1));
            Assert.That(result[1].Id, Is.EqualTo(50));
            Assert.That(result[2].Id, Is.EqualTo(100));
        }

        [Test]
        public void GetWellnessItems_ReturnsList()
        {
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(new List<Item>());
            var result = service.GetWellnessItems();
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<List<Item>>());
        }

        [Test]
        public void GetWellnessItems_CalledTwice_ReturnsSameData()
        {
            var items = new List<Item> { new Item(1, "A", "P", "wellness", 10f, 1, quantity: 0) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);
            var result1 = service.GetWellnessItems();
            var result2 = service.GetWellnessItems();
            Assert.That(result1.Count, Is.EqualTo(result2.Count));
        }

        [Test]
        public void GetWellnessItems_DoesNotModifyRepository()
        {
            var items = new List<Item> { new Item(1, "A", "P", "wellness", 10f, 1, quantity: 0) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);
            service.GetWellnessItems();
            mockItemsRepository.Verify(r => r.GetAllItems(), Times.Once);
        }

        [Test]
        public void GetWellnessItems_ZeroPriceItem_StillReturned()
        {
            var items = new List<Item> { new Item(1, "Free", "P", "wellness", 0f, 1, quantity: 0) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);
            var result = service.GetWellnessItems();
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetWellnessItems_NegativePriceItem_StillReturned()
        {
            var items = new List<Item> { new Item(1, "Discount", "P", "wellness", -5f, 1, quantity: 0) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);
            var result = service.GetWellnessItems();
            Assert.That(result.Count, Is.EqualTo(1));
        }
    }
}
