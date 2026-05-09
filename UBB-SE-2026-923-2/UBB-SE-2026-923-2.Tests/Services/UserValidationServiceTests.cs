namespace UBB_SE_2026_923_2.Tests.Services
{
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class UserValidationServiceTests
    {
        private UserValidationService validationService;

        [SetUp]
        public void Setup()
        {
            this.validationService = new UserValidationService();
        }

        // --- Email Validation ---
        [Test]
        public void IsCorrectEmailFormat_ValidEmail_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectEmailFormat("user@example.com"), Is.True);
        }

        [Test]
        [TestCase("invalid-email")]
        [TestCase("user@example")]
        [TestCase(null)]
        [TestCase("   ")]
        public void IsCorrectEmailFormat_InvalidScenarios_ReturnsFalse(string? email)
        {
            // The ! tells the compiler we know what we are doing with the null value
            Assert.That(this.validationService.IsCorrectEmailFormat(email!), Is.False);
        }

        // --- Password Validation ---
        [Test]
        public void IsCorrectPasswordFormat_ValidPassword_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectPasswordFormat("Abcdef1!"), Is.True);
        }

        [Test]
        [TestCase("Ab1!")]         // Too short
        [TestCase("abcdef1!")]     // No uppercase
        [TestCase("ABCDEF1!")]     // No lowercase
        [TestCase("Abcdefg!")]     // No digit
        [TestCase("Abcdefg1")]     // No special char
        [TestCase(null)]           // Null input
        [TestCase("   ")]          // Whitespace
        public void IsCorrectPasswordFormat_InvalidScenarios_ReturnsFalse(string? password)
        {
            Assert.That(this.validationService.IsCorrectPasswordFormat(password!), Is.False);
        }

        // --- Phone Number Validation ---
        [Test]
        public void IsCorrectPhoneNumberFormat_ValidNumber_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectPhoneNumberFormat("0711111111"), Is.True);
        }

        [Test]
        [TestCase("0711abc")]      // Letters
        [TestCase("+40711")]       // Symbols
        [TestCase("071-111")]      // Dashes
        [TestCase("")]             // Empty
        [TestCase(null)]           // Null
        public void IsCorrectPhoneNumberFormat_InvalidScenarios_ReturnsFalse(string? phone)
        {
            Assert.That(this.validationService.IsCorrectPhoneNumberFormat(phone!), Is.False);
        }

        // --- Username Validation ---
        [Test]
        public void IsCorrectUsernameFormat_ValidUsername_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectUsernameFormat("john_doe"), Is.True);
        }

        [Test]
        [TestCase("john123")]      // Digits
        [TestCase("john@doe")]     // Special chars
        [TestCase("john.doe")]     // Dot
        [TestCase("   ")]          // Whitespace
        [TestCase(null)]           // Null
        public void IsCorrectUsernameFormat_InvalidScenarios_ReturnsFalse(string? username)
        {
            Assert.That(this.validationService.IsCorrectUsernameFormat(username!), Is.False);
        }
    }
}