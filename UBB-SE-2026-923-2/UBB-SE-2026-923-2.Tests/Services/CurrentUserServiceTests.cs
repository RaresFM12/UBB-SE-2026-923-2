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
        public void RoleType_SetAndGet_ReturnsCorrectValue()
        {
            this.service.RoleType = UserRole.Admin;
            Assert.That(this.service.RoleType, Is.EqualTo(UserRole.Admin));
        }

        [Test]
        public void Role_WhenClient_ReturnsClientString()
        {
            this.service.RoleType = UserRole.Client;
            Assert.That(this.service.Role, Is.EqualTo("Client"));
        }

        [Test]
        public void Role_WhenAdmin_ReturnsAdminString()
        {
            this.service.RoleType = UserRole.Admin;
            Assert.That(this.service.Role, Is.EqualTo("Admin"));
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
        public void SetFromUser_UserWithAdminRole_SetsAdminRoleType()
        {
            var user = new User(5, "a@b.com", "123", "hash", true, false, "admin1", false, 0);
            user.Role = "Admin";
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

        [Test]
        public void SetFromUser_SetsUserId()
        {
            var user = new User(99, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            user.Role = "Client";
            this.service.SetFromUser(user);
            Assert.That(this.service.UserId, Is.EqualTo(99));
        }

        [Test]
        public void UserId_DefaultIsZero()
        {
            Assert.That(this.service.UserId, Is.EqualTo(0));
        }

        [Test]
        public void RoleType_DefaultIsClient()
        {
            Assert.That(this.service.RoleType, Is.EqualTo(UserRole.Client));
        }

        [Test]
        public void Role_DefaultIsClientString()
        {
            Assert.That(this.service.Role, Is.EqualTo("Client"));
        }

        [Test]
        public void UserId_SetNegative_Works()
        {
            this.service.UserId = -1;
            Assert.That(this.service.UserId, Is.EqualTo(-1));
        }

        [Test]
        public void UserId_SetMaxInt_Works()
        {
            this.service.UserId = int.MaxValue;
            Assert.That(this.service.UserId, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void RoleType_SetDoctor_Works()
        {
            this.service.RoleType = UserRole.Doctor;
            Assert.That(this.service.RoleType, Is.EqualTo(UserRole.Doctor));
            Assert.That(this.service.Role, Is.EqualTo("Doctor"));
        }

        [Test]
        public void RoleType_SetPharmacist_Works()
        {
            this.service.RoleType = UserRole.Pharmacist;
            Assert.That(this.service.RoleType, Is.EqualTo(UserRole.Pharmacist));
            Assert.That(this.service.Role, Is.EqualTo("Pharmacist"));
        }

        [Test]
        public void SetFromUser_DoctorRole_SetsDoctorRoleType()
        {
            var user = new User(2, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            user.Role = "Doctor";
            this.service.SetFromUser(user);
            Assert.That(this.service.RoleType, Is.EqualTo(UserRole.Doctor));
        }

        [Test]
        public void SetFromUser_PharmacistRole_SetsPharmacistRoleType()
        {
            var user = new User(2, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            user.Role = "Pharmacist";
            this.service.SetFromUser(user);
            Assert.That(this.service.RoleType, Is.EqualTo(UserRole.Pharmacist));
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
        public void SetFromUser_NullUser_DoesNotChangeUserId()
        {
            this.service.UserId = 42;
            this.service.SetFromUser(null);
            Assert.That(this.service.UserId, Is.EqualTo(42));
        }

        [Test]
        public void SetFromUser_NullUser_DoesNotChangeRole()
        {
            this.service.RoleType = UserRole.Admin;
            this.service.SetFromUser(null);
            Assert.That(this.service.RoleType, Is.EqualTo(UserRole.Admin));
        }

        [Test]
        public void SetFromUser_EmptyRole_NotAdmin_FallsBackToClient()
        {
            var user = new User(2, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            user.Role = string.Empty;
            this.service.SetFromUser(user);
            Assert.That(this.service.RoleType, Is.EqualTo(UserRole.Client));
        }

        [Test]
        public void SetFromUser_EmptyRole_IsAdmin_FallsBackToAdmin()
        {
            var user = new User(2, "a@b.com", "123", "hash", true, false, "user1", false, 0);
            user.Role = string.Empty;
            this.service.SetFromUser(user);
            Assert.That(this.service.RoleType, Is.EqualTo(UserRole.Admin));
        }

        [Test]
        public void SetFromUser_CalledTwice_LastWins()
        {
            var user1 = new User(1, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            user1.Role = "Client";
            var user2 = new User(2, "c@d.com", "456", "hash2", true, false, "user2", false, 0);
            user2.Role = "Admin";
            this.service.SetFromUser(user1);
            this.service.SetFromUser(user2);
            Assert.That(this.service.UserId, Is.EqualTo(2));
            Assert.That(this.service.RoleType, Is.EqualTo(UserRole.Admin));
        }
    }
}
