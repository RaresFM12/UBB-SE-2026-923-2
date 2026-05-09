namespace UBB_SE_2026_923_2.Tests.Services
{
    using Moq;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class BasketServiceTests
    {
        private Mock<IOrderService> mockOrderService;
        private BasketService basketService;

        [SetUp]
        public void Setup()
        {
            this.mockOrderService = new Mock<IOrderService>();
            this.basketService = new BasketService(this.mockOrderService.Object);
        }

        [Test]
        public void AddToBasket_CallsOrderServiceWithCorrectParameters()
        {
            // Arrange
            int itemId = 101;
            int quantity = 5;
            float discount = 0.1f;

            // Act
            this.basketService.AddToBasket(itemId, quantity, discount);

            // Assert: Verificăm dacă metoda din OrderService a fost apelată exact o dată cu aceleași date
            this.mockOrderService.Verify(s => s.AddItemToBasket(itemId, quantity, discount), Times.Once);
        }

        [Test]
        public void AddToBasket_WithDefaultDiscount_CallsOrderServiceWithZero()
        {
            // Act
            this.basketService.AddToBasket(1, 1);

            // Assert: Verificăm că parametrul opțional (0f) a fost transmis corect
            this.mockOrderService.Verify(s => s.AddItemToBasket(1, 1, 0f), Times.Once);
        }
    }
}