namespace UBB_SE_2026_923_2.Tests.Services
{
    using Moq;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class PeriodTrackerServiceFactoryTests
    {
        private Mock<IUsersRepository> mockUsersRepo;
        private Mock<IItemsRepository> mockItemsRepo;
        private Mock<RaresICurrentUserService> mockCurrentUserRepo;
        private Mock<IOrderService> mockOrderService;
        private PeriodTrackerServiceFactory factory;

        [SetUp]
        public void Setup()
        {
            this.mockUsersRepo = new Mock<IUsersRepository>();
            this.mockItemsRepo = new Mock<IItemsRepository>();
            this.mockCurrentUserRepo = new Mock<RaresICurrentUserService>();
            this.mockOrderService = new Mock<IOrderService>();

            this.factory = new PeriodTrackerServiceFactory(
                this.mockUsersRepo.Object,
                this.mockItemsRepo.Object,
                this.mockCurrentUserRepo.Object,
                this.mockOrderService.Object);
        }

        [Test]
        public void CreatePeriodTrackerService_ReturnsImplementation()
        {
            // Act
            var service = this.factory.CreatePeriodTrackerService();

            // Assert
            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.InstanceOf<IPeriodTrackerService>());
        }

        [Test]
        public void CreateWellnessItemsService_ReturnsImplementation()
        {
            // Act
            var service = this.factory.CreateWellnessItemsService();

            // Assert
            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.InstanceOf<IWellnessItemsService>());
        }

        [Test]
        public void CreateBasketService_ReturnsImplementation()
        {
            // Act
            var service = this.factory.CreateBasketService();

            // Assert
            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.InstanceOf<IBasketService>());
        }
    }
}