namespace UBB_SE_2026_923_2.Tests.Services
{
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class SecurityServiceTests
    {
        private SecurityService securityService;

        [SetUp]
        public void Setup()
        {
            this.securityService = new SecurityService();
        }

        [Test]
        public void HashPassword_ReturnsInputUnchanged()
        {
            // Note: This is currently testing a placeholder method.
            // When actual hashing is implemented, update this test.
            var result = this.securityService.HashPassword("mypassword");
            Assert.That(result, Is.EqualTo("mypassword"));
        }

        [Test]
        public void VerifyPassword_MatchingStrings_ReturnsTrue()
        {
            Assert.That(this.securityService.VerifyPassword("abc123", "abc123"), Is.True);
        }

        [Test]
        public void VerifyPassword_DifferentStrings_ReturnsFalse()
        {
            Assert.That(this.securityService.VerifyPassword("abc123", "wrong"), Is.False);
        }
    }
}