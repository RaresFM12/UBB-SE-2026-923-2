namespace UBB_SE_2026_923_2.Tests.Services
{
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class CurrentUserServiceLogicTests
    {
        private CurrentUserService currentUserService;

        [SetUp]
        public void Setup()
        {
            this.currentUserService = new CurrentUserService();
            this.currentUserService.UserId = 0;
            this.currentUserService.RoleType = UserRole.Client;
        }

        [Test]
        public void SetFromUser_WhenUserIsNull_DoesNotChangeExistingUserIdentifier()
        {
            this.currentUserService.UserId = 15;

            this.currentUserService.SetFromUser(null);

            Assert.That(this.currentUserService.UserId, Is.EqualTo(15));
        }

        [Test]
        public void SetFromUser_WhenUserRoleCanBeParsedIgnoringCase_SetsParsedRoleType()
        {
            var userWithLowercaseAdminRole = new User(25, "admin@test.com", "1234567890", "hashedPassword", false, false, "admin", false, 0)
            {
                Role = "admin",
            };

            this.currentUserService.SetFromUser(userWithLowercaseAdminRole);

            Assert.That(this.currentUserService.RoleType, Is.EqualTo(UserRole.Admin));
        }

        [Test]
        public void SetFromUser_WhenUserRoleCannotBeParsedAndUserIsAdmin_FallsBackToAdminRoleType()
        {
            var administratorWithUnknownRole = new User(30, "admin@test.com", "1234567890", "hashedPassword", true, false, "admin", false, 0)
            {
                Role = "UnknownRole",
            };

            this.currentUserService.SetFromUser(administratorWithUnknownRole);

            Assert.That(this.currentUserService.RoleType, Is.EqualTo(UserRole.Admin));
        }
    }
}