namespace UBB_SE_2026_923_2.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Moq;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class UserAccountServiceTests
    {
        private Mock<IUsersRepository> mockUsersRepository;
        private Mock<ISecurityService> mockSecurityService;
        private Mock<IUserValidationService> mockUserValidationService;
        private UserAccountService userAccountService;

        private static User CreateUser(int id = 1, string email = "t@t.com", string hash = "h", bool isAdmin = false, bool isDisabled = false, string username = "user")
        {
            return new User(id, email, "0700000000", hash, isAdmin, isDisabled, username, false, 0);
        }

        private void SetLoggedInUser(User? user)
        {
            var property = typeof(UserAccountService).GetProperty("CurrentUser", BindingFlags.Public | BindingFlags.Instance);
            property?.SetValue(this.userAccountService, user);
        }

        [SetUp]
        public void Setup()
        {
            this.mockUsersRepository = new Mock<IUsersRepository>();
            this.mockSecurityService = new Mock<ISecurityService>();
            this.mockUserValidationService = new Mock<IUserValidationService>();
            this.userAccountService = new UserAccountService(
                this.mockUsersRepository.Object,
                this.mockSecurityService.Object,
                this.mockUserValidationService.Object);
        }

        private void LoginAs(User user)
        {
            this.mockUserValidationService.Setup(v => v.IsCorrectEmailFormat(user.Email)).Returns(true);
            this.mockUsersRepository.Setup(r => r.GetUserByEmail(user.Email)).Returns(user);
            this.mockSecurityService.Setup(s => s.VerifyPassword(It.IsAny<string>(), user.PasswordHash)).Returns(true);
            this.userAccountService.Login(user.Email, "any_password");
        }

        [Test]
        public void Login_ValidCredentials_SetsCurrentUser()
        {
            var user = CreateUser(email: "paul@gmail.com", hash: "abc123");
            this.mockUserValidationService.Setup(v => v.IsCorrectEmailFormat("paul@gmail.com")).Returns(true);
            this.mockUsersRepository.Setup(r => r.GetUserByEmail("paul@gmail.com")).Returns(user);
            this.mockSecurityService.Setup(s => s.VerifyPassword("abc123", "abc123")).Returns(true);

            this.userAccountService.Login("paul@gmail.com", "abc123");

            Assert.That(this.userAccountService.CurrentUser, Is.EqualTo(user));
        }

        [Test]
        [TestCase("invalid", "Not a valid e-mail")]
        [TestCase("disabled", "Account disabled")]
        [TestCase("wrongpass", "Incorrect password")]
        public void Login_FailedScenarios_ThrowsException(string scenario, string expectedMessage)
        {
            var user = CreateUser(isDisabled: scenario == "disabled");
            this.mockUserValidationService.Setup(v => v.IsCorrectEmailFormat(It.IsAny<string>())).Returns(scenario != "invalid");
            this.mockUsersRepository.Setup(r => r.GetUserByEmail(It.IsAny<string>())).Returns(user);
            this.mockSecurityService.Setup(s => s.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(scenario != "wrongpass");

            var ex = Assert.Throws<Exception>(() => this.userAccountService.Login("test@test.com", "pass"));
            Assert.That(ex.Message, Is.EqualTo(expectedMessage));
        }

        [Test]
        public void Register_ValidData_AddsToRepository()
        {
            // CONFIGURARE: Acum am adăugat și validarea pentru telefon!
            this.mockUserValidationService.Setup(v => v.IsCorrectEmailFormat(It.IsAny<string>())).Returns(true);
            this.mockUserValidationService.Setup(v => v.IsCorrectPasswordFormat(It.IsAny<string>())).Returns(true);
            this.mockUserValidationService.Setup(v => v.IsCorrectUsernameFormat(It.IsAny<string>())).Returns(true);
            this.mockUserValidationService.Setup(v => v.IsCorrectPhoneNumberFormat(It.IsAny<string>())).Returns(true); // <--- FIX
            this.mockUsersRepository.Setup(r => r.GetUserByEmail("new@test.com")).Returns((User?)null);

            this.userAccountService.Register("new@test.com", "Pass123!", "Pass123!", "user", "0700");

            this.mockUsersRepository.Verify(r => r.AddUser(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void UpdateProfile_ValidData_UpdatesUser()
        {
            var user = CreateUser();
            this.LoginAs(user);
            this.mockUserValidationService.Setup(v => v.IsCorrectUsernameFormat(It.IsAny<string>())).Returns(true);
            this.mockUserValidationService.Setup(v => v.IsCorrectPhoneNumberFormat(It.IsAny<string>())).Returns(true);

            this.userAccountService.UpdateProfile("newname", "0799");

            Assert.That(user.Username, Is.EqualTo("newname"));
            this.mockUsersRepository.Verify(r => r.UpdateUser(user), Times.Once);
        }

        [Test]
        public void ChangePassword_ValidData_UpdatesHash()
        {
            var user = CreateUser(hash: "old");
            this.LoginAs(user);
            this.mockSecurityService.Setup(s => s.VerifyPassword(It.IsAny<string>(), "old")).Returns(true);
            this.mockUserValidationService.Setup(v => v.IsCorrectPasswordFormat(It.IsAny<string>())).Returns(true);
            this.mockSecurityService.Setup(s => s.HashPassword("new")).Returns("newhash");

            this.userAccountService.ChangePassword("old", "new", "new");

            Assert.That(user.PasswordHash, Is.EqualTo("newhash"));
        }

        [Test]
        public void SearchUsers_AsAdmin_ReturnsFilteredResults()
        {
            var admin = CreateUser(isAdmin: true);
            this.LoginAs(admin);
            var users = new List<User> { admin, CreateUser(id: 99, username: "target") };
            this.mockUsersRepository.Setup(r => r.GetAllUsers()).Returns(users);

            var result = this.userAccountService.SearchUsers("id:99");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(99));
        }

        [Test]
        public void PromoteToAdmin_ValidAdmin_UpdatesClient()
        {
            var admin = CreateUser(isAdmin: true);
            var client = CreateUser(id: 2, isAdmin: false);
            this.LoginAs(admin);

            this.userAccountService.PromoteToAdmin(client);

            Assert.That(client.IsAdmin, Is.True);
            this.mockUsersRepository.Verify(r => r.UpdateUser(client), Times.Once);
        }
    }
}