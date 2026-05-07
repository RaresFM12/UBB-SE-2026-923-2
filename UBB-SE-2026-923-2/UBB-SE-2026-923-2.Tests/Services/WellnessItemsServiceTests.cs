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
        private Mock<IItemsRepository> mockItemsRepo;
        private WellnessItemsService service;

        [SetUp]
        public void Setup()
        {
            mockItemsRepo = new Mock<IItemsRepository>();
            service = new WellnessItemsService(mockItemsRepo.Object);
        }

        [Test]
        public void GetWellnessItems_NoItems_ReturnsEmpty()
        {
            mockItemsRepo.Setup(r => r.GetAllItems()).Returns(new List<Item>());
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
            mockItemsRepo.Setup(r => r.GetAllItems()).Returns(items);
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
            mockItemsRepo.Setup(r => r.GetAllItems()).Returns(items);
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
            mockItemsRepo.Setup(r => r.GetAllItems()).Returns(items);
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
            mockItemsRepo.Setup(r => r.GetAllItems()).Returns(items);
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
            mockItemsRepo.Setup(r => r.GetAllItems()).Returns(new List<Item> { item, itemNull });
            var result = service.GetWellnessItems();
            Assert.That(result.Count, Is.EqualTo(1));
        }
    }
}
