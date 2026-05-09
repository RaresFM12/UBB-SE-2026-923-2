namespace UBB_SE_2026_923_2.Tests.Services
{
    using System;
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class ServiceWrapperTests
    {
        private Mock<IServiceProvider> mockServiceProvider;
        private Mock<IUsersRepository> mockUserRepository;

        [SetUp]
        public void Setup()
        {
            this.mockServiceProvider = new Mock<IServiceProvider>();
            this.mockUserRepository = new Mock<IUsersRepository>();

            this.mockServiceProvider
                .Setup(x => x.GetService(typeof(IUsersRepository)))
                .Returns(this.mockUserRepository.Object);

            // MAGICĂ: Căutăm clasa App în asamblajul proiectului tău principal
            // Înlocuiește "UBB_SE_2026_923_2" cu namespace-ul tău principal dacă diferă
            Assembly mainAssembly = typeof(ServiceWrapper).Assembly;
            Type appType = mainAssembly.GetType("UBB_SE_2026_923_2.App");

            if (appType != null)
            {
                PropertyInfo prop = appType.GetProperty("Services", BindingFlags.Public | BindingFlags.Static);
                if (prop != null)
                {
                    prop.SetValue(null, this.mockServiceProvider.Object);
                }
            }
        }

        [Test]
        public void Initialize_SuccessfullyCreatesUserAccountService_WithoutAppReference()
        {
            // Act
            ServiceWrapper.Initialize();

            // Assert
            Assert.That(ServiceWrapper.UserAccountService, Is.Not.Null);
            Assert.That(ServiceWrapper.UserAccountService.UsersRepository, Is.EqualTo(this.mockUserRepository.Object));
        }
    }
}