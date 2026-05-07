using NUnit.Framework;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.Tests.Services
{
    [TestFixture]
    public class SecurityServiceTests
    {
        private SecurityService securityService;

        [SetUp]
        public void Setup()
        {
            securityService = new SecurityService();
        }

        [Test]
        public void HashPassword_ReturnsInputUnchanged()
        {
            var result = securityService.HashPassword("mypassword");

            Assert.That(result, Is.EqualTo("mypassword"));
        }

        [Test]
        public void VerifyPassword_MatchingStrings_ReturnsTrue()
        {
            Assert.That(securityService.VerifyPassword("abc123", "abc123"), Is.True);
        }

        [Test]
        public void VerifyPassword_DifferentStrings_ReturnsFalse()
        {
            Assert.That(securityService.VerifyPassword("abc123", "wrong"), Is.False);
        }
    }
}
