using NUnit.Framework;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.Tests.Services
{
    [TestFixture]
    public class CurrentUserServiceTests
    {
        private CurrentUserService service;

        [SetUp]
        public void Setup()
        {
            service = new CurrentUserService();
            service.UserId = 0;
            service.RoleType = UserRole.Client;
        }

        [Test]
        public void UserId_SetAndGet_ReturnsCorrectValue()
        {
            service.UserId = 42;
            Assert.That(service.UserId, Is.EqualTo(42));
        }

        [Test]
        public void RoleType_SetAndGet_ReturnsCorrectValue()
        {
            service.RoleType = UserRole.Admin;
            Assert.That(service.RoleType, Is.EqualTo(UserRole.Admin));
        }

        [Test]
        public void Role_WhenClient_ReturnsClientString()
        {
            service.RoleType = UserRole.Client;
            Assert.That(service.Role, Is.EqualTo("Client"));
        }

        [Test]
        public void Role_WhenAdmin_ReturnsAdminString()
        {
            service.RoleType = UserRole.Admin;
            Assert.That(service.Role, Is.EqualTo("Admin"));
        }

        [Test]
        public void SetFromUser_NullUser_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => service.SetFromUser(null));
        }

        [Test]
        public void SetFromUser_UserWithClientRole_SetsClientRoleType()
        {
            var user = new User(1, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            user.Role = "Client";
            service.SetFromUser(user);
            Assert.That(service.RoleType, Is.EqualTo(UserRole.Client));
            Assert.That(service.UserId, Is.EqualTo(1));
        }

        [Test]
        public void SetFromUser_UserWithAdminRole_SetsAdminRoleType()
        {
            var user = new User(5, "a@b.com", "123", "hash", true, false, "admin1", false, 0);
            user.Role = "Admin";
            service.SetFromUser(user);
            Assert.That(service.RoleType, Is.EqualTo(UserRole.Admin));
        }

        [Test]
        public void SetFromUser_UserWithUnknownRole_FallsBackToIsAdmin()
        {
            var user = new User(3, "a@b.com", "123", "hash", true, false, "user1", false, 0);
            user.Role = "UnknownRole";
            service.SetFromUser(user);
            Assert.That(service.RoleType, Is.EqualTo(UserRole.Admin));
        }

        [Test]
        public void SetFromUser_UserWithUnknownRoleNotAdmin_FallsBackToClient()
        {
            var user = new User(3, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            user.Role = "UnknownRole";
            service.SetFromUser(user);
            Assert.That(service.RoleType, Is.EqualTo(UserRole.Client));
        }

        [Test]
        public void SetFromUser_SetsUserId()
        {
            var user = new User(99, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            user.Role = "Client";
            service.SetFromUser(user);
            Assert.That(service.UserId, Is.EqualTo(99));
        }

        [Test]
        public void UserId_DefaultIsZero()
        {
            Assert.That(service.UserId, Is.EqualTo(0));
        }

        [Test]
        public void RoleType_DefaultIsClient()
        {
            Assert.That(service.RoleType, Is.EqualTo(UserRole.Client));
        }

        [Test]
        public void Role_DefaultIsClientString()
        {
            Assert.That(service.Role, Is.EqualTo("Client"));
        }

        [Test]
        public void UserId_SetNegative_Works()
        {
            service.UserId = -1;
            Assert.That(service.UserId, Is.EqualTo(-1));
        }

        [Test]
        public void UserId_SetMaxInt_Works()
        {
            service.UserId = int.MaxValue;
            Assert.That(service.UserId, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void RoleType_SetDoctor_Works()
        {
            service.RoleType = UserRole.Doctor;
            Assert.That(service.RoleType, Is.EqualTo(UserRole.Doctor));
            Assert.That(service.Role, Is.EqualTo("Doctor"));
        }

        [Test]
        public void RoleType_SetPharmacist_Works()
        {
            service.RoleType = UserRole.Pharmacist;
            Assert.That(service.RoleType, Is.EqualTo(UserRole.Pharmacist));
            Assert.That(service.Role, Is.EqualTo("Pharmacist"));
        }

        [Test]
        public void SetFromUser_DoctorRole_SetsDoctorRoleType()
        {
            var user = new User(2, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            user.Role = "Doctor";
            service.SetFromUser(user);
            Assert.That(service.RoleType, Is.EqualTo(UserRole.Doctor));
        }

        [Test]
        public void SetFromUser_PharmacistRole_SetsPharmacistRoleType()
        {
            var user = new User(2, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            user.Role = "Pharmacist";
            service.SetFromUser(user);
            Assert.That(service.RoleType, Is.EqualTo(UserRole.Pharmacist));
        }

        [Test]
        public void SetFromUser_CaseInsensitiveRole_Works()
        {
            var user = new User(2, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            user.Role = "admin";
            service.SetFromUser(user);
            Assert.That(service.RoleType, Is.EqualTo(UserRole.Admin));
        }

        [Test]
        public void SetFromUser_NullUser_DoesNotChangeUserId()
        {
            service.UserId = 42;
            service.SetFromUser(null);
            Assert.That(service.UserId, Is.EqualTo(42));
        }

        [Test]
        public void SetFromUser_NullUser_DoesNotChangeRole()
        {
            service.RoleType = UserRole.Admin;
            service.SetFromUser(null);
            Assert.That(service.RoleType, Is.EqualTo(UserRole.Admin));
        }

        [Test]
        public void SetFromUser_EmptyRole_NotAdmin_FallsBackToClient()
        {
            var user = new User(2, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            user.Role = "";
            service.SetFromUser(user);
            Assert.That(service.RoleType, Is.EqualTo(UserRole.Client));
        }

        [Test]
        public void SetFromUser_EmptyRole_IsAdmin_FallsBackToAdmin()
        {
            var user = new User(2, "a@b.com", "123", "hash", true, false, "user1", false, 0);
            user.Role = "";
            service.SetFromUser(user);
            Assert.That(service.RoleType, Is.EqualTo(UserRole.Admin));
        }

        [Test]
        public void SetFromUser_CalledTwice_LastWins()
        {
            var user1 = new User(1, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            user1.Role = "Client";
            var user2 = new User(2, "c@d.com", "456", "hash2", true, false, "user2", false, 0);
            user2.Role = "Admin";
            service.SetFromUser(user1);
            service.SetFromUser(user2);
            Assert.That(service.UserId, Is.EqualTo(2));
            Assert.That(service.RoleType, Is.EqualTo(UserRole.Admin));
        }
    }
}
