using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.Tests.Services
{
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
            mockUsersRepository = new Mock<IUsersRepository>();
            mockSecurityService = new Mock<ISecurityService>();
            mockUserValidationService = new Mock<IUserValidationService>();
            userAccountService = new UserAccountService(
                mockUsersRepository.Object,
                mockSecurityService.Object,
                mockUserValidationService.Object);
        }

        [Test]
        public void Login_ValidCredentials_SetsCurrentUser()
        {
            var user = CreateUser(email: "paul@gmail.com", passwordHash: "abc123");
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("paul@gmail.com")).Returns(true);
            mockUsersRepository.Setup(repository => repository.GetUserByEmail("paul@gmail.com")).Returns(user);
            mockSecurityService.Setup(service => service.VerifyPassword("abc123", "abc123")).Returns(true);

            userAccountService.Login("paul@gmail.com", "abc123");

            Assert.That(userAccountService.CurrentUser, Is.EqualTo(user));
        }

        [Test]
        public void Login_EmptyEmail_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => userAccountService.Login("", "password"));
        }

        [Test]
        public void Login_EmptyPassword_ThrowsArgumentException()
        {
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("test@test.com")).Returns(true);

            Assert.Throws<ArgumentException>(() => userAccountService.Login("test@test.com", ""));
        }

        [Test]
        public void Login_InvalidEmailFormat_ThrowsException()
        {
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("invalid")).Returns(false);

            var thrownException = Assert.Throws<Exception>(() => userAccountService.Login("invalid", "password"));

            Assert.That(thrownException.Message, Is.EqualTo("Not a valid e-mail"));
        }

        [Test]
        public void Login_EmailNotFound_ThrowsException()
        {
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("unknown@test.com")).Returns(true);
            mockUsersRepository.Setup(repository => repository.GetUserByEmail("unknown@test.com")).Returns((User)null!);

            var thrownException = Assert.Throws<Exception>(() => userAccountService.Login("unknown@test.com", "password"));

            Assert.That(thrownException.Message, Is.EqualTo("E-mail not found"));
        }

        [Test]
        public void Login_DisabledAccount_ThrowsException()
        {
            var user = CreateUser(isDisabled: true);
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("test@test.com")).Returns(true);
            mockUsersRepository.Setup(repository => repository.GetUserByEmail("test@test.com")).Returns(user);

            var thrownException = Assert.Throws<Exception>(() => userAccountService.Login("test@test.com", "password"));

            Assert.That(thrownException.Message, Is.EqualTo("Account disabled"));
        }

        [Test]
        public void Login_IncorrectPassword_ThrowsException()
        {
            var user = CreateUser(passwordHash: "hashed");
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("test@test.com")).Returns(true);
            mockUsersRepository.Setup(repository => repository.GetUserByEmail("test@test.com")).Returns(user);
            mockSecurityService.Setup(service => service.VerifyPassword("wrong", "hashed")).Returns(false);

            var thrownException = Assert.Throws<Exception>(() => userAccountService.Login("test@test.com", "wrong"));

            Assert.That(thrownException.Message, Is.EqualTo("Incorrect password"));
        }

        [Test]
        public void Register_InvalidEmailFormat_ThrowsException()
        {
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("bad")).Returns(false);

            Assert.Throws<Exception>(() =>
                userAccountService.Register("bad", "Pass1234!", "Pass1234!", "user", "0711111111"));
        }

        [Test]
        public void Register_EmptyPassword_ThrowsException()
        {
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("a@b.c")).Returns(true);

            var thrownException = Assert.Throws<Exception>(() =>
                userAccountService.Register("a@b.c", "", "", "user", "0711111111"));

            Assert.That(thrownException.Message, Is.EqualTo("Password cannot be empty."));
        }

        [Test]
        public void Register_PasswordsDoNotMatch_ThrowsException()
        {
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("a@b.c")).Returns(true);

            var thrownException = Assert.Throws<Exception>(() =>
                userAccountService.Register("a@b.c", "Pass1234!", "Different1!", "user", "0711111111"));

            Assert.That(thrownException.Message, Is.EqualTo("Passwords don't match."));
        }

        [Test]
        public void Register_WeakPassword_ThrowsException()
        {
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("a@b.c")).Returns(true);
            mockUserValidationService.Setup(validationService => validationService.IsCorrectPasswordFormat("weak")).Returns(false);

            Assert.Throws<Exception>(() =>
                userAccountService.Register("a@b.c", "weak", "weak", "user", "0711111111"));
        }

        [Test]
        public void Register_InvalidUsername_ThrowsException()
        {
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("a@b.c")).Returns(true);
            mockUserValidationService.Setup(validationService => validationService.IsCorrectPasswordFormat("Pass1234!")).Returns(true);
            mockUserValidationService.Setup(validationService => validationService.IsCorrectUsernameFormat("bad user!")).Returns(false);

            Assert.Throws<Exception>(() =>
                userAccountService.Register("a@b.c", "Pass1234!", "Pass1234!", "bad user!", "0711111111"));
        }

        [Test]
        public void Register_InvalidPhoneNumber_ThrowsException()
        {
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("a@b.c")).Returns(true);
            mockUserValidationService.Setup(validationService => validationService.IsCorrectPasswordFormat("Pass1234!")).Returns(true);
            mockUserValidationService.Setup(validationService => validationService.IsCorrectUsernameFormat("user")).Returns(true);
            mockUserValidationService.Setup(validationService => validationService.IsCorrectPhoneNumberFormat("abc")).Returns(false);
            mockUsersRepository.Setup(repository => repository.GetUserByEmail("a@b.c")).Returns((User)null!);

            Assert.Throws<Exception>(() =>
                userAccountService.Register("a@b.c", "Pass1234!", "Pass1234!", "user", "abc"));
        }

        [Test]
        public void UpdateProfile_NotLoggedIn_ThrowsException()
        {
            Assert.Throws<Exception>(() => userAccountService.UpdateProfile("newname", "0711111111"));
        }

        [Test]
        public void UpdateProfile_ValidData_UpdatesUsername()
        {
            var user = CreateUser();
            LoginAs(user);
            mockUserValidationService.Setup(validationService => validationService.IsCorrectUsernameFormat("newname")).Returns(true);
            mockUserValidationService.Setup(validationService => validationService.IsCorrectPhoneNumberFormat("0799999999")).Returns(true);

            userAccountService.UpdateProfile("newname", "0799999999");

            Assert.That(userAccountService.CurrentUser!.Username, Is.EqualTo("newname"));
            mockUsersRepository.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public void UpdateProfile_InvalidUsername_ThrowsException()
        {
            var user = CreateUser();
            LoginAs(user);
            mockUserValidationService.Setup(validationService => validationService.IsCorrectUsernameFormat("bad!")).Returns(false);

            Assert.Throws<Exception>(() => userAccountService.UpdateProfile("bad!", "0711111111"));
        }

        [Test]
        public void ChangePassword_NotLoggedIn_ThrowsException()
        {
            Assert.Throws<Exception>(() =>
                userAccountService.ChangePassword("old", "New1234!", "New1234!"));
        }

        [Test]
        public void ChangePassword_IncorrectOldPassword_ThrowsException()
        {
            var user = CreateUser(passwordHash: "oldhash");
            LoginAs(user);
            mockSecurityService.Setup(service => service.VerifyPassword("wrong", "oldhash")).Returns(false);

            var thrownException = Assert.Throws<Exception>(() =>
                userAccountService.ChangePassword("wrong", "New1234!", "New1234!"));

            Assert.That(thrownException.Message, Is.EqualTo("Incorrect password"));
        }

        [Test]
        public void ChangePassword_PasswordsDoNotMatch_ThrowsException()
        {
            var user = CreateUser(passwordHash: "oldhash");
            LoginAs(user);
            mockSecurityService.Setup(service => service.VerifyPassword("old", "oldhash")).Returns(true);
            mockUserValidationService.Setup(validationService => validationService.IsCorrectPasswordFormat("New1234!")).Returns(true);

            var thrownException = Assert.Throws<Exception>(() =>
                userAccountService.ChangePassword("old", "New1234!", "Different!"));

            Assert.That(thrownException.Message, Is.EqualTo("Passwords don't match"));
        }

        [Test]
        public void ChangePassword_ValidData_UpdatesPasswordHash()
        {
            var user = CreateUser(passwordHash: "oldhash");
            LoginAs(user);
            mockSecurityService.Setup(service => service.VerifyPassword("old", "oldhash")).Returns(true);
            mockUserValidationService.Setup(validationService => validationService.IsCorrectPasswordFormat("New1234!")).Returns(true);
            mockSecurityService.Setup(service => service.HashPassword("New1234!")).Returns("newhash");

            userAccountService.ChangePassword("old", "New1234!", "New1234!");

            Assert.That(userAccountService.CurrentUser!.PasswordHash, Is.EqualTo("newhash"));
            mockUsersRepository.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public void SearchUsers_NotLoggedIn_ThrowsException()
        {
            Assert.Throws<Exception>(() => userAccountService.SearchUsers("query"));
        }

        [Test]
        public void SearchUsers_NonAdmin_ThrowsException()
        {
            var user = CreateUser(isAdmin: false);
            LoginAs(user);

            Assert.Throws<Exception>(() => userAccountService.SearchUsers("query"));
        }

        [Test]
        public void SearchUsers_ByIdPrefix_ReturnsMatchingUser()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            var target = CreateUser(id: 5, username: "target");
            LoginAs(admin);
            mockUsersRepository.Setup(repository => repository.GetAllUsers()).Returns(new List<User> { admin, target });

            var result = userAccountService.SearchUsers("id:5");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(5));
        }

        [Test]
        public void SearchUsers_ByUsernamePrefix_ReturnsMatchingUsers()
        {
            var admin = CreateUser(id: 1, isAdmin: true, username: "admin");
            var user1 = CreateUser(id: 2, username: "john_doe");
            var user2 = CreateUser(id: 3, username: "jane_doe");
            LoginAs(admin);
            mockUsersRepository.Setup(repository => repository.GetAllUsers()).Returns(new List<User> { admin, user1, user2 });

            var result = userAccountService.SearchUsers("username:doe");

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void SearchUsers_ByEmailPrefix_ReturnsMatchingUsers()
        {
            var admin = CreateUser(id: 1, isAdmin: true, email: "admin@test.com");
            var user1 = CreateUser(id: 2, email: "paul@gmail.com");
            LoginAs(admin);
            mockUsersRepository.Setup(repository => repository.GetAllUsers()).Returns(new List<User> { admin, user1 });

            var result = userAccountService.SearchUsers("mail:gmail");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Email, Does.Contain("gmail"));
        }

        [Test]
        public void SearchUsers_NoPrefix_ReturnsAllUsers()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            var user1 = CreateUser(id: 2);
            LoginAs(admin);
            mockUsersRepository.Setup(repository => repository.GetAllUsers()).Returns(new List<User> { admin, user1 });

            var result = userAccountService.SearchUsers("anything");

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void PromoteToAdmin_NotLoggedIn_ThrowsException()
        {
            var client = CreateUser(id: 2);

            Assert.Throws<Exception>(() => userAccountService.PromoteToAdmin(client));
        }

        [Test]
        public void PromoteToAdmin_NonAdmin_ThrowsException()
        {
            var user = CreateUser(isAdmin: false);
            LoginAs(user);
            var client = CreateUser(id: 2);

            Assert.Throws<Exception>(() => userAccountService.PromoteToAdmin(client));
        }

        [Test]
        public void PromoteToAdmin_ValidAdmin_SetsClientAsAdmin()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            LoginAs(admin);
            var client = CreateUser(id: 2);

            userAccountService.PromoteToAdmin(client);

            Assert.That(client.IsAdmin, Is.True);
            mockUsersRepository.Verify(repository => repository.UpdateUser(client), Times.Once);
        }

        [Test]
        public void PromoteToAdmin_ClientAlreadyAdmin_DoesNotCallRepository()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            LoginAs(admin);
            var client = CreateUser(id: 2, isAdmin: true);

            userAccountService.PromoteToAdmin(client);

            mockUsersRepository.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void PromoteToAdmin_ClientIsDisabled_DoesNotCallRepository()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            LoginAs(admin);
            var client = CreateUser(id: 2, isDisabled: true);

            userAccountService.PromoteToAdmin(client);

            mockUsersRepository.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        private void LoginAs(User user)
        {
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat(user.Email)).Returns(true);
            mockUsersRepository.Setup(repository => repository.GetUserByEmail(user.Email)).Returns(user);
            mockSecurityService.Setup(service => service.VerifyPassword(user.PasswordHash, user.PasswordHash)).Returns(true);
            userAccountService.Login(user.Email, user.PasswordHash);
        }

        [Test]
        public void Login_NullEmail_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => userAccountService.Login(null, "password"));
        }

        [Test]
        public void Login_NullPassword_ThrowsArgumentException()
        {
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("test@test.com")).Returns(true);
            Assert.Throws<ArgumentException>(() => userAccountService.Login("test@test.com", null));
        }

        [Test]
        public void Login_WhitespaceEmail_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => userAccountService.Login("   ", "password"));
        }

        [Test]
        public void Login_WhitespacePassword_ThrowsArgumentException()
        {
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("test@test.com")).Returns(true);
            Assert.Throws<ArgumentException>(() => userAccountService.Login("test@test.com", "   "));
        }

        [Test]
        public void Login_SuccessfulLogin_CurrentUserNotNull()
        {
            var user = CreateUser(email: "u@u.com", passwordHash: "pass");
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("u@u.com")).Returns(true);
            mockUsersRepository.Setup(repository => repository.GetUserByEmail("u@u.com")).Returns(user);
            mockSecurityService.Setup(service => service.VerifyPassword("pass", "pass")).Returns(true);

            userAccountService.Login("u@u.com", "pass");

            Assert.That(userAccountService.CurrentUser, Is.Not.Null);
        }

        [Test]
        public void Register_EmptyEmail_ThrowsException()
        {
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat("")).Returns(false);
            Assert.Throws<Exception>(() =>
                userAccountService.Register("", "Pass1234!", "Pass1234!", "user", "0711111111"));
        }

        [Test]
        public void Register_NullEmail_ThrowsException()
        {
            mockUserValidationService.Setup(validationService => validationService.IsCorrectEmailFormat((string)null!)).Returns(false);
            Assert.Throws<Exception>(() =>
                userAccountService.Register(null, "Pass1234!", "Pass1234!", "user", "0711111111"));
        }

        [Test]
        public void ChangePassword_EmptyOldPassword_ThrowsException()
        {
            var user = CreateUser(passwordHash: "oldhash");
            LoginAs(user);
            mockSecurityService.Setup(service => service.VerifyPassword("", "oldhash")).Returns(false);

            Assert.Throws<Exception>(() =>
                userAccountService.ChangePassword("", "New1234!", "New1234!"));
        }

        [Test]
        public void ChangePassword_WeakNewPassword_ThrowsException()
        {
            var user = CreateUser(passwordHash: "oldhash");
            LoginAs(user);
            mockSecurityService.Setup(service => service.VerifyPassword("old", "oldhash")).Returns(true);
            mockUserValidationService.Setup(validationService => validationService.IsCorrectPasswordFormat("weak")).Returns(false);

            Assert.Throws<Exception>(() =>
                userAccountService.ChangePassword("old", "weak", "weak"));
        }

        [Test]
        public void UpdateProfile_InvalidPhoneNumber_ThrowsException()
        {
            var user = CreateUser();
            LoginAs(user);
            mockUserValidationService.Setup(validationService => validationService.IsCorrectUsernameFormat("validname")).Returns(true);
            mockUserValidationService.Setup(validationService => validationService.IsCorrectPhoneNumberFormat("abc")).Returns(false);

            Assert.Throws<Exception>(() => userAccountService.UpdateProfile("validname", "abc"));
        }

        [Test]
        public void UpdateProfile_ValidData_UpdatesPhone()
        {
            var user = CreateUser();
            LoginAs(user);
            mockUserValidationService.Setup(validationService => validationService.IsCorrectUsernameFormat("newname")).Returns(true);
            mockUserValidationService.Setup(validationService => validationService.IsCorrectPhoneNumberFormat("0722222222")).Returns(true);

            userAccountService.UpdateProfile("newname", "0722222222");

            Assert.That(userAccountService.CurrentUser!.PhoneNumber, Is.EqualTo("0722222222"));
        }

        [Test]
        public void SearchUsers_EmptyQuery_ReturnsAll()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            var user1 = CreateUser(id: 2);
            LoginAs(admin);
            mockUsersRepository.Setup(repository => repository.GetAllUsers()).Returns(new List<User> { admin, user1 });

            var result = userAccountService.SearchUsers("");

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void SearchUsers_IdPrefixNoMatch_ReturnsEmpty()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            LoginAs(admin);
            mockUsersRepository.Setup(repository => repository.GetAllUsers()).Returns(new List<User> { admin });

            var result = userAccountService.SearchUsers("id:999");

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void PromoteToAdmin_NullClient_ThrowsException()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            LoginAs(admin);

            Assert.Throws<NullReferenceException>(() => userAccountService.PromoteToAdmin(null));
        }
    }
}



