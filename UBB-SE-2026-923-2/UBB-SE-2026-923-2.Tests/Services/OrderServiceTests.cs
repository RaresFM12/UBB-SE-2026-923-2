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
    using UBB_SE_2026_923_2.ViewModels.Orders;

    [TestFixture]
    public class OrderServiceTests
    {
        private Mock<ISubstancesRepository> mockSubstancesRepo;
        private Mock<IItemsRepository> mockItemsRepo;
        private Mock<IUsersRepository> mockUsersRepo;
        private Mock<IOrdersRepository> mockOrdersRepo;
        private Mock<IEvaluationsRepository> mockEvaluationsRepo;
        private User activeUser;
        private OrderService service;

        [SetUp]
        public void Setup()
        {
            this.mockSubstancesRepo = new Mock<ISubstancesRepository>();
            this.mockItemsRepo = new Mock<IItemsRepository>();
            this.mockUsersRepo = new Mock<IUsersRepository>();
            this.mockOrdersRepo = new Mock<IOrdersRepository>();
            this.mockEvaluationsRepo = new Mock<IEvaluationsRepository>();
            this.activeUser = new User(1, "test@test.com", "123", "hash", false, false, "testuser", false, 0);

            this.service = new OrderService(
                this.mockSubstancesRepo.Object,
                this.mockItemsRepo.Object,
                this.mockUsersRepo.Object,
                this.mockOrdersRepo.Object,
                this.activeUser,
                this.mockEvaluationsRepo.Object);
        }

        [Test]
        public void ActiveUser_ReturnsInjectedUser()
        {
            Assert.That(this.service.ActiveUser, Is.SameAs(this.activeUser));
        }

        [Test]
        public void RepositoryProperties_ReturnInjectedRepositories()
        {
            Assert.That(this.service.SubstancesRepository, Is.SameAs(this.mockSubstancesRepo.Object));
            Assert.That(this.service.ItemsRepository, Is.SameAs(this.mockItemsRepo.Object));
            Assert.That(this.service.UsersRepository, Is.SameAs(this.mockUsersRepo.Object));
            Assert.That(this.service.OrdersRepository, Is.SameAs(this.mockOrdersRepo.Object));
        }

        [Test]
        public void AddToBasket_NewItem_AddsToUserBasket()
        {
            this.service.AddToBasket(1, 5);

            Assert.That(this.activeUser.Basket.ContainsKey(1), Is.True);
        }

        [Test]
        public void AddToBasket_NewItem_UsesNoExtraDiscount()
        {
            this.service.AddToBasket(1, 5);

            Assert.That(this.activeUser.Basket[1].ExtraDiscountPercentage, Is.EqualTo(0f));
        }

        [Test]
        public void AddItemToBasket_NewItem_StoresQuantityAndDiscount()
        {
            this.service.AddItemToBasket(1, 3, 0.15f);

            Assert.That(this.activeUser.Basket[1].Quantity, Is.EqualTo(3));
            Assert.That(this.activeUser.Basket[1].ExtraDiscountPercentage, Is.EqualTo(0.15f));
        }

        [Test]
        public void AddItemToBasket_ExistingItem_IncreasesQuantity()
        {
            this.service.AddItemToBasket(1, 3, 0f);
            this.service.AddItemToBasket(1, 2, 0f);

            Assert.That(this.activeUser.Basket[1].Quantity, Is.EqualTo(5));
        }

        [Test]
        public void AddItemToBasket_ExistingItemHigherDiscount_UpdatesDiscount()
        {
            this.service.AddItemToBasket(1, 3, 0.1f);
            this.service.AddItemToBasket(1, 2, 0.2f);

            Assert.That(this.activeUser.Basket[1].ExtraDiscountPercentage, Is.EqualTo(0.2f));
        }

        [Test]
        public void AddItemToBasket_ExistingItemLowerDiscount_KeepsHigherDiscount()
        {
            this.service.AddItemToBasket(1, 3, 0.3f);
            this.service.AddItemToBasket(1, 2, 0.1f);

            Assert.That(this.activeUser.Basket[1].ExtraDiscountPercentage, Is.EqualTo(0.3f));
        }

        [Test]
        public void UpdateBasketItemQuantity_PositiveQuantity_Updates()
        {
            this.activeUser.AddItemToBasket(1, 5);

            this.service.UpdateBasketItemQuantity(1, 10);

            Assert.That(this.activeUser.Basket[1].Quantity, Is.EqualTo(10));
        }

        [Test]
        public void UpdateBasketItemQuantity_ZeroQuantity_RemovesItem()
        {
            this.activeUser.AddItemToBasket(1, 5);

            this.service.UpdateBasketItemQuantity(1, 0);

            Assert.That(this.activeUser.Basket.ContainsKey(1), Is.False);
        }

        [Test]
        public void RemoveFromBasket_ExistingItem_RemovesIt()
        {
            this.activeUser.AddItemToBasket(1, 5);

            this.service.RemoveFromBasket(1);

            Assert.That(this.activeUser.Basket.ContainsKey(1), Is.False);
        }

        [Test]
        public void RemoveFromBasket_MissingItem_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => this.service.RemoveFromBasket(1));
        }

        [Test]
        public void GetBasketItems_EmptyBasket_ReturnsEmpty()
        {
            var result = this.service.GetBasketItems();

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetBasketItems_WithItems_ReturnsViewModels()
        {
            var item = new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20 , Quantity = 100 };
            item.Batches = new Dictionary<DateOnly, int> { { DateOnly.FromDateTime(DateTime.Now.AddDays(30)), 100 } };
            this.mockItemsRepo.Setup(repository => repository.GetItemById(1)).Returns(item);
            this.activeUser.AddItemToBasket(1, 2);

            var result = this.service.GetBasketItems();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].ItemName, Is.EqualTo("Aspirin"));
        }

        [Test]
        public void GetBasketItems_RepositoryThrows_RemovesInvalidBasketItem()
        {
            this.mockItemsRepo.Setup(repository => repository.GetItemById(1)).Throws<ArgumentException>();
            this.activeUser.AddItemToBasket(1, 2);

            var result = this.service.GetBasketItems();

            Assert.That(result.Count, Is.EqualTo(0));
            Assert.That(this.activeUser.Basket.ContainsKey(1), Is.False);
        }

        [Test]
        public void RecalculateBasketItemPrices_NoDiscount_SetsCorrectPrices()
        {
            var basketItem = new BasketItemViewModel(1, "img", "Aspirin", "Bayer", 2, 0f, 0f, 0f, 10f);

            this.service.RecalculateBasketItemPrices(basketItem);

            Assert.That(basketItem.FinalPriceBeforeDiscount, Is.EqualTo(20f));
            Assert.That(basketItem.FinalPriceAfterDiscount, Is.EqualTo(20f));
        }

        [Test]
        public void RecalculateBasketItemPrices_MultipleDiscounts_RoundsDownToTwoDecimals()
        {
            var basketItem = new BasketItemViewModel(1, "img", "Aspirin", "Bayer", 3, 0.1f, 0.2f, 0.05f, 19.999f);

            this.service.RecalculateBasketItemPrices(basketItem);

            Assert.That(basketItem.FinalPriceBeforeDiscount, Is.EqualTo(59.99f));
            Assert.That(basketItem.FinalPriceAfterDiscount, Is.EqualTo(41.03f));
        }

        [Test]
        public void CalculateBasketTotalSum_MultipleItems_ReturnsTotals()
        {
            var item1 = new BasketItemViewModel(1, "img", "A", "P", 1, 0f, 0f, 0f, 10f);
            var item2 = new BasketItemViewModel(2, "img", "B", "P", 1, 0f, 0f, 0f, 20f);
            item1.SetFinalPrices(10f, 9f);
            item2.SetFinalPrices(20f, 18f);

            var result = this.service.CalculateBasketTotalSum(new[] { item1, item2 });

            Assert.That(result.Item1, Is.EqualTo(30f));
        }

        [Test]
        public void CalculateBasketTotalSum_EmptyItems_ReturnsZeroTotals()
        {
            var result = this.service.CalculateBasketTotalSum(Array.Empty<BasketItemViewModel>());

            Assert.That(result.Item1, Is.EqualTo(0f));
            Assert.That(result.Item2, Is.EqualTo(0f));
        }

        [Test]
        public void CancelOrder_SetsOrderAsExpired()
        {
            var order = new Order(1, this.activeUser, DateOnly.FromDateTime(DateTime.Now.AddDays(5)));
            this.mockOrdersRepo.Setup(repository => repository.GetOrder(1)).Returns(order);

            this.service.CancelOrder(1);

            Assert.That(order.IsExpired, Is.True);
            this.mockOrdersRepo.Verify(repository => repository.UpdateOrder(order), Times.Once);
        }

        [Test]
        public void CancelOrder_CompletedOrder_StillMarksExpired()
        {
            var order = new Order(1, this.activeUser, DateOnly.FromDateTime(DateTime.Now.AddDays(5)), true);
            this.mockOrdersRepo.Setup(repository => repository.GetOrder(1)).Returns(order);

            this.service.CancelOrder(1);

            Assert.That(order.IsExpired, Is.True);
            this.mockOrdersRepo.Verify(repository => repository.UpdateOrder(order), Times.Once);
        }

        [Test]
        public void ExpireOverdueOrders_OverdueOrder_MarksExpired()
        {
            var overdueOrder = new Order(1, this.activeUser, DateOnly.FromDateTime(DateTime.Now.AddDays(-10)));
            this.mockOrdersRepo.Setup(repository => repository.GetAllOrders()).Returns(new List<Order> { overdueOrder });

            this.service.ExpireOverdueOrders();

            Assert.That(overdueOrder.IsExpired, Is.True);
            this.mockOrdersRepo.Verify(repository => repository.UpdateOrder(overdueOrder), Times.Once);
        }

        [Test]
        public void ExpireOverdueOrders_FutureOrder_NotMarkedExpired()
        {
            var futureOrder = new Order(1, this.activeUser, DateOnly.FromDateTime(DateTime.Now.AddDays(10)));
            this.mockOrdersRepo.Setup(repository => repository.GetAllOrders()).Returns(new List<Order> { futureOrder });

            this.service.ExpireOverdueOrders();

            Assert.That(futureOrder.IsExpired, Is.False);
        }

        [Test]
        public void PlaceOrderFromBasket_ValidBasket_CreatesOrderAndClearsBasket()
        {
            var futureDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            var item = new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20 , Quantity = 100 };
            item.Batches = new Dictionary<DateOnly, int> { { futureDate.AddDays(30), 100 } };
            this.mockItemsRepo.Setup(repository => repository.GetItemById(1)).Returns(item);
            this.mockOrdersRepo.Setup(repository => repository.AddOrder(It.IsAny<int>(), It.IsAny<DateOnly>(), false, false)).Returns(99);
            this.activeUser.AddItemToBasket(1, 2);

            this.service.PlaceOrderFromBasket(futureDate);

            Assert.That(this.activeUser.Basket.Count, Is.EqualTo(0));
            this.mockOrdersRepo.Verify(repository => repository.AddOrder(this.activeUser.Id, futureDate, false, false), Times.Once);
        }

        [Test]
        public void PlaceOrderFromBasket_DiscountsApplied_StoresDiscountedFinalPrice()
        {
            var futureDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            var item = new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 100f, NumberOfPills = 20, Quantity = 100, DiscountPercentage = 10f };
            item.Batches = new Dictionary<DateOnly, int> { { futureDate.AddDays(30), 100 } };
            this.mockItemsRepo.Setup(repository => repository.GetItemById(1)).Returns(item);
            this.mockOrdersRepo.Setup(repository => repository.AddOrder(It.IsAny<int>(), It.IsAny<DateOnly>(), false, false)).Returns(99);
            this.activeUser.AddUserDiscount(1, 0.2f);
            this.activeUser.AddItemToBasket(1, 2, 30f);

            this.service.PlaceOrderFromBasket(futureDate);

            this.mockOrdersRepo.Verify(
                repository => repository.UpdateOrder(It.Is<Order>(order =>
                    order.ItemQuantitiesWithFinalPrice[1].Item1 == 2 &&
                    order.ItemQuantitiesWithFinalPrice[1].Item2 == 100.8f)),
                Times.Once);
        }

        [Test]
        public void PlaceOrderFromBasket_InsufficientStock_ThrowsArgumentException()
        {
            var futureDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            var item = new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20 , Quantity = 1 };
            item.Batches = new Dictionary<DateOnly, int> { { futureDate.AddDays(30), 1 } };
            this.mockItemsRepo.Setup(repository => repository.GetItemById(1)).Returns(item);
            this.activeUser.AddItemToBasket(1, 999);

            Assert.Throws<ArgumentException>(() => this.service.PlaceOrderFromBasket(futureDate));
        }

        [Test]
        public void CompleteOrder_ValidOrder_MarksCompleted()
        {
            var order = new Order(1, this.activeUser, DateOnly.FromDateTime(DateTime.Now.AddDays(5)));
            order.AddItemToOrder(1, 2, 20f);
            this.mockOrdersRepo.Setup(repository => repository.GetOrder(1)).Returns(order);

            var item = new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20 , Quantity = 10 };
            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            item.Batches = new Dictionary<DateOnly, int> { { currentDate.AddDays(30), 10 } };
            this.mockItemsRepo.Setup(repository => repository.GetItemById(1)).Returns(item);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, Tuple.Create(2, 20f) },
            };

            this.service.CompleteOrder(1, updatedQuantities);

            Assert.That(order.IsCompleted, Is.True);
            this.mockOrdersRepo.Verify(repository => repository.UpdateOrder(order), Times.Once);
        }

        [Test]
        public void CompleteOrder_ValidOrder_RemovesQuantityFromInventory()
        {
            var order = new Order(1, this.activeUser, DateOnly.FromDateTime(DateTime.Now.AddDays(5)));
            order.AddItemToOrder(1, 2, 20f);
            this.mockOrdersRepo.Setup(repository => repository.GetOrder(1)).Returns(order);

            var item = new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20, Quantity = 10 };
            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            item.Batches = new Dictionary<DateOnly, int> { { currentDate.AddDays(30), 10 } };
            this.mockItemsRepo.Setup(repository => repository.GetItemById(1)).Returns(item);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, Tuple.Create(2, 20f) },
            };

            this.service.CompleteOrder(1, updatedQuantities);

            Assert.That(item.Quantity, Is.EqualTo(8));
            this.mockItemsRepo.Verify(repository => repository.UpdateItemById(item), Times.Once);
        }

        [Test]
        public void CompleteOrder_InsufficientStock_ThrowsArgumentException()
        {
            var order = new Order(1, this.activeUser, DateOnly.FromDateTime(DateTime.Now.AddDays(5)));
            order.AddItemToOrder(1, 2, 20f);
            this.mockOrdersRepo.Setup(repository => repository.GetOrder(1)).Returns(order);

            var item = new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20, Quantity = 1 };
            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            item.Batches = new Dictionary<DateOnly, int> { { currentDate.AddDays(30), 1 } };
            this.mockItemsRepo.Setup(repository => repository.GetItemById(1)).Returns(item);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, Tuple.Create(2, 20f) },
            };

            Assert.Throws<ArgumentException>(() => this.service.CompleteOrder(1, updatedQuantities));
            this.mockOrdersRepo.Verify(repository => repository.UpdateOrder(It.IsAny<Order>()), Times.Never);
        }

        [Test]
        public void ModifyIncompleteOrder_ValidData_UpdatesOrder()
        {
            var futureDate = DateOnly.FromDateTime(DateTime.Now.AddDays(10));
            var order = new Order(1, this.activeUser, DateOnly.FromDateTime(DateTime.Now.AddDays(5)));
            order.AddItemToOrder(1, 2, 20f);
            this.mockOrdersRepo.Setup(repository => repository.GetOrder(1)).Returns(order);

            var item = new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20 , Quantity = 10 };
            item.Batches = new Dictionary<DateOnly, int> { { futureDate.AddDays(30), 10 } };
            this.mockItemsRepo.Setup(repository => repository.GetItemById(1)).Returns(item);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, Tuple.Create(3, 30f) },
            };

            this.service.ModifyIncompleteOrder(1, updatedQuantities, futureDate);

            Assert.That(order.PickUpDate, Is.EqualTo(futureDate));
            this.mockOrdersRepo.Verify(repository => repository.UpdateOrder(order), Times.Once);
        }

        [Test]
        public void ModifyIncompleteOrder_PastDate_ThrowsArgumentException()
        {
            var pastDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
            var order = new Order(1, this.activeUser, DateOnly.FromDateTime(DateTime.Now.AddDays(5)));
            this.mockOrdersRepo.Setup(repository => repository.GetOrder(1)).Returns(order);

            Assert.Throws<ArgumentException>(
                () => this.service.ModifyIncompleteOrder(1, new Dictionary<int, Tuple<int, float>>(), pastDate));
        }

        [Test]
        public void ModifyIncompleteOrder_InsufficientStock_ThrowsArgumentException()
        {
            var futureDate = DateOnly.FromDateTime(DateTime.Now.AddDays(10));
            var order = new Order(1, this.activeUser, DateOnly.FromDateTime(DateTime.Now.AddDays(5)));
            this.mockOrdersRepo.Setup(repository => repository.GetOrder(1)).Returns(order);

            var item = new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20, Quantity = 1 };
            item.Batches = new Dictionary<DateOnly, int> { { futureDate.AddDays(30), 1 } };
            this.mockItemsRepo.Setup(repository => repository.GetItemById(1)).Returns(item);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, Tuple.Create(2, 20f) },
            };

            Assert.Throws<ArgumentException>(() => this.service.ModifyIncompleteOrder(1, updatedQuantities, futureDate));
            this.mockOrdersRepo.Verify(repository => repository.UpdateOrder(It.IsAny<Order>()), Times.Never);
        }

        [Test]
        public void ResubmitExpiredOrder_ValidData_CreatesNewOrder()
        {
            var futureDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            var expiredOrder = new Order(1, this.activeUser, DateOnly.FromDateTime(DateTime.Now.AddDays(-5)), false, true);
            expiredOrder.AddItemToOrder(1, 2, 20f);
            this.mockOrdersRepo.Setup(repository => repository.GetOrder(1)).Returns(expiredOrder);
            this.mockOrdersRepo.Setup(repository => repository.AddOrder(It.IsAny<int>(), It.IsAny<DateOnly>(), false, false)).Returns(2);

            var item = new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20 , Quantity = 10 };
            item.Batches = new Dictionary<DateOnly, int> { { futureDate.AddDays(30), 10 } };
            this.mockItemsRepo.Setup(repository => repository.GetItemById(1)).Returns(item);

            this.service.ResubmitExpiredOrder(1, futureDate);
            this.mockOrdersRepo.Verify(repository => repository.RemoveOrder(1), Times.Once);
            this.mockOrdersRepo.Verify(repository => repository.AddOrder(this.activeUser.Id, futureDate, false, false), Times.Once);
        }

        [Test]
        public void ResubmitExpiredOrder_InsufficientStock_ThrowsArgumentException()
        {
            var futureDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            var expiredOrder = new Order(1, this.activeUser, DateOnly.FromDateTime(DateTime.Now.AddDays(-5)), false, true);
            expiredOrder.AddItemToOrder(1, 2, 20f);
            this.mockOrdersRepo.Setup(repository => repository.GetOrder(1)).Returns(expiredOrder);

            var item = new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20, Quantity = 1 };
            item.Batches = new Dictionary<DateOnly, int> { { futureDate.AddDays(30), 1 } };
            this.mockItemsRepo.Setup(repository => repository.GetItemById(1)).Returns(item);

            Assert.Throws<ArgumentException>(() => this.service.ResubmitExpiredOrder(1, futureDate));
            this.mockOrdersRepo.Verify(repository => repository.RemoveOrder(It.IsAny<int>()), Times.Never);
            this.mockOrdersRepo.Verify(repository => repository.AddOrder(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public void FillBasketFromPrescription_DelegatesToPrescriptionService()
        {
            this.mockEvaluationsRepo.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation>());

            Assert.Throws<ArgumentException>(() => this.service.FillBasketFromPrescription("invalid"));
        }

        [Test]
        public void FillBasketFromPrescription_ValidPrescription_ReturnsMatchingItems()
        {
            var evaluation = new MedicalEvaluation
            {
                EvaluationID = 7,
                MedicationsList = "Aspirin",
            };
            var item = new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20, Quantity = 5 };
            item.ActiveSubstances = new Dictionary<string, float> { { "ASA", 500f } };
            this.mockEvaluationsRepo.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation> { evaluation });
            this.mockItemsRepo.Setup(repository => repository.GetAllItems()).Returns(new List<Item> { item });
            this.mockItemsRepo.Setup(repository => repository.GetItemsByName("Aspirin")).Returns(new List<Item> { item });

            var result = this.service.FillBasketFromPrescription("7");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[1], Is.EqualTo(1));
        }

        [Test]
        public void ApplyPrescriptionToBasket_EmptyResult_ThrowsArgumentException()
        {
            this.mockEvaluationsRepo.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation>());

            Assert.Throws<ArgumentException>(() => this.service.ApplyPrescriptionToBasket("invalid"));
        }

        [Test]
        public void ApplyPrescriptionToBasket_ValidPrescription_AddsItemsToBasket()
        {
            var evaluation = new MedicalEvaluation
            {
                EvaluationID = 7,
                MedicationsList = "Aspirin",
            };
            var item = new Item { Id = 1, Name = "Aspirin", Producer = "Bayer", Category = "Pain", Price = 10f, NumberOfPills = 20, Quantity = 5 };
            item.ActiveSubstances = new Dictionary<string, float> { { "ASA", 500f } };
            this.mockEvaluationsRepo.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation> { evaluation });
            this.mockItemsRepo.Setup(repository => repository.GetAllItems()).Returns(new List<Item> { item });
            this.mockItemsRepo.Setup(repository => repository.GetItemsByName("Aspirin")).Returns(new List<Item> { item });

            this.service.ApplyPrescriptionToBasket("7");

            Assert.That(this.activeUser.Basket.ContainsKey(1), Is.True);
            Assert.That(this.activeUser.Basket[1].Quantity, Is.EqualTo(1));
        }
    }
}
