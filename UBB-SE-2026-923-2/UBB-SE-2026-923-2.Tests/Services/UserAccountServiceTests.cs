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
    public class UserAccountServiceTests
    {
        private Mock<IUsersRepository> mockUsersRepository;
        private Mock<ISecurityService> mockSecurityService;
        private Mock<IUserValidationService> mockUserValidationService;
        private UserAccountService userAccountService;

        private static User CreateUser(
            int id = 1,
            string email = "test@test.com",
            string passwordHash = "hashed",
            bool isAdmin = false,
            bool isDisabled = false,
            string username = "testuser")
        {
            return new User(id, email, "0700000000", passwordHash, isAdmin, isDisabled, username, false, 0);
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

        [Test]
        public void Login_ValidCredentials_SetsCurrentUser()
        {
            var user = CreateUser(email: "paul@gmail.com", passwordHash: "abc123");
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("paul@gmail.com")).Returns(true);
            this.mockUsersRepository.Setup(repository => repository.GetUserByEmail("paul@gmail.com")).Returns(user);
            this.mockSecurityService.Setup(service => service.VerifyPassword("abc123", "abc123")).Returns(true);

            this.userAccountService.Login("paul@gmail.com", "abc123");

            Assert.That(this.userAccountService.CurrentUser, Is.EqualTo(user));
        }

        [Test]
        public void Login_EmptyEmail_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => this.userAccountService.Login(string.Empty, "password"));
        }

        [Test]
        public void Login_EmptyPassword_ThrowsArgumentException()
        {
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("test@test.com")).Returns(true);

            Assert.Throws<ArgumentException>(() => this.userAccountService.Login("test@test.com", string.Empty));
        }

        [Test]
        public void Login_InvalidEmailFormat_ThrowsException()
        {
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("invalid")).Returns(false);

            var thrownException = Assert.Throws<Exception>(() => this.userAccountService.Login("invalid", "password"));

            Assert.That(thrownException.Message, Is.EqualTo("Not a valid e-mail"));
        }

        [Test]
        public void Login_EmailNotFound_ThrowsException()
        {
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("unknown@test.com")).Returns(true);
            this.mockUsersRepository.Setup(repository => repository.GetUserByEmail("unknown@test.com")).Returns((User)null!);

            var thrownException = Assert.Throws<Exception>(() => this.userAccountService.Login("unknown@test.com", "password"));

            Assert.That(thrownException.Message, Is.EqualTo("E-mail not found"));
        }

        [Test]
        public void Login_DisabledAccount_ThrowsException()
        {
            var user = CreateUser(isDisabled: true);
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("test@test.com")).Returns(true);
            this.mockUsersRepository.Setup(repository => repository.GetUserByEmail("test@test.com")).Returns(user);

            var thrownException = Assert.Throws<Exception>(() => this.userAccountService.Login("test@test.com", "password"));

            Assert.That(thrownException.Message, Is.EqualTo("Account disabled"));
        }

        [Test]
        public void Login_IncorrectPassword_ThrowsException()
        {
            var user = CreateUser(passwordHash: "hashed");
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("test@test.com")).Returns(true);
            this.mockUsersRepository.Setup(repository => repository.GetUserByEmail("test@test.com")).Returns(user);
            this.mockSecurityService.Setup(service => service.VerifyPassword("wrong", "hashed")).Returns(false);

            var thrownException = Assert.Throws<Exception>(() => this.userAccountService.Login("test@test.com", "wrong"));

            Assert.That(thrownException.Message, Is.EqualTo("Incorrect password"));
        }

        [Test]
        public void Register_InvalidEmailFormat_ThrowsException()
        {
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("bad")).Returns(false);

            Assert.Throws<Exception>(() =>
                this.userAccountService.Register("bad", "Pass1234!", "Pass1234!", "user", "0711111111"));
        }

        [Test]
        public void Register_EmptyPassword_ThrowsException()
        {
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("a@b.c")).Returns(true);

            var thrownException = Assert.Throws<Exception>(() =>
                this.userAccountService.Register("a@b.c", string.Empty, string.Empty, "user", "0711111111"));

            Assert.That(thrownException.Message, Is.EqualTo("Password cannot be empty."));
        }

        [Test]
        public void Register_PasswordsDoNotMatch_ThrowsException()
        {
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("a@b.c")).Returns(true);

            var thrownException = Assert.Throws<Exception>(() =>
                this.userAccountService.Register("a@b.c", "Pass1234!", "Different1!", "user", "0711111111"));

            Assert.That(thrownException.Message, Is.EqualTo("Passwords don't match."));
        }

        [Test]
        public void Register_WeakPassword_ThrowsException()
        {
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("a@b.c")).Returns(true);
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectPasswordFormat("weak")).Returns(false);

            Assert.Throws<Exception>(() =>
                this.userAccountService.Register("a@b.c", "weak", "weak", "user", "0711111111"));
        }

        [Test]
        public void Register_InvalidUsername_ThrowsException()
        {
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("a@b.c")).Returns(true);
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectPasswordFormat("Pass1234!")).Returns(true);
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectUsernameFormat("bad user!")).Returns(false);

            Assert.Throws<Exception>(() =>
                this.userAccountService.Register("a@b.c", "Pass1234!", "Pass1234!", "bad user!", "0711111111"));
        }

        [Test]
        public void Register_InvalidPhoneNumber_ThrowsException()
        {
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("a@b.c")).Returns(true);
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectPasswordFormat("Pass1234!")).Returns(true);
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectUsernameFormat("user")).Returns(true);
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectPhoneNumberFormat("abc")).Returns(false);
            this.mockUsersRepository.Setup(repository => repository.GetUserByEmail("a@b.c")).Returns((User)null!);

            Assert.Throws<Exception>(() =>
                this.userAccountService.Register("a@b.c", "Pass1234!", "Pass1234!", "user", "abc"));
        }

        [Test]
        public void UpdateProfile_NotLoggedIn_ThrowsException()
        {
            Assert.Throws<Exception>(() => this.userAccountService.UpdateProfile("newname", "0711111111"));
        }

        [Test]
        public void UpdateProfile_ValidData_UpdatesUsername()
        {
            var user = CreateUser();
            this.LoginAs(user);
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectUsernameFormat("newname")).Returns(true);
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectPhoneNumberFormat("0799999999")).Returns(true);

            this.userAccountService.UpdateProfile("newname", "0799999999");

            Assert.That(this.userAccountService.CurrentUser!.Username, Is.EqualTo("newname"));
            this.mockUsersRepository.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public void UpdateProfile_InvalidUsername_ThrowsException()
        {
            var user = CreateUser();
            this.LoginAs(user);
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectUsernameFormat("bad!")).Returns(false);

            Assert.Throws<Exception>(() => this.userAccountService.UpdateProfile("bad!", "0711111111"));
        }

        [Test]
        public void ChangePassword_NotLoggedIn_ThrowsException()
        {
            Assert.Throws<Exception>(() =>
                this.userAccountService.ChangePassword("old", "New1234!", "New1234!"));
        }

        [Test]
        public void ChangePassword_IncorrectOldPassword_ThrowsException()
        {
            var user = CreateUser(passwordHash: "oldhash");
            this.LoginAs(user);
            this.mockSecurityService.Setup(service => service.VerifyPassword("wrong", "oldhash")).Returns(false);

            var thrownException = Assert.Throws<Exception>(() =>
                this.userAccountService.ChangePassword("wrong", "New1234!", "New1234!"));

            Assert.That(thrownException.Message, Is.EqualTo("Incorrect password"));
        }

        [Test]
        public void ChangePassword_PasswordsDoNotMatch_ThrowsException()
        {
            var user = CreateUser(passwordHash: "oldhash");
            this.LoginAs(user);
            this.mockSecurityService.Setup(service => service.VerifyPassword("old", "oldhash")).Returns(true);
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectPasswordFormat("New1234!")).Returns(true);

            var thrownException = Assert.Throws<Exception>(() =>
                this.userAccountService.ChangePassword("old", "New1234!", "Different!"));

            Assert.That(thrownException.Message, Is.EqualTo("Passwords don't match"));
        }

        [Test]
        public void ChangePassword_ValidData_UpdatesPasswordHash()
        {
            var user = CreateUser(passwordHash: "oldhash");
            this.LoginAs(user);
            this.mockSecurityService.Setup(service => service.VerifyPassword("old", "oldhash")).Returns(true);
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectPasswordFormat("New1234!")).Returns(true);
            this.mockSecurityService.Setup(service => service.HashPassword("New1234!")).Returns("newhash");

            this.userAccountService.ChangePassword("old", "New1234!", "New1234!");

            Assert.That(this.userAccountService.CurrentUser!.PasswordHash, Is.EqualTo("newhash"));
            this.mockUsersRepository.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public void SearchUsers_NotLoggedIn_ThrowsException()
        {
            Assert.Throws<Exception>(() => this.userAccountService.SearchUsers("query"));
        }

        [Test]
        public void SearchUsers_NonAdmin_ThrowsException()
        {
            var user = CreateUser(isAdmin: false);
            this.LoginAs(user);

            Assert.Throws<Exception>(() => this.userAccountService.SearchUsers("query"));
        }

        [Test]
        public void SearchUsers_ByIdPrefix_ReturnsMatchingUser()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            var target = CreateUser(id: 5, username: "target");
            this.LoginAs(admin);
            this.mockUsersRepository.Setup(repository => repository.GetAllUsers()).Returns(new List<User> { admin, target });

            var result = this.userAccountService.SearchUsers("id:5");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(5));
        }

        [Test]
        public void SearchUsers_ByUsernamePrefix_ReturnsMatchingUsers()
        {
            var admin = CreateUser(id: 1, isAdmin: true, username: "admin");
            var user1 = CreateUser(id: 2, username: "john_doe");
            var user2 = CreateUser(id: 3, username: "jane_doe");
            this.LoginAs(admin);
            this.mockUsersRepository.Setup(repository => repository.GetAllUsers()).Returns(new List<User> { admin, user1, user2 });

            var result = this.userAccountService.SearchUsers("username:doe");

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void SearchUsers_ByEmailPrefix_ReturnsMatchingUsers()
        {
            var admin = CreateUser(id: 1, isAdmin: true, email: "admin@test.com");
            var user1 = CreateUser(id: 2, email: "paul@gmail.com");
            this.LoginAs(admin);
            this.mockUsersRepository.Setup(repository => repository.GetAllUsers()).Returns(new List<User> { admin, user1 });

            var result = this.userAccountService.SearchUsers("mail:gmail");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Email, Does.Contain("gmail"));
        }

        [Test]
        public void SearchUsers_NoPrefix_ReturnsAllUsers()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            var user1 = CreateUser(id: 2);
            this.LoginAs(admin);
            this.mockUsersRepository.Setup(repository => repository.GetAllUsers()).Returns(new List<User> { admin, user1 });

            var result = this.userAccountService.SearchUsers("anything");

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void PromoteToAdmin_NotLoggedIn_ThrowsException()
        {
            var client = CreateUser(id: 2);

            Assert.Throws<Exception>(() => this.userAccountService.PromoteToAdmin(client));
        }

        [Test]
        public void PromoteToAdmin_NonAdmin_ThrowsException()
        {
            var user = CreateUser(isAdmin: false);
            this.LoginAs(user);
            var client = CreateUser(id: 2);

            Assert.Throws<Exception>(() => this.userAccountService.PromoteToAdmin(client));
        }

        [Test]
        public void PromoteToAdmin_ValidAdmin_SetsClientAsAdmin()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            this.LoginAs(admin);
            var client = CreateUser(id: 2);

            this.userAccountService.PromoteToAdmin(client);

            Assert.That(client.IsAdmin, Is.True);
            this.mockUsersRepository.Verify(repository => repository.UpdateUser(client), Times.Once);
        }

        [Test]
        public void PromoteToAdmin_ClientAlreadyAdmin_DoesNotCallRepository()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            this.LoginAs(admin);
            var client = CreateUser(id: 2, isAdmin: true);

            this.userAccountService.PromoteToAdmin(client);

            this.mockUsersRepository.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void PromoteToAdmin_ClientIsDisabled_DoesNotCallRepository()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            this.LoginAs(admin);
            var client = CreateUser(id: 2, isDisabled: true);

            this.userAccountService.PromoteToAdmin(client);

            this.mockUsersRepository.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        private void LoginAs(User user)
        {
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat(user.Email)).Returns(true);
            this.mockUsersRepository.Setup(repository => repository.GetUserByEmail(user.Email)).Returns(user);
            this.mockSecurityService.Setup(service => service.VerifyPassword(user.PasswordHash, user.PasswordHash)).Returns(true);
            this.userAccountService.Login(user.Email, user.PasswordHash);
        }

        [Test]
        public void Login_NullEmail_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => this.userAccountService.Login(null, "password"));
        }

        [Test]
        public void Login_NullPassword_ThrowsArgumentException()
        {
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("test@test.com")).Returns(true);
            Assert.Throws<ArgumentException>(() => this.userAccountService.Login("test@test.com", null));
        }

        [Test]
        public void Login_WhitespaceEmail_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => this.userAccountService.Login("   ", "password"));
        }

        [Test]
        public void Login_WhitespacePassword_ThrowsArgumentException()
        {
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("test@test.com")).Returns(true);
            Assert.Throws<ArgumentException>(() => this.userAccountService.Login("test@test.com", "   "));
        }

        [Test]
        public void Login_SuccessfulLogin_CurrentUserNotNull()
        {
            var user = CreateUser(email: "u@u.com", passwordHash: "pass");
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("u@u.com")).Returns(true);
            this.mockUsersRepository.Setup(repository => repository.GetUserByEmail("u@u.com")).Returns(user);
            this.mockSecurityService.Setup(service => service.VerifyPassword("pass", "pass")).Returns(true);

            this.userAccountService.Login("u@u.com", "pass");

            Assert.That(this.userAccountService.CurrentUser, Is.Not.Null);
        }

        [Test]
        public void Register_EmptyEmail_ThrowsException()
        {
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat(string.Empty)).Returns(false);
            Assert.Throws<Exception>(() =>
                this.userAccountService.Register(string.Empty, "Pass1234!", "Pass1234!", "user", "0711111111"));
        }

        [Test]
        public void Register_NullEmail_ThrowsException()
        {
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat((string)null!)).Returns(false);
            Assert.Throws<Exception>(() =>
                this.userAccountService.Register(null, "Pass1234!", "Pass1234!", "user", "0711111111"));
        }

        [Test]
        public void ChangePassword_EmptyOldPassword_ThrowsException()
        {
            var user = CreateUser(passwordHash: "oldhash");
            this.LoginAs(user);
            this.mockSecurityService.Setup(service => service.VerifyPassword(string.Empty, "oldhash")).Returns(false);

            Assert.Throws<Exception>(() =>
                this.userAccountService.ChangePassword(string.Empty, "New1234!", "New1234!"));
        }

        [Test]
        public void ChangePassword_WeakNewPassword_ThrowsException()
        {
            var user = CreateUser(passwordHash: "oldhash");
            this.LoginAs(user);
            this.mockSecurityService.Setup(service => service.VerifyPassword("old", "oldhash")).Returns(true);
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectPasswordFormat("weak")).Returns(false);

            Assert.Throws<Exception>(() =>
                this.userAccountService.ChangePassword("old", "weak", "weak"));
        }

        [Test]
        public void UpdateProfile_InvalidPhoneNumber_ThrowsException()
        {
            var user = CreateUser();
            this.LoginAs(user);
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectUsernameFormat("validname")).Returns(true);
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectPhoneNumberFormat("abc")).Returns(false);

            Assert.Throws<Exception>(() => this.userAccountService.UpdateProfile("validname", "abc"));
        }

        [Test]
        public void UpdateProfile_ValidData_UpdatesPhone()
        {
            var user = CreateUser();
            this.LoginAs(user);
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectUsernameFormat("newname")).Returns(true);
            this.mockUserValidationService.Setup(validationService => validationService.IsCorrectPhoneNumberFormat("0722222222")).Returns(true);

            this.userAccountService.UpdateProfile("newname", "0722222222");

            Assert.That(this.userAccountService.CurrentUser!.PhoneNumber, Is.EqualTo("0722222222"));
        }

        [Test]
        public void SearchUsers_EmptyQuery_ReturnsAll()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            var user1 = CreateUser(id: 2);
            this.LoginAs(admin);
            this.mockUsersRepository.Setup(repository => repository.GetAllUsers()).Returns(new List<User> { admin, user1 });

            var result = this.userAccountService.SearchUsers(string.Empty);

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void SearchUsers_IdPrefixNoMatch_ReturnsEmpty()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            this.LoginAs(admin);
            this.mockUsersRepository.Setup(repository => repository.GetAllUsers()).Returns(new List<User> { admin });

            var result = this.userAccountService.SearchUsers("id:999");

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void PromoteToAdmin_NullClient_ThrowsException()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            this.LoginAs(admin);

            Assert.Throws<NullReferenceException>(() => this.userAccountService.PromoteToAdmin(null));
        }
    }
}
