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
    public class AdminServiceTests
    {
        private Mock<IItemsRepository> mockItemRepo;
        private Mock<ISubstancesRepository> mockSubstanceRepo;
        private AdminService service;

        [SetUp]
        public void Setup()
        {
            this.mockItemRepo = new Mock<IItemsRepository>();
            this.mockSubstanceRepo = new Mock<ISubstancesRepository>();
            this.service = new AdminService(this.mockItemRepo.Object, this.mockSubstanceRepo.Object);
        }

        [Test]
        public void GetAllItems_ReturnsItemsFromRepository()
        {
            var items = new List<Item> { new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20 } };
            this.mockItemRepo.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = this.service.GetAllItems();

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetAllSubstances_ReturnsSubstancesFromRepository()
        {
            var substances = new List<Substance> { new Substance("Ibuprofen", 500f, "NSAID") };
            this.mockSubstanceRepo.Setup(repository => repository.GetAllSubstances()).Returns(substances);

            var result = this.service.GetAllSubstances();

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void SearchItemsByName_MatchingQuery_ReturnsFiltered()
        {
            var items = new List<Item>
            {
                new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20 },
                new Item { Id = 2, Name = "Paracetamol", Producer = "Teva", Category = "Pain", Price = 5f, NumberOfPills = 10 },
            };
            this.mockItemRepo.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = this.service.SearchItemsByName("asp");

            Assert.That(result[0].Name, Is.EqualTo("Aspirin"));
        }

        [Test]
        public void SearchItemsByName_NullQuery_ReturnsAll()
        {
            var items = new List<Item> { new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20 } };
            this.mockItemRepo.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = this.service.SearchItemsByName(null);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetItemById_ReturnsFromRepository()
        {
            var item = new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20 };
            this.mockItemRepo.Setup(repository => repository.GetItemById(1)).Returns(item);

            var result = this.service.GetItemById(1);

            Assert.That(result.Name, Is.EqualTo("Aspirin"));
        }

        [Test]
        public void GetSubstanceByName_ReturnsFromRepository()
        {
            var substance = new Substance("Ibuprofen", 500f, "NSAID");
            this.mockSubstanceRepo.Setup(repository => repository.GetSubstanceByName("Ibuprofen")).Returns(substance);

            var result = this.service.GetSubstanceByName("Ibuprofen");

            Assert.That(result.Name, Is.EqualTo("Ibuprofen"));
        }

        [Test]
        public void SubstanceExists_DelegatesToRepository()
        {
            this.mockSubstanceRepo.Setup(repository => repository.SubstanceExists("Ibuprofen")).Returns(true);

            Assert.That(this.service.SubstanceExists("Ibuprofen"), Is.True);
        }

        [Test]
        public void AddItem_ValidItem_CallsRepository()
        {
            this.mockItemRepo.Setup(repository => repository.GetAllItems()).Returns(new List<Item>());
            var newItem = new Item
            {
                Name = "NewItem",
                Producer = "Producer",
                Category = "Cat",
                Price = 10f,
                NumberOfPills = 5,
                Quantity = 10,
                ActiveSubstances = new Dictionary<string, float> { { "Sub1", 1.0f } },
                Batches = new Dictionary<DateOnly, int>(),
            };

            this.service.AddItem(newItem);

            this.mockItemRepo.Verify(repository => repository.AddItemWithQuantity(
                newItem.Name, newItem.Producer, newItem.Category,
                newItem.Price, newItem.NumberOfPills, newItem.Quantity,
                newItem.ActiveSubstances, newItem.Batches,
                newItem.Label, newItem.Description, newItem.ImagePath,
                newItem.DiscountPercentage), Times.Once);
        }

        [Test]
        public void AddItem_InvalidItem_ThrowsArgumentException()
        {
            var invalidItem = new Item { Name = string.Empty, Producer = "P", Price = 10f, NumberOfPills = 5, ActiveSubstances = new Dictionary<string, float> { { "S", 1f } } };

            Assert.Throws<ArgumentException>(() => this.service.AddItem(invalidItem));
        }

        [Test]
        public void AddItemWithQuantity_ValidItem_CallsRepository()
        {
            this.mockItemRepo.Setup(repository => repository.GetAllItems()).Returns(new List<Item>());
            var newItem = new Item
            {
                Name = "NewItem",
                Producer = "Producer",
                Category = "Cat",
                Price = 10f,
                NumberOfPills = 5,
                Quantity = 10,
                ActiveSubstances = new Dictionary<string, float> { { "Sub1", 1.0f } },
                Batches = new Dictionary<DateOnly, int>(),
            };

            this.service.AddItemWithQuantity(newItem);

            this.mockItemRepo.Verify(repository => repository.AddItemWithQuantity(
                newItem.Name, newItem.Producer, newItem.Category,
                newItem.Price, newItem.NumberOfPills, newItem.Quantity,
                newItem.ActiveSubstances, newItem.Batches,
                newItem.Label, newItem.Description, newItem.ImagePath,
                newItem.DiscountPercentage), Times.Once);
        }

        [Test]
        public void RemoveItemById_CallsRepository()
        {
            this.service.RemoveItemById(1);

            this.mockItemRepo.Verify(repository => repository.RemoveItemById(1), Times.Once);
        }

        [Test]
        public void UpdateItemById_ItemExists_UpdatesItem()
        {
            var existingItem = new Item { Id = 1, Name = "Old", Producer = "P", Category = "C", Price = 10f, NumberOfPills = 5 , Quantity = 5 };
            var updatedItem = new Item { Name = "Updated", Quantity = 10 };
            this.mockItemRepo.Setup(repository => repository.ItemExists(1)).Returns(true);
            this.mockItemRepo.Setup(repository => repository.GetItemById(1)).Returns(existingItem);

            this.service.UpdateItemById(1, updatedItem);

            Assert.That(updatedItem.Id, Is.EqualTo(1));
            this.mockItemRepo.Verify(repository => repository.UpdateItemById(updatedItem), Times.Once);
        }

        [Test]
        public void UpdateItemById_ItemDoesNotExist_ThrowsArgumentException()
        {
            this.mockItemRepo.Setup(repository => repository.ItemExists(1)).Returns(false);

            Assert.Throws<ArgumentException>(() => this.service.UpdateItemById(1, new Item()));
        }

        [Test]
        public void AddSubstance_NewSubstance_CallsRepository()
        {
            this.mockSubstanceRepo.Setup(repository => repository.SubstanceExists("New")).Returns(false);
            var substance = new Substance("New", 100f, "Desc");

            this.service.AddSubstance(substance);

            this.mockSubstanceRepo.Verify(repository => repository.AddSubstance("New", 100f, "Desc"), Times.Once);
        }

        [Test]
        public void AddSubstance_Duplicate_ThrowsArgumentException()
        {
            this.mockSubstanceRepo.Setup(repository => repository.SubstanceExists("Existing")).Returns(true);
            var substance = new Substance("Existing", 100f, "Desc");

            Assert.Throws<ArgumentException>(() => this.service.AddSubstance(substance));
        }

        [Test]
        public void RemoveSubstanceByName_Exists_CallsRepository()
        {
            this.mockSubstanceRepo.Setup(repository => repository.SubstanceExists("Sub")).Returns(true);
            var substance = new Substance("Sub", 100f, "Desc");

            this.service.RemoveSubstanceByName(substance);

            this.mockSubstanceRepo.Verify(repository => repository.RemoveSubstanceByName("Sub"), Times.Once);
        }

        [Test]
        public void RemoveSubstanceByName_NotExists_ThrowsArgumentException()
        {
            this.mockSubstanceRepo.Setup(repository => repository.SubstanceExists("None")).Returns(false);

            Assert.Throws<ArgumentException>(() => this.service.RemoveSubstanceByName(new Substance("None", 0f, string.Empty)));
        }

        [Test]
        public void UpdateSubstanceByName_Exists_CallsRepository()
        {
            var substance = new Substance("Sub", 200f, "Updated");
            this.mockSubstanceRepo.Setup(repository => repository.SubstanceExists("Sub")).Returns(true);

            this.service.UpdateSubstanceByName("Sub", substance);

            this.mockSubstanceRepo.Verify(repository => repository.UpdateSubstanceByName(substance), Times.Once);
        }

        [Test]
        public void UpdateSubstanceByName_NotExists_ThrowsArgumentException()
        {
            this.mockSubstanceRepo.Setup(repository => repository.SubstanceExists("None")).Returns(false);

            Assert.Throws<ArgumentException>(() => this.service.UpdateSubstanceByName("None", new Substance("None", 0f, string.Empty)));
        }

        [Test]
        public void ValidateItemForAdd_InvalidData_ThrowsArgumentException()
        {
            var item = new Item { Name = string.Empty, ActiveSubstances = new Dictionary<string, float>() };

            Assert.Throws<ArgumentException>(() => this.service.ValidateItemForAdd(item));
        }

        [Test]
        public void ValidateItemForAdd_DuplicateName_ThrowsArgumentException()
        {
            var existingItem = new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20 };
            existingItem.ActiveSubstances = new Dictionary<string, float> { { "S", 1f } };
            this.mockItemRepo.Setup(repository => repository.GetAllItems()).Returns(new List<Item> { existingItem });

            var newItem = new Item
            {
                Name = "aspirin",
                Producer = "Other",
                Price = 5f,
                NumberOfPills = 10,
                Quantity = 1,
                ActiveSubstances = new Dictionary<string, float> { { "S", 1f } },
            };

            Assert.Throws<ArgumentException>(() => this.service.ValidateItemForAdd(newItem));
        }

        [Test]
        public void GetExpiredItems_ExpiredBatch_ReturnsItem()
        {
            var expiredItem = new Item { Id = 1, Name = "Expired", Producer = "P", Category = "C", Price = 10f, NumberOfPills = 5 };
            expiredItem.Batches = new Dictionary<DateOnly, int>
            {
                { DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), 10 },
            };
            this.mockItemRepo.Setup(repository => repository.GetAllItems()).Returns(new List<Item> { expiredItem });

            var result = this.service.GetExpiredItems();

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetExpiredItems_NoBatchesExpired_ReturnsEmpty()
        {
            var freshItem = new Item { Id = 1, Name = "Fresh", Producer = "P", Category = "C", Price = 10f, NumberOfPills = 5 };
            freshItem.Batches = new Dictionary<DateOnly, int>
            {
                { DateOnly.FromDateTime(DateTime.Now.AddDays(30)), 10 },
            };
            this.mockItemRepo.Setup(repository => repository.GetAllItems()).Returns(new List<Item> { freshItem });

            var result = this.service.GetExpiredItems();

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void SendNewStockNotification_ReturnsNotification()
        {
            var item = new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20 , Quantity = 5 };

            var notification = this.service.SendNewStockNotification(item);
            Assert.That(notification.Title, Is.EqualTo("Stock Alert"));
        }

        [Test]
        public void SendAboutToExpireNotification_ReturnsNotification()
        {
            var notification = this.service.SendAboutToExpireNotification();

            Assert.That(notification.Title, Is.EqualTo("Product Expired"));
        }

        [Test]
        public void GetNotificationsForUser_AdminWithExpiredBatch_ReturnsNotification()
        {
            var user = new User(1, "a@b.com", "123", "hash", true, false, "admin", false, 0);
            var item = new Item { Id = 1, Name = "Expired", Producer = "P", Category = "C", Price = 10f, NumberOfPills = 5 };
            item.Batches = new Dictionary<DateOnly, int>
            {
                { DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), 10 },
            };
            this.mockItemRepo.Setup(repository => repository.GetAllItems()).Returns(new List<Item> { item });

            var result = this.service.GetNotificationsForUser(user);

            Assert.That(result.Count, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void GetNotificationsForUser_ClientWithStockAlert_ReturnsNotification()
        {
            var user = new User(1, "a@b.com", "123", "hash", false, false, "client", false, 0);
            user.StockAlerts.Add(1);
            var item = new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20 , Quantity = 5 };
            item.ActiveSubstances = new Dictionary<string, float> { { "ASA", 500f } };            
            this.mockItemRepo.Setup(repository => repository.GetAllItems()).Returns(new List<Item>());
            this.mockItemRepo.Setup(repository => repository.GetItemById(1)).Returns(item);

            var result = this.service.GetNotificationsForUser(user);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetTop30Items_DelegatesToRepository()
        {
            var top30 = new List<Tuple<int, string, int>> { Tuple.Create(1, "Aspirin", 100) };
            this.mockItemRepo.Setup(repository => repository.GetTop30Items()).Returns(top30);

            var result = this.service.GetTop30Items();

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetTop30Substances_DelegatesToRepository()
        {
            var top30 = new Dictionary<string, int> { { "Ibuprofen", 50 } };
            this.mockSubstanceRepo.Setup(repository => repository.GetTop30Substances()).Returns(top30);

            var result = this.service.GetTop30Substances();

            Assert.That(result.Count, Is.EqualTo(1));
        }
    }
}
