using NUnit.Framework;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.Tests.Services
{
    [TestFixture]
    public class UserValidationServiceTests
    {
        private UserValidationService validationService;

        [SetUp]
        public void Setup()
        {
            validationService = new UserValidationService();
        }

        // ========== Email Validation ==========

        [Test]
        public void IsCorrectEmailFormat_ValidEmail_ReturnsTrue()
        {
            Assert.That(validationService.IsCorrectEmailFormat("user@example.com"), Is.True);
        }

        [Test]
        public void IsCorrectEmailFormat_NullEmail_ReturnsFalse()
        {
            Assert.That(validationService.IsCorrectEmailFormat(null!), Is.False);
        }

        [Test]
        public void IsCorrectEmailFormat_EmptyEmail_ReturnsFalse()
        {
            Assert.That(validationService.IsCorrectEmailFormat(""), Is.False);
        }

        [Test]
        public void IsCorrectEmailFormat_NoAtSymbol_ReturnsFalse()
        {
            Assert.That(validationService.IsCorrectEmailFormat("userexample.com"), Is.False);
        }

        [Test]
        public void IsCorrectEmailFormat_NoDotAfterAt_ReturnsFalse()
        {
            Assert.That(validationService.IsCorrectEmailFormat("user@example"), Is.False);
        }

        // ========== Password Validation ==========

        [Test]
        public void IsCorrectPasswordFormat_ValidPassword_ReturnsTrue()
        {
            Assert.That(validationService.IsCorrectPasswordFormat("Abcdef1!"), Is.True);
        }

        [Test]
        public void IsCorrectPasswordFormat_TooShort_ReturnsFalse()
        {
            Assert.That(validationService.IsCorrectPasswordFormat("Ab1!"), Is.False);
        }

        [Test]
        public void IsCorrectPasswordFormat_NoUppercase_ReturnsFalse()
        {
            Assert.That(validationService.IsCorrectPasswordFormat("abcdef1!"), Is.False);
        }

        [Test]
        public void IsCorrectPasswordFormat_NoLowercase_ReturnsFalse()
        {
            Assert.That(validationService.IsCorrectPasswordFormat("ABCDEF1!"), Is.False);
        }

        [Test]
        public void IsCorrectPasswordFormat_NoDigit_ReturnsFalse()
        {
            Assert.That(validationService.IsCorrectPasswordFormat("Abcdefg!"), Is.False);
        }

        [Test]
        public void IsCorrectPasswordFormat_NoSpecialChar_ReturnsFalse()
        {
            Assert.That(validationService.IsCorrectPasswordFormat("Abcdefg1"), Is.False);
        }

        // ========== Phone Number Validation ==========

        [Test]
        public void IsCorrectPhoneNumberFormat_ValidNumber_ReturnsTrue()
        {
            Assert.That(validationService.IsCorrectPhoneNumberFormat("0711111111"), Is.True);
        }

        [Test]
        public void IsCorrectPhoneNumberFormat_ContainsLetters_ReturnsFalse()
        {
            Assert.That(validationService.IsCorrectPhoneNumberFormat("07111abc"), Is.False);
        }

        [Test]
        public void IsCorrectPhoneNumberFormat_Empty_ReturnsFalse()
        {
            Assert.That(validationService.IsCorrectPhoneNumberFormat(""), Is.False);
        }

        // ========== Username Validation ==========

        [Test]
        public void IsCorrectUsernameFormat_ValidUsername_ReturnsTrue()
        {
            Assert.That(validationService.IsCorrectUsernameFormat("john_doe"), Is.True);
        }

        [Test]
        public void IsCorrectUsernameFormat_ContainsDigits_ReturnsFalse()
        {
            Assert.That(validationService.IsCorrectUsernameFormat("john123"), Is.False);
        }

        [Test]
        public void IsCorrectUsernameFormat_ContainsSpecialChars_ReturnsFalse()
        {
            Assert.That(validationService.IsCorrectUsernameFormat("john@doe"), Is.False);
        }

        [Test]
        public void IsCorrectUsernameFormat_OnlyLetters_ReturnsTrue()
        {
            Assert.That(validationService.IsCorrectUsernameFormat("johndoe"), Is.True);
        }
    }
}
