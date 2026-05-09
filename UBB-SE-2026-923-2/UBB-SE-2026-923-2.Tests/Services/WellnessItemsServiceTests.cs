namespace UBB_SE_2026_923_2.Tests.Services
{
    using System.Collections.Generic;
    using Moq;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class WellnessItemsServiceLogicTests
    {
        private Mock<IItemsRepository> mockItemsRepository;
        private WellnessItemsService wellnessItemsService;

        [SetUp]
        public void Setup()
        {
            this.mockItemsRepository = new Mock<IItemsRepository>();
            this.wellnessItemsService = new WellnessItemsService(this.mockItemsRepository.Object);
        }

        [Test]
        public void GetWellnessItems_WhenItemsHaveDifferentCategories_ReturnsOnlyWellnessItems()
        {
            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item>
                {
                    CreateItem(2, "Vitamin C", "wellness"),
                    CreateItem(1, "Paracetamol", "medicine"),
                });

            var wellnessItems = this.wellnessItemsService.GetWellnessItems();

            Assert.That(wellnessItems.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetWellnessItems_WhenWellnessCategoryUsesDifferentCasing_ReturnsMatchingItem()
        {
            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item>
                {
                    CreateItem(5, "Magnesium", "Wellness"),
                });

            var wellnessItems = this.wellnessItemsService.GetWellnessItems();

            Assert.That(wellnessItems.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetWellnessItems_WhenWellnessItemsAreUnordered_ReturnsItemsOrderedByIdentifier()
        {
            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item>
                {
                    CreateItem(5, "Second wellness item", "wellness"),
                    CreateItem(2, "First wellness item", "wellness"),
                });

            var wellnessItems = this.wellnessItemsService.GetWellnessItems();

            Assert.That(wellnessItems[0].Id, Is.EqualTo(2));
        }

        private static Item CreateItem(int itemIdentifier, string itemName, string itemCategory)
        {
            return new Item
            {
                Id = itemIdentifier,
                Name = itemName,
                Category = itemCategory,
            };
        }
    }
}