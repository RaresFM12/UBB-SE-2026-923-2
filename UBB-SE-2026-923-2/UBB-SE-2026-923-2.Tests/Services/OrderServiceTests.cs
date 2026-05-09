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
    public class OrderServiceTests
    {
        private Mock<ISubstancesRepository> mockSubstancesRepo;
        private Mock<IItemsRepository> mockItemsRepo;
        private Mock<IUsersRepository> mockUsersRepo;
        private Mock<IOrdersRepository> mockOrdersRepo;
        private Mock<IEvaluationsRepository> mockEvaluationsRepo;
        private User testUser;
        private OrderService orderService;

        [SetUp]
        public void Setup()
        {
            this.mockSubstancesRepo = new Mock<ISubstancesRepository>();
            this.mockItemsRepo = new Mock<IItemsRepository>();
            this.mockUsersRepo = new Mock<IUsersRepository>();
            this.mockOrdersRepo = new Mock<IOrdersRepository>();
            this.mockEvaluationsRepo = new Mock<IEvaluationsRepository>();

            this.testUser = new User(1, "test@test.com", "0700", "hash", false, false, "user", false, 0);

            this.orderService = new OrderService(
                this.mockSubstancesRepo.Object,
                this.mockItemsRepo.Object,
                this.mockUsersRepo.Object,
                this.mockOrdersRepo.Object,
                this.testUser,
                this.mockEvaluationsRepo.Object);
        }

        [Test]
        [TestCase(15f, 0.15f)] // 15 / 100 = 0.15
        [TestCase(0.5f, 0.5f)]
        [TestCase(-5f, 0f)]
        [TestCase(150f, 1f)]   // 150 / 100 = 1.5 -> Clamped la 1.0
        public void AddItemToBasket_NormalizesDiscounts(float inputDiscount, float expectedNormalized)
        {
            int itemId = 1;
            // REPARARE: Constructorul Item are nevoie de 11 parametri conform AdminService
            var item = new Item(
                itemId,
                "A",
                "P",
                "C",
                10f,
                1,
                "L",
                "descriere", // Arg 8 (string)
                "img.png",   // Arg 9 (string)
                0f,          // Arg 10 (float - discount)
                100);        // Arg 11 (int - stoc)

            this.mockItemsRepo.Setup(r => r.GetItemById(itemId)).Returns(item);

            this.orderService.AddItemToBasket(itemId, 1, inputDiscount);
            var basketItems = this.orderService.GetBasketItems();

            Assert.That(basketItems[0].ExtraItemDiscount, Is.EqualTo(expectedNormalized).Within(0.01));
        }

        [Test]
        public void AddItemToBasket_ExistingItem_IncrementsQuantity()
        {
            int itemId = 1;
            var item = new Item(itemId, "A", "P", "C", 10f, 1, "L", "d", "i", 0f, 100);
            this.mockItemsRepo.Setup(r => r.GetItemById(itemId)).Returns(item);

            this.orderService.AddItemToBasket(itemId, 2);
            this.orderService.AddItemToBasket(itemId, 3);

            Assert.That(this.testUser.Basket[itemId].Quantity, Is.EqualTo(5));
        }

        [Test]
        public void UpdateBasketItemQuantity_ZeroOrNegative_RemovesItem()
        {
            this.testUser.AddItemToBasket(1, 5, 0f);
            this.orderService.UpdateBasketItemQuantity(1, 0);

            Assert.That(this.testUser.Basket.ContainsKey(1), Is.False);
        }

        [Test]
        public void CompleteOrder_UpdatesItemStockAndOrderState()
        {
            int orderId = 50;
            int itemId = 1;
            var order = new Order(orderId, this.testUser.Id, DateOnly.FromDateTime(DateTime.Today), false, false);
            var item = new Item(itemId, "Pill", "P", "C", 10f, 1, "L", "d", "i", 0f, 100);

            this.mockOrdersRepo.Setup(r => r.GetOrder(orderId)).Returns(order);
            this.mockItemsRepo.Setup(r => r.GetItemById(itemId)).Returns(item);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { itemId, new Tuple<int, float>(10, 100f) }
            };

            this.orderService.CompleteOrder(orderId, updatedQuantities);

            Assert.That(order.IsCompleted, Is.True);
            this.mockOrdersRepo.Verify(r => r.UpdateOrder(order), Times.Once);
            this.mockItemsRepo.Verify(r => r.UpdateItemById(It.IsAny<Item>()), Times.Once);
        }
    }
}