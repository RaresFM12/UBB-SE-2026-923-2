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
    public class AdminServiceLogicTests
    {
        private Mock<IItemsRepository> mockItemsRepository;
        private Mock<ISubstancesRepository> mockSubstancesRepository;
        private AdminService adminService;

        [SetUp]
        public void Setup()
        {
            this.mockItemsRepository = new Mock<IItemsRepository>();
            this.mockSubstancesRepository = new Mock<ISubstancesRepository>();
            this.adminService = new AdminService(this.mockItemsRepository.Object, this.mockSubstancesRepository.Object);
        }

        [Test]
        public void SearchItemsByName_WhenQueryUsesDifferentCasing_ReturnsMatchingItemsOnly()
        {
            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item>
                {
                    CreateValidItem("Paracetamol"),
                    CreateValidItem("Ibuprofen"),
                });

            var matchingItems = this.adminService.SearchItemsByName("PARA");

            Assert.That(matchingItems.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateItemForAdd_WhenItemHasNoActiveSubstances_ThrowsArgumentException()
        {
            var itemWithoutActiveSubstances = CreateValidItem("Valid item");
            itemWithoutActiveSubstances.ActiveSubstances = new Dictionary<string, float>();

            Assert.Throws<ArgumentException>(() => this.adminService.ValidateItemForAdd(itemWithoutActiveSubstances));
        }

        [Test]
        public void ValidateItemForAdd_WhenItemNameAlreadyExistsIgnoringCase_ThrowsArgumentException()
        {
            var existingItem = CreateValidItem("Paracetamol");
            var newItemWithSameName = CreateValidItem("paracetamol");

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item> { existingItem });

            Assert.Throws<ArgumentException>(() => this.adminService.ValidateItemForAdd(newItemWithSameName));
        }

        [Test]
        public void UpdateItemById_WhenItemIdentifierDoesNotExist_ThrowsArgumentException()
        {
            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.ItemExists(99))
                .Returns(false);

            Assert.Throws<ArgumentException>(() => this.adminService.UpdateItemById(99, CreateValidItem("Updated item")));
        }

        [Test]
        public void UpdateItemById_WhenItemExists_SendsUpdatedItemWithRequestedIdentifierToRepository()
        {
            var requestedItemIdentifier = 7;
            var previousItemWithoutStock = CreateValidItem("Previous item");
            previousItemWithoutStock.Quantity = 0;
            var updatedItemWithStock = CreateValidItem("Updated item");
            updatedItemWithStock.Quantity = 5;

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.ItemExists(requestedItemIdentifier))
                .Returns(true);

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetItemById(requestedItemIdentifier))
                .Returns(previousItemWithoutStock);

            this.adminService.UpdateItemById(requestedItemIdentifier, updatedItemWithStock);

            this.mockItemsRepository.Verify(
                itemsRepository => itemsRepository.UpdateItemById(
                    It.Is<Item>(updatedItem => updatedItem.Id == requestedItemIdentifier)),
                Times.Once);
        }

        [Test]
        public void AddSubstance_WhenSubstanceNameAlreadyExists_ThrowsArgumentException()
        {
            var existingSubstance = new Substance("Caffeine", 10, "Existing substance");

            this.mockSubstancesRepository
                .Setup(substancesRepository => substancesRepository.SubstanceExists(existingSubstance.Name))
                .Returns(true);

            Assert.Throws<ArgumentException>(() => this.adminService.AddSubstance(existingSubstance));
        }

        [Test]
        public void RemoveSubstanceByName_WhenSubstanceDoesNotExist_ThrowsArgumentException()
        {
            var missingSubstance = new Substance("Missing substance", 10, "Missing substance description");

            this.mockSubstancesRepository
                .Setup(substancesRepository => substancesRepository.SubstanceExists(missingSubstance.Name))
                .Returns(false);

            Assert.Throws<ArgumentException>(() => this.adminService.RemoveSubstanceByName(missingSubstance));
        }

        [Test]
        public void GetExpiredItems_WhenOneItemHasExpiredBatch_ReturnsOnlyExpiredItem()
        {
            var expiredItem = CreateValidItem("Expired item");
            expiredItem.Batches = new Dictionary<DateOnly, int>
            {
                { DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), 3 },
            };

            var validItem = CreateValidItem("Valid item");
            validItem.Batches = new Dictionary<DateOnly, int>
            {
                { DateOnly.FromDateTime(DateTime.Today.AddDays(5)), 3 },
            };

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item> { expiredItem, validItem });

            var expiredItems = this.adminService.GetExpiredItems();

            Assert.That(expiredItems.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetNotificationsForUser_WhenAdminHasExpiredItem_ReturnsExpiredProductNotification()
        {
            var administrator = new User(1, "admin@test.com", "1234567890", "hashedPassword", true, false, "admin", false, 0);
            var expiredItem = CreateValidItem("Expired item");
            expiredItem.Id = 11;
            expiredItem.Batches = new Dictionary<DateOnly, int>
            {
                { DateOnly.FromDateTime(DateTime.Today), 1 },
            };

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item> { expiredItem });

            var notifications = this.adminService.GetNotificationsForUser(administrator);

            Assert.That(notifications.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetNotificationsForUser_WhenStockAlertItemIsBackInStock_ReturnsStockNotification()
        {
            var clientWithStockAlert = new User(2, "client@test.com", "1234567890", "hashedPassword", false, false, "client", false, 0);
            clientWithStockAlert.StockAlerts.Add(20);

            var itemBackInStock = CreateValidItem("Back in stock item");
            itemBackInStock.Id = 20;
            itemBackInStock.Quantity = 4;

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetItemById(20))
                .Returns(itemBackInStock);

            var notifications = this.adminService.GetNotificationsForUser(clientWithStockAlert);

            Assert.That(notifications.Count, Is.EqualTo(1));
        }

        private static Item CreateValidItem(string itemName)
        {
            return new Item
            {
                Name = itemName,
                Producer = "Valid producer",
                Category = "Valid category",
                Price = 10,
                NumberOfPills = 20,
                Quantity = 5,
                DiscountPercentage = 0,
                ActiveSubstances = new Dictionary<string, float>
                {
                    { "Valid substance", 1.5f },
                },
                Batches = new Dictionary<DateOnly, int>
                {
                    { DateOnly.FromDateTime(DateTime.Today.AddDays(10)), 5 },
                },
                Label = "Valid label",
                Description = "Valid description",
                ImagePath = "valid-image.png",
            };
        }
    }
}