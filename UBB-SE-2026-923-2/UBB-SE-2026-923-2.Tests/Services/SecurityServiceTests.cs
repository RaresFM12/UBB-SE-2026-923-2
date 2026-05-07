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

        [Test]
        public void HashPassword_EmptyString_ReturnsEmpty()
        {
            Assert.That(this.securityService.HashPassword(string.Empty), Is.EqualTo(string.Empty));
        }

        [Test]
        public void HashPassword_NullInput_ReturnsNull()
        {
            Assert.That(this.securityService.HashPassword(null), Is.Null);
        }

        [Test]
        public void HashPassword_SpecialChars_ReturnsSame()
        {
            Assert.That(this.securityService.HashPassword("!@#$%^&*()"), Is.EqualTo("!@#$%^&*()"));
        }

        [Test]
        public void HashPassword_LongString_ReturnsSame()
        {
            var longStr = new string('a', 1000);
            Assert.That(this.securityService.HashPassword(longStr), Is.EqualTo(longStr));
        }

        [Test]
        public void HashPassword_WhitespaceOnly_ReturnsSame()
        {
            Assert.That(this.securityService.HashPassword("   "), Is.EqualTo("   "));
        }

        [Test]
        public void VerifyPassword_BothEmpty_ReturnsTrue()
        {
            Assert.That(this.securityService.VerifyPassword(string.Empty, string.Empty), Is.True);
        }

        [Test]
        public void VerifyPassword_BothNull_ReturnsTrue()
        {
            Assert.That(this.securityService.VerifyPassword(null, null), Is.True);
        }

        [Test]
        public void VerifyPassword_OneNull_ReturnsFalse()
        {
            Assert.That(this.securityService.VerifyPassword(null, "abc"), Is.False);
        }

        [Test]
        public void VerifyPassword_CaseSensitive_ReturnsFalse()
        {
            Assert.That(this.securityService.VerifyPassword("ABC", "abc"), Is.False);
        }

        [Test]
        public void VerifyPassword_WhitespaceDifference_ReturnsFalse()
        {
            Assert.That(this.securityService.VerifyPassword("abc ", "abc"), Is.False);
        }

        [Test]
        public void VerifyPassword_SpecialCharsMatch_ReturnsTrue()
        {
            Assert.That(this.securityService.VerifyPassword("!@#$", "!@#$"), Is.True);
        }

        [Test]
        public void VerifyPassword_UnicodeMatch_ReturnsTrue()
        {
            Assert.That(this.securityService.VerifyPassword("ñoño", "ñoño"), Is.True);
        }

        [Test]
        public void VerifyPassword_UnicodeNoMatch_ReturnsFalse()
        {
            Assert.That(this.securityService.VerifyPassword("ñoño", "nono"), Is.False);
        }
    }
}
