namespace UBB_SE_2026_923_2.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using Moq;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Orders;

    [TestFixture]
    public class OrderServiceLogicTests
    {
        private Mock<ISubstancesRepository> mockSubstancesRepository;
        private Mock<IItemsRepository> mockItemsRepository;
        private Mock<IUsersRepository> mockUsersRepository;
        private Mock<IOrdersRepository> mockOrdersRepository;
        private Mock<IEvaluationsRepository> mockEvaluationsRepository;
        private User activeUser;
        private OrderService orderService;

        [SetUp]
        public void Setup()
        {
            this.mockSubstancesRepository = new Mock<ISubstancesRepository>();
            this.mockItemsRepository = new Mock<IItemsRepository>();
            this.mockUsersRepository = new Mock<IUsersRepository>();
            this.mockOrdersRepository = new Mock<IOrdersRepository>();
            this.mockEvaluationsRepository = new Mock<IEvaluationsRepository>();
            this.activeUser = CreateUser(7);

            this.orderService = new OrderService(
                this.mockSubstancesRepository.Object,
                this.mockItemsRepository.Object,
                this.mockUsersRepository.Object,
                this.mockOrdersRepository.Object,
                this.activeUser,
                this.mockEvaluationsRepository.Object);
        }

        [Test]
        public void AddItemToBasket_WhenItemAlreadyExists_IncreasesBasketItemQuantity()
        {
            this.activeUser.AddItemToBasket(10, 2, 0);

            this.orderService.AddItemToBasket(10, 3, 0);

            Assert.That(this.activeUser.Basket[10].Quantity, Is.EqualTo(5));
        }

        [Test]
        public void AddItemToBasket_WhenExistingItemHasHigherExtraDiscount_KeepsHigherExtraDiscount()
        {
            this.activeUser.AddItemToBasket(10, 2, 0.30f);

            this.orderService.AddItemToBasket(10, 3, 0.10f);

            Assert.That(this.activeUser.Basket[10].ExtraDiscountPercentage, Is.EqualTo(0.30f));
        }

        [Test]
        public void UpdateBasketItemQuantity_WhenNewQuantityIsZero_RemovesItemFromBasket()
        {
            this.activeUser.AddItemToBasket(10, 2, 0);

            this.orderService.UpdateBasketItemQuantity(10, 0);

            Assert.That(this.activeUser.Basket.ContainsKey(10), Is.False);
        }

        [Test]
        public void GetBasketItems_WhenRepositoryThrowsForBasketItem_RemovesInvalidItemFromBasket()
        {
            this.activeUser.AddItemToBasket(10, 2, 0);

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetItemById(10))
                .Throws(new ArgumentException("Missing item"));

            this.orderService.GetBasketItems();

            Assert.That(this.activeUser.Basket.ContainsKey(10), Is.False);
        }

        [Test]
        public void RecalculateBasketItemPrices_WhenMultipleDiscountsExist_CalculatesFinalPriceAfterDiscount()
        {
            var basketItem = new BasketItemViewModel(
                10,
                "ms-appx:///Assets/logo.png",
                "Paracetamol",
                "Producer",
                2,
                0.10f,
                0.10f,
                0.10f,
                100);

            this.orderService.RecalculateBasketItemPrices(basketItem);

            Assert.That(basketItem.FinalPriceAfterDiscount, Is.EqualTo(145.79f));
        }

        [Test]
        public void CalculateBasketTotalSum_WhenBasketItemsExist_ReturnsTotalBeforeDiscount()
        {
            var firstBasketItem = new BasketItemViewModel(1, "image", "First", "Producer", 1, 0, 0, 0, 10);
            firstBasketItem.SetFinalPrices(10, 8);

            var secondBasketItem = new BasketItemViewModel(2, "image", "Second", "Producer", 1, 0, 0, 0, 20);
            secondBasketItem.SetFinalPrices(20, 18);

            var basketTotal = this.orderService.CalculateBasketTotalSum(new List<BasketItemViewModel> { firstBasketItem, secondBasketItem });

            Assert.That(basketTotal.Item1, Is.EqualTo(30));
        }

        [Test]
        public void PlaceOrderFromBasket_WhenRequestedQuantityIsGreaterThanAvailableQuantity_ThrowsArgumentException()
        {
            var pickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3));
            var unavailableItem = CreateItem(10, "Paracetamol", 1, 10);
            this.activeUser.AddItemToBasket(10, 5, 0);

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetItemById(10))
                .Returns(unavailableItem);

            Assert.Throws<ArgumentException>(() => this.orderService.PlaceOrderFromBasket(pickupDate));
        }

        [Test]
        public void PlaceOrderFromBasket_WhenBasketIsValid_ClearsActiveUserBasket()
        {
            var pickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3));
            var availableItem = CreateItem(10, "Paracetamol", 10, 10);
            this.activeUser.AddItemToBasket(10, 2, 0);

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetItemById(10))
                .Returns(availableItem);

            this.mockOrdersRepository
                .Setup(ordersRepository => ordersRepository.AddOrder(this.activeUser.Id, pickupDate, false, false))
                .Returns(55);

            this.orderService.PlaceOrderFromBasket(pickupDate);

            Assert.That(this.activeUser.Basket.Count, Is.EqualTo(0));
        }

        [Test]
        public void CancelOrder_WhenOrderExists_MarksOrderAsExpired()
        {
            var orderToCancel = new Order(12, this.activeUser, DateOnly.FromDateTime(DateTime.Today.AddDays(3)), false, false);

            this.mockOrdersRepository
                .Setup(ordersRepository => ordersRepository.GetOrder(12))
                .Returns(orderToCancel);

            this.orderService.CancelOrder(12);

            Assert.That(orderToCancel.IsExpired, Is.True);
        }

        [Test]
        public void ExpireOverdueOrders_WhenOrderIsOverdue_UpdatesOrderAsExpired()
        {
            var overdueOrder = new Order(
                12,
                this.activeUser,
                DateOnly.FromDateTime(DateTime.Today.AddDays(-10)),
                false,
                false);

            this.mockOrdersRepository
                .Setup(ordersRepository => ordersRepository.GetAllOrders())
                .Returns(new List<Order> { overdueOrder });

            this.orderService.ExpireOverdueOrders();

            this.mockOrdersRepository.Verify(
                ordersRepository => ordersRepository.UpdateOrder(
                    It.Is<Order>(updatedOrder => updatedOrder.IsExpired)),
                Times.Once);
        }

        private static User CreateUser(int userIdentifier)
        {
            return new User(userIdentifier, "user@test.com", "1234567890", "hashedPassword", false, false, "testUser", false, 0);
        }

        private static Item CreateItem(int itemIdentifier, string itemName, int quantity, float price)
        {
            return new Item
            {
                Id = itemIdentifier,
                Name = itemName,
                Producer = "Producer",
                Category = "Category",
                Price = price,
                Quantity = quantity,
                DiscountPercentage = 0,
                NumberOfPills = 20,
                ActiveSubstances = new Dictionary<string, float>(),
                Batches = new Dictionary<DateOnly, int>
                {
                    { DateOnly.FromDateTime(DateTime.Today.AddDays(20)), quantity },
                },
            };
        }
    }
}