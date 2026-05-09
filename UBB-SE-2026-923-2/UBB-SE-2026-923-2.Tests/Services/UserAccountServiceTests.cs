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
    public class UserAccountServiceLogicTests
    {
        private Mock<IUsersRepository> mockUsersRepository;
        private Mock<ISecurityService> mockSecurityService;
        private Mock<IUserValidationService> mockUserValidationService;
        private UserAccountService userAccountService;

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

        [Test]
        public void Login_WhenEmailIsEmpty_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => this.userAccountService.Login(string.Empty, "Password123!"));
        }

        [Test]
        public void Login_WhenPasswordIsIncorrect_ThrowsException()
        {
            var existingUser = CreateUser(1, "user@test.com", false, false);

            this.mockUserValidationService
                .Setup(userValidationService => userValidationService.IsCorrectEmailFormat("user@test.com"))
                .Returns(true);

            this.mockUsersRepository
                .Setup(usersRepository => usersRepository.GetUserByEmail("user@test.com"))
                .Returns(existingUser);

            this.mockSecurityService
                .Setup(securityService => securityService.VerifyPassword("WrongPassword123!", existingUser.PasswordHash))
                .Returns(false);

            Assert.Throws<Exception>(() => this.userAccountService.Login("user@test.com", "WrongPassword123!"));
        }

        [Test]
        public void Login_WhenCredentialsAreValid_SetsCurrentUser()
        {
            var existingUser = CreateUser(1, "user@test.com", false, false);

            this.mockUserValidationService
                .Setup(userValidationService => userValidationService.IsCorrectEmailFormat("user@test.com"))
                .Returns(true);

            this.mockUsersRepository
                .Setup(usersRepository => usersRepository.GetUserByEmail("user@test.com"))
                .Returns(existingUser);

            this.mockSecurityService
                .Setup(securityService => securityService.VerifyPassword("Password123!", existingUser.PasswordHash))
                .Returns(true);

            this.userAccountService.Login("user@test.com", "Password123!");

            Assert.That(this.userAccountService.CurrentUser, Is.EqualTo(existingUser));
        }

        [Test]
        public void Register_WhenPasswordsDoNotMatch_ThrowsException()
        {
            this.mockUserValidationService
                .Setup(userValidationService => userValidationService.IsCorrectEmailFormat("user@test.com"))
                .Returns(true);

            Assert.Throws<Exception>(
                () => this.userAccountService.Register("user@test.com", "Password123!", "Different123!", "username", "0744123456"));
        }

        [Test]
        public void Register_WhenDataIsValid_AddsUserToRepository()
        {
            this.mockUserValidationService
                .Setup(userValidationService => userValidationService.IsCorrectEmailFormat("user@test.com"))
                .Returns(true);

            this.mockUserValidationService
                .Setup(userValidationService => userValidationService.IsCorrectPasswordFormat("Password123!"))
                .Returns(true);

            this.mockUserValidationService
                .Setup(userValidationService => userValidationService.IsCorrectUsernameFormat("username"))
                .Returns(true);

            this.mockUserValidationService
                .Setup(userValidationService => userValidationService.IsCorrectPhoneNumberFormat("0744123456"))
                .Returns(true);

            this.mockSecurityService
                .Setup(securityService => securityService.HashPassword("Password123!"))
                .Returns("hashedPassword");

            this.mockUsersRepository
                .SetupSequence(usersRepository => usersRepository.GetUserByEmail("user@test.com"))
                .Returns((User)null)
                .Returns(CreateUser(1, "user@test.com", false, false));

            this.userAccountService.Register("user@test.com", "Password123!", "Password123!", "username", "0744123456");

            this.mockUsersRepository.Verify(
                usersRepository => usersRepository.AddUser("user@test.com", "0744123456", "hashedPassword", "username", false, false, false, 0, "Client"),
                Times.Once);
        }

        [Test]
        public void UpdateProfile_WhenCurrentUserIsNotLoggedIn_ThrowsException()
        {
            Assert.Throws<Exception>(() => this.userAccountService.UpdateProfile("newUsername", "0744123456"));
        }

        [Test]
        public void UpdateProfile_WhenNewUsernameIsEmpty_UsesEmailPrefixAsUsername()
        {
            var existingUser = LoginAsUser(false);

            this.userAccountService.UpdateProfile(string.Empty, "0744123456");

            Assert.That(existingUser.Username, Is.EqualTo("admin"));
        }

        [Test]
        public void ChangePassword_WhenOldPasswordIsIncorrect_ThrowsException()
        {
            var existingUser = LoginAsUser(false);

            this.mockSecurityService
                .Setup(securityService => securityService.VerifyPassword("WrongPassword123!", existingUser.PasswordHash))
                .Returns(false);

            Assert.Throws<Exception>(() => this.userAccountService.ChangePassword("WrongPassword123!", "NewPassword123!", "NewPassword123!"));
        }

        [Test]
        public void ChangePassword_WhenDataIsValid_UpdatesCurrentUserPasswordHash()
        {
            var existingUser = LoginAsUser(false);

            this.mockSecurityService
                .Setup(securityService => securityService.VerifyPassword("OldPassword123!", existingUser.PasswordHash))
                .Returns(true);

            this.mockUserValidationService
                .Setup(userValidationService => userValidationService.IsCorrectPasswordFormat("NewPassword123!"))
                .Returns(true);

            this.mockSecurityService
                .Setup(securityService => securityService.HashPassword("NewPassword123!"))
                .Returns("newHashedPassword");

            this.userAccountService.ChangePassword("OldPassword123!", "NewPassword123!", "NewPassword123!");

            Assert.That(existingUser.PasswordHash, Is.EqualTo("newHashedPassword"));
        }

        [Test]
        public void SearchUsers_WhenQueryUsesIdentifierPrefix_ReturnsUserWithRequestedIdentifier()
        {
            LoginAsUser(true);

            this.mockUsersRepository
                .Setup(usersRepository => usersRepository.GetAllUsers())
                .Returns(new List<User>
                {
                    CreateUser(1, "first@test.com", false, false),
                    CreateUser(2, "second@test.com", false, false),
                });

            var searchedUsers = this.userAccountService.SearchUsers("id:2");

            Assert.That(searchedUsers[0].Id, Is.EqualTo(2));
        }

        [Test]
        public void SearchUsers_WhenCurrentUserIsNotAdmin_ThrowsException()
        {
            LoginAsUser(false);

            Assert.Throws<Exception>(() => this.userAccountService.SearchUsers("id:1"));
        }

        [Test]
        public void PromoteToAdmin_WhenClientIsActiveClient_UpdatesUserAsAdmin()
        {
            LoginAsUser(true);
            var clientUser = CreateUser(2, "client@test.com", false, false);

            this.userAccountService.PromoteToAdmin(clientUser);

            Assert.That(clientUser.IsAdmin, Is.True);
        }

        [Test]
        public void DisableAccount_WhenClientIsActiveClient_UpdatesUserAsDisabled()
        {
            LoginAsUser(true);
            var clientUser = CreateUser(2, "client@test.com", false, false);

            this.userAccountService.DisableAccount(clientUser);

            Assert.That(clientUser.IsDisabled, Is.True);
        }

        [Test]
        public void Logout_WhenCurrentUserIsLoggedIn_ClearsCurrentUser()
        {
            LoginAsUser(false);

            this.userAccountService.Logout();

            Assert.That(this.userAccountService.CurrentUser, Is.Null);
        }

        private User LoginAsUser(bool isAdmin)
        {
            var existingUser = CreateUser(1, "admin@test.com", isAdmin, false);

            this.mockUserValidationService
                .Setup(userValidationService => userValidationService.IsCorrectEmailFormat("admin@test.com"))
                .Returns(true);

            this.mockUsersRepository
                .Setup(usersRepository => usersRepository.GetUserByEmail("admin@test.com"))
                .Returns(existingUser);

            this.mockSecurityService
                .Setup(securityService => securityService.VerifyPassword("Password123!", existingUser.PasswordHash))
                .Returns(true);

            this.userAccountService.Login("admin@test.com", "Password123!");
            return existingUser;
        }

        private static User CreateUser(int userIdentifier, string email, bool isAdmin, bool isDisabled)
        {
            return new User(userIdentifier, email, "0744123456", "hashedPassword", isAdmin, isDisabled, "username", false, 0);
        }
    }
}