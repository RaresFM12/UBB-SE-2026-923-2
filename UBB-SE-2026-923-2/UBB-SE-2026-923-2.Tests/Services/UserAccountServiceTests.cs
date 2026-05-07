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

        // ========== Login Tests ==========

        [Test]
        public void Login_ValidCredentials_SetsCurrentUser()
        {
            var user = CreateUser(email: "paul@gmail.com", passwordHash: "abc123");
            mockUserValidationService.Setup(v => v.IsCorrectEmailFormat("paul@gmail.com")).Returns(true);
            mockUsersRepository.Setup(r => r.GetUserByEmail("paul@gmail.com")).Returns(user);
            mockSecurityService.Setup(s => s.VerifyPassword("abc123", "abc123")).Returns(true);

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
            mockUserValidationService.Setup(v => v.IsCorrectEmailFormat("test@test.com")).Returns(true);

            Assert.Throws<ArgumentException>(() => userAccountService.Login("test@test.com", ""));
        }

        [Test]
        public void Login_InvalidEmailFormat_ThrowsException()
        {
            mockUserValidationService.Setup(v => v.IsCorrectEmailFormat("invalid")).Returns(false);

            var ex = Assert.Throws<Exception>(() => userAccountService.Login("invalid", "password"));

            Assert.That(ex.Message, Is.EqualTo("Not a valid e-mail"));
        }

        [Test]
        public void Login_EmailNotFound_ThrowsException()
        {
            mockUserValidationService.Setup(v => v.IsCorrectEmailFormat("unknown@test.com")).Returns(true);
            mockUsersRepository.Setup(r => r.GetUserByEmail("unknown@test.com")).Returns((User)null!);

            var ex = Assert.Throws<Exception>(() => userAccountService.Login("unknown@test.com", "password"));

            Assert.That(ex.Message, Is.EqualTo("E-mail not found"));
        }

        [Test]
        public void Login_DisabledAccount_ThrowsException()
        {
            var user = CreateUser(isDisabled: true);
            mockUserValidationService.Setup(v => v.IsCorrectEmailFormat("test@test.com")).Returns(true);
            mockUsersRepository.Setup(r => r.GetUserByEmail("test@test.com")).Returns(user);

            var ex = Assert.Throws<Exception>(() => userAccountService.Login("test@test.com", "password"));

            Assert.That(ex.Message, Is.EqualTo("Account disabled"));
        }

        [Test]
        public void Login_IncorrectPassword_ThrowsException()
        {
            var user = CreateUser(passwordHash: "hashed");
            mockUserValidationService.Setup(v => v.IsCorrectEmailFormat("test@test.com")).Returns(true);
            mockUsersRepository.Setup(r => r.GetUserByEmail("test@test.com")).Returns(user);
            mockSecurityService.Setup(s => s.VerifyPassword("wrong", "hashed")).Returns(false);

            var ex = Assert.Throws<Exception>(() => userAccountService.Login("test@test.com", "wrong"));

            Assert.That(ex.Message, Is.EqualTo("Incorrect password"));
        }

        // ========== Register Tests ==========

        [Test]
        public void Register_ValidData_SetsCurrentUser()
        {
            var user = CreateUser(email: "new@test.com");
            mockUserValidationService.Setup(v => v.IsCorrectEmailFormat("new@test.com")).Returns(true);
            mockUserValidationService.Setup(v => v.IsCorrectPasswordFormat("Pass1234!")).Returns(true);
            mockUserValidationService.Setup(v => v.IsCorrectUsernameFormat("newuser")).Returns(true);
            mockUserValidationService.Setup(v => v.IsCorrectPhoneNumberFormat("0711111111")).Returns(true);
            mockUsersRepository.Setup(r => r.GetUserByEmail("new@test.com")).Returns((User)null!);
            mockSecurityService.Setup(s => s.HashPassword("Pass1234!")).Returns("hashed");
            mockUsersRepository.Setup(r => r.GetUserByEmail("new@test.com")).Returns(user);

            userAccountService.Register("new@test.com", "Pass1234!", "Pass1234!", "newuser", "0711111111");

            Assert.That(userAccountService.CurrentUser, Is.EqualTo(user));
        }

        [Test]
        public void Register_InvalidEmailFormat_ThrowsException()
        {
            mockUserValidationService.Setup(v => v.IsCorrectEmailFormat("bad")).Returns(false);

            Assert.Throws<Exception>(() =>
                userAccountService.Register("bad", "Pass1234!", "Pass1234!", "user", "0711111111"));
        }

        [Test]
        public void Register_EmptyPassword_ThrowsException()
        {
            mockUserValidationService.Setup(v => v.IsCorrectEmailFormat("a@b.c")).Returns(true);

            var ex = Assert.Throws<Exception>(() =>
                userAccountService.Register("a@b.c", "", "", "user", "0711111111"));

            Assert.That(ex.Message, Is.EqualTo("Password cannot be empty."));
        }

        [Test]
        public void Register_PasswordsDoNotMatch_ThrowsException()
        {
            mockUserValidationService.Setup(v => v.IsCorrectEmailFormat("a@b.c")).Returns(true);

            var ex = Assert.Throws<Exception>(() =>
                userAccountService.Register("a@b.c", "Pass1234!", "Different1!", "user", "0711111111"));

            Assert.That(ex.Message, Is.EqualTo("Passwords don't match."));
        }

        [Test]
        public void Register_WeakPassword_ThrowsException()
        {
            mockUserValidationService.Setup(v => v.IsCorrectEmailFormat("a@b.c")).Returns(true);
            mockUserValidationService.Setup(v => v.IsCorrectPasswordFormat("weak")).Returns(false);

            Assert.Throws<Exception>(() =>
                userAccountService.Register("a@b.c", "weak", "weak", "user", "0711111111"));
        }

        [Test]
        public void Register_EmailAlreadyExists_ThrowsException()
        {
            var existingUser = CreateUser(email: "exists@test.com");
            mockUserValidationService.Setup(v => v.IsCorrectEmailFormat("exists@test.com")).Returns(true);
            mockUserValidationService.Setup(v => v.IsCorrectPasswordFormat("Pass1234!")).Returns(true);
            mockUserValidationService.Setup(v => v.IsCorrectUsernameFormat("user")).Returns(true);
            mockUsersRepository.Setup(r => r.GetUserByEmail("exists@test.com")).Returns(existingUser);

            var ex = Assert.Throws<Exception>(() =>
                userAccountService.Register("exists@test.com", "Pass1234!", "Pass1234!", "user", "0711111111"));

            Assert.That(ex.Message, Is.EqualTo("Email already linked to an account"));
        }

        [Test]
        public void Register_InvalidUsername_ThrowsException()
        {
            mockUserValidationService.Setup(v => v.IsCorrectEmailFormat("a@b.c")).Returns(true);
            mockUserValidationService.Setup(v => v.IsCorrectPasswordFormat("Pass1234!")).Returns(true);
            mockUserValidationService.Setup(v => v.IsCorrectUsernameFormat("bad user!")).Returns(false);

            Assert.Throws<Exception>(() =>
                userAccountService.Register("a@b.c", "Pass1234!", "Pass1234!", "bad user!", "0711111111"));
        }

        [Test]
        public void Register_InvalidPhoneNumber_ThrowsException()
        {
            mockUserValidationService.Setup(v => v.IsCorrectEmailFormat("a@b.c")).Returns(true);
            mockUserValidationService.Setup(v => v.IsCorrectPasswordFormat("Pass1234!")).Returns(true);
            mockUserValidationService.Setup(v => v.IsCorrectUsernameFormat("user")).Returns(true);
            mockUserValidationService.Setup(v => v.IsCorrectPhoneNumberFormat("abc")).Returns(false);
            mockUsersRepository.Setup(r => r.GetUserByEmail("a@b.c")).Returns((User)null!);

            Assert.Throws<Exception>(() =>
                userAccountService.Register("a@b.c", "Pass1234!", "Pass1234!", "user", "abc"));
        }

        // ========== UpdateProfile Tests ==========

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
            mockUserValidationService.Setup(v => v.IsCorrectUsernameFormat("newname")).Returns(true);
            mockUserValidationService.Setup(v => v.IsCorrectPhoneNumberFormat("0799999999")).Returns(true);

            userAccountService.UpdateProfile("newname", "0799999999");

            Assert.That(userAccountService.CurrentUser!.Username, Is.EqualTo("newname"));
            mockUsersRepository.Verify(r => r.UpdateUser(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public void UpdateProfile_InvalidUsername_ThrowsException()
        {
            var user = CreateUser();
            LoginAs(user);
            mockUserValidationService.Setup(v => v.IsCorrectUsernameFormat("bad!")).Returns(false);

            Assert.Throws<Exception>(() => userAccountService.UpdateProfile("bad!", "0711111111"));
        }

        // ========== ChangePassword Tests ==========

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
            mockSecurityService.Setup(s => s.VerifyPassword("wrong", "oldhash")).Returns(false);

            var ex = Assert.Throws<Exception>(() =>
                userAccountService.ChangePassword("wrong", "New1234!", "New1234!"));

            Assert.That(ex.Message, Is.EqualTo("Incorrect password"));
        }

        [Test]
        public void ChangePassword_PasswordsDoNotMatch_ThrowsException()
        {
            var user = CreateUser(passwordHash: "oldhash");
            LoginAs(user);
            mockSecurityService.Setup(s => s.VerifyPassword("old", "oldhash")).Returns(true);
            mockUserValidationService.Setup(v => v.IsCorrectPasswordFormat("New1234!")).Returns(true);

            var ex = Assert.Throws<Exception>(() =>
                userAccountService.ChangePassword("old", "New1234!", "Different!"));

            Assert.That(ex.Message, Is.EqualTo("Passwords don't match"));
        }

        [Test]
        public void ChangePassword_ValidData_UpdatesPasswordHash()
        {
            var user = CreateUser(passwordHash: "oldhash");
            LoginAs(user);
            mockSecurityService.Setup(s => s.VerifyPassword("old", "oldhash")).Returns(true);
            mockUserValidationService.Setup(v => v.IsCorrectPasswordFormat("New1234!")).Returns(true);
            mockSecurityService.Setup(s => s.HashPassword("New1234!")).Returns("newhash");

            userAccountService.ChangePassword("old", "New1234!", "New1234!");

            Assert.That(userAccountService.CurrentUser!.PasswordHash, Is.EqualTo("newhash"));
            mockUsersRepository.Verify(r => r.UpdateUser(It.IsAny<User>()), Times.Once);
        }

        // ========== SearchUsers Tests ==========

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
            mockUsersRepository.Setup(r => r.GetAllUsers()).Returns(new List<User> { admin, target });

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
            mockUsersRepository.Setup(r => r.GetAllUsers()).Returns(new List<User> { admin, user1, user2 });

            var result = userAccountService.SearchUsers("username:doe");

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void SearchUsers_ByEmailPrefix_ReturnsMatchingUsers()
        {
            var admin = CreateUser(id: 1, isAdmin: true, email: "admin@test.com");
            var user1 = CreateUser(id: 2, email: "paul@gmail.com");
            LoginAs(admin);
            mockUsersRepository.Setup(r => r.GetAllUsers()).Returns(new List<User> { admin, user1 });

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
            mockUsersRepository.Setup(r => r.GetAllUsers()).Returns(new List<User> { admin, user1 });

            var result = userAccountService.SearchUsers("anything");

            Assert.That(result.Count, Is.EqualTo(2));
        }

        // ========== PromoteToAdmin Tests ==========

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
            mockUsersRepository.Verify(r => r.UpdateUser(client), Times.Once);
        }

        [Test]
        public void PromoteToAdmin_ClientAlreadyAdmin_DoesNotCallRepository()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            LoginAs(admin);
            var client = CreateUser(id: 2, isAdmin: true);

            userAccountService.PromoteToAdmin(client);

            mockUsersRepository.Verify(r => r.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void PromoteToAdmin_ClientIsDisabled_DoesNotCallRepository()
        {
            var admin = CreateUser(id: 1, isAdmin: true);
            LoginAs(admin);
            var client = CreateUser(id: 2, isDisabled: true);

            userAccountService.PromoteToAdmin(client);

            mockUsersRepository.Verify(r => r.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        // ========== Helper ==========

        private void LoginAs(User user)
        {
            mockUserValidationService.Setup(v => v.IsCorrectEmailFormat(user.Email)).Returns(true);
            mockUsersRepository.Setup(r => r.GetUserByEmail(user.Email)).Returns(user);
            mockSecurityService.Setup(s => s.VerifyPassword(user.PasswordHash, user.PasswordHash)).Returns(true);
            userAccountService.Login(user.Email, user.PasswordHash);
        }
    }
}
