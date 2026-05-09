namespace UBB_SE_2026_923_2.Tests.Services
{
    using Moq;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class BasketServiceTests
    {
        private Mock<IOrderService> mockOrderService;
        private BasketService service;

        [SetUp]
        public void Setup()
        {
            this.mockOrderService = new Mock<IOrderService>();
            this.service = new BasketService(this.mockOrderService.Object);
        }

        [Test]
        public void AddToBasket_ValidInput_DelegatesToOrderService()
        {
            this.service.AddToBasket(1, 5, 0.1f);

            this.mockOrderService.Verify(
                orderService => orderService.AddItemToBasket(1, 5, 0.1f),
                Times.Once);
        }

        [Test]
        public void AddToBasket_DefaultDiscount_PassesZero()
        {
            this.service.AddToBasket(2, 3);

            this.mockOrderService.Verify(
                orderService => orderService.AddItemToBasket(2, 3, 0f),
                Times.Once);
        }
    }
}
