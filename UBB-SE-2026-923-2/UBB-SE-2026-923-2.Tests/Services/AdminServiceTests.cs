namespace UBB_SE_2026_923_2.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Moq;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class AdminServiceTests
    {
        private Mock<IItemsRepository> mockItemsRepository;
        private Mock<ISubstancesRepository> mockSubstancesRepository;
        private AdminService adminService;

        [SetUp]
        public void Setup()
        {
            this.mockItemsRepository = new Mock<IItemsRepository>();
            this.mockSubstancesRepository = new Mock<ISubstancesRepository>();

            // FIX: AdminService cere DOUĂ repository-uri în constructor conform codului tău
            this.adminService = new AdminService(this.mockItemsRepository.Object, this.mockSubstancesRepository.Object);
        }

        // Helper pentru a crea un Item valid (folosind ordinea din proiectul tău)
        private static Item CreateValidItem(int id = 1, string name = "Aspirin", int quantity = 10)
        {
            return new Item(id, name, "Bayer", "wellness", 10f, 20, "label", "desc", "img.png", 0f, quantity);
        }

        [Test]
        public void AddItem_InvalidData_ThrowsArgumentException()
        {
            // Nume gol = date invalide
            var invalidItem = new Item(1, string.Empty, "", "", 0, 0, "", "", "", 0, 0);

            Assert.Throws<ArgumentException>(() => this.adminService.AddItem(invalidItem));
        }

        [Test]
        public void AddItem_ValidNewItem_CallsRepository()
        {
            var newItem = CreateValidItem(name: "UniquePill");
            this.mockItemsRepository.Setup(r => r.GetAllItems()).Returns(new List<Item>());

            this.adminService.AddItem(newItem);

            // Verificăm că s-a apelat metoda cu cele 12+ argumente din repository
            this.mockItemsRepository.Verify(r => r.AddItemWithQuantity(
                newItem.Name, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<float>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dictionary<string, float>>(),
                It.IsAny<Dictionary<DateOnly, int>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<float>()), Times.Once);
        }

        [Test]
        public void UpdateItemById_StockGoesFromZeroToPositive_ReturnsNotification()
        {
            var oldItem = CreateValidItem(id: 1, quantity: 0);
            var updatedItem = CreateValidItem(id: 1, quantity: 5);

            this.mockItemsRepository.Setup(r => r.ItemExists(1)).Returns(true);
            this.mockItemsRepository.Setup(r => r.GetItemById(1)).Returns(oldItem);

            this.adminService.UpdateItemById(1, updatedItem);

            this.mockItemsRepository.Verify(r => r.UpdateItemById(updatedItem), Times.Once);
        }

        [Test]
        public void SearchItemsByName_ReturnsFilteredResults()
        {
            var items = new List<Item>
            {
                CreateValidItem(1, "Paracetamol"),
                CreateValidItem(2, "Aspirin")
            };
            this.mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = this.adminService.SearchItemsByName("PARA");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Paracetamol"));
        }

        [Test]
        public void GetExpiredItems_ReturnsOnlyItemsWithPastBatches()
        {
            var expiredItem = CreateValidItem(1);
            // Adăugăm un batch expirat (ieri)
            expiredItem.Batches[DateOnly.FromDateTime(DateTime.Now.AddDays(-1))] = 10;

            var freshItem = CreateValidItem(2);
            // Adăugăm un batch valabil (peste un an)
            freshItem.Batches[DateOnly.FromDateTime(DateTime.Now.AddYears(1))] = 10;

            this.mockItemsRepository.Setup(r => r.GetAllItems()).Returns(new List<Item> { expiredItem, freshItem });

            var result = this.adminService.GetExpiredItems();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(1));
        }

        [Test]
        public void AddSubstance_AlreadyExists_ThrowsArgumentException()
        {
            var sub = new Substance { Name = "Zinc" };
            this.mockSubstancesRepository.Setup(r => r.SubstanceExists("Zinc")).Returns(true);

            Assert.Throws<ArgumentException>(() => this.adminService.AddSubstance(sub));
        }

        [Test]
        public void GetNotificationsForUser_Admin_ChecksForExpiredItems()
        {
            var adminUser = new User(1, "a@a.com", "07", "h", true, false, "admin", false, 0);
            var itemWithExpiredBatch = CreateValidItem(99);
            itemWithExpiredBatch.Batches[DateOnly.FromDateTime(DateTime.Now.AddDays(-1))] = 5;

            this.mockItemsRepository.Setup(r => r.GetAllItems()).Returns(new List<Item> { itemWithExpiredBatch });

            var result = this.adminService.GetNotificationsForUser(adminUser);

            Assert.That(result.Any(n => n.Title == "Product Expired"), Is.True);
        }
    }
}