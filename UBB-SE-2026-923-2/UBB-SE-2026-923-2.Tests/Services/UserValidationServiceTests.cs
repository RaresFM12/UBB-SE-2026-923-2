namespace UBB_SE_2026_923_2.Tests.Services
{
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class UserValidationServiceLogicTests
    {
        private UserValidationService userValidationService;

        [SetUp]
        public void Setup()
        {
            this.userValidationService = new UserValidationService();
        }

        [Test]
        public void IsCorrectEmailFormat_WhenEmailContainsAtSignAndDot_ReturnsTrue()
        {
            var validationResult = this.userValidationService.IsCorrectEmailFormat("user@test.com");

            Assert.That(validationResult, Is.True);
        }

        [Test]
        public void IsCorrectEmailFormat_WhenEmailIsEmpty_ReturnsFalse()
        {
            var validationResult = this.userValidationService.IsCorrectEmailFormat(string.Empty);

            Assert.That(validationResult, Is.False);
        }

        [Test]
        public void IsCorrectPasswordFormat_WhenPasswordContainsRequiredCharacters_ReturnsTrue()
        {
            var validationResult = this.userValidationService.IsCorrectPasswordFormat("Password1!");

            Assert.That(validationResult, Is.True);
        }

        [Test]
        public void IsCorrectPasswordFormat_WhenPasswordHasNoSpecialCharacter_ReturnsFalse()
        {
            var validationResult = this.userValidationService.IsCorrectPasswordFormat("Password123");

            Assert.That(validationResult, Is.False);
        }

        [Test]
        public void IsCorrectPhoneNumberFormat_WhenPhoneNumberContainsOnlyDigits_ReturnsTrue()
        {
            var validationResult = this.userValidationService.IsCorrectPhoneNumberFormat("0744123456");

            Assert.That(validationResult, Is.True);
        }

        [Test]
        public void IsCorrectPhoneNumberFormat_WhenPhoneNumberContainsLetters_ReturnsFalse()
        {
            var validationResult = this.userValidationService.IsCorrectPhoneNumberFormat("0744ABC456");

            Assert.That(validationResult, Is.False);
        }

        [Test]
        public void IsCorrectUsernameFormat_WhenUsernameContainsLettersAndUnderscore_ReturnsTrue()
        {
            var validationResult = this.userValidationService.IsCorrectUsernameFormat("john_doe");

            Assert.That(validationResult, Is.True);
        }

        [Test]
        public void IsCorrectUsernameFormat_WhenUsernameContainsDigits_ReturnsFalse()
        {
            var validationResult = this.userValidationService.IsCorrectUsernameFormat("john123");

            Assert.That(validationResult, Is.False);
        }
    }
}