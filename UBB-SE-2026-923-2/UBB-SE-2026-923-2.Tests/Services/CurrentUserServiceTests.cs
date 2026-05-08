namespace UBB_SE_2026_923_2.Tests.Services
{
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class CurrentUserServiceTests
    {
        private CurrentUserService service;

        [SetUp]
        public void Setup()
        {
            this.service = new CurrentUserService();
            this.service.UserId = 0;
            this.service.RoleType = UserRole.Client;
        }

        [Test]
        public void UserId_SetAndGet_ReturnsCorrectValue()
        {
            this.service.UserId = 42;
            Assert.That(this.service.UserId, Is.EqualTo(42));
        }

        [Test]
        public void Role_WhenClient_ReturnsClientString()
        {
            this.service.RoleType = UserRole.Client;
            Assert.That(this.service.Role, Is.EqualTo("Client"));
        }

        [Test]
        public void SetFromUser_NullUser_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => this.service.SetFromUser(null));
        }

        [Test]
        public void SetFromUser_UserWithClientRole_SetsClientRoleType()
        {
            var user = new User(1, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            user.Role = "Client";
            this.service.SetFromUser(user);
            Assert.That(this.service.RoleType, Is.EqualTo(UserRole.Client));
            Assert.That(this.service.UserId, Is.EqualTo(1));
        }

        [Test]
        public void SetFromUser_CaseInsensitiveRole_Works()
        {
            var user = new User(2, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            user.Role = "admin";
            this.service.SetFromUser(user);
            Assert.That(this.service.RoleType, Is.EqualTo(UserRole.Admin));
        }

        [Test]
        public void SetFromUser_UserWithUnknownRole_FallsBackToIsAdmin()
        {
            var user = new User(3, "a@b.com", "123", "hash", true, false, "user1", false, 0);
            user.Role = "UnknownRole";
            this.service.SetFromUser(user);
            Assert.That(this.service.RoleType, Is.EqualTo(UserRole.Admin));
        }

        [Test]
        public void SetFromUser_UserWithUnknownRoleNotAdmin_FallsBackToClient()
        {
            var user = new User(3, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            user.Role = "UnknownRole";
            this.service.SetFromUser(user);
            Assert.That(this.service.RoleType, Is.EqualTo(UserRole.Client));
        }
    }
}