namespace UBB_SE_2026_923_2.Tests.Services
{
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class SecurityServiceLogicTests
    {
        private SecurityService securityService;

        [SetUp]
        public void Setup()
        {
            this.securityService = new SecurityService();
        }

        [Test]
        public void HashPassword_WhenPasswordIsProvided_ReturnsSamePassword()
        {
            var plainTextPassword = "Password123!";

            var hashedPassword = this.securityService.HashPassword(plainTextPassword);

            Assert.That(hashedPassword, Is.EqualTo(plainTextPassword));
        }

        [Test]
        public void VerifyPassword_WhenPasswordMatchesStoredPassword_ReturnsTrue()
        {
            var verificationResult = this.securityService.VerifyPassword("Password123!", "Password123!");

            Assert.That(verificationResult, Is.True);
        }

        [Test]
        public void VerifyPassword_WhenPasswordDoesNotMatchStoredPassword_ReturnsFalse()
        {
            var verificationResult = this.securityService.VerifyPassword("WrongPassword123!", "Password123!");

            Assert.That(verificationResult, Is.False);
        }
    }
}