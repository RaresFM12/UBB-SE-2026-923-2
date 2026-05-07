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

        [Test]
        public void IsCorrectEmailFormat_ValidEmail_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectEmailFormat("user@example.com"), Is.True);
        }

        [Test]
        public void IsCorrectEmailFormat_NullEmail_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectEmailFormat(null!), Is.False);
        }

        [Test]
        public void IsCorrectEmailFormat_EmptyEmail_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectEmailFormat(string.Empty), Is.False);
        }

        [Test]
        public void IsCorrectEmailFormat_NoAtSymbol_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectEmailFormat("userexample.com"), Is.False);
        }

        [Test]
        public void IsCorrectEmailFormat_NoDotAfterAt_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectEmailFormat("user@example"), Is.False);
        }

        [Test]
        public void IsCorrectPasswordFormat_ValidPassword_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectPasswordFormat("Abcdef1!"), Is.True);
        }

        [Test]
        public void IsCorrectPasswordFormat_TooShort_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectPasswordFormat("Ab1!"), Is.False);
        }

        [Test]
        public void IsCorrectPasswordFormat_NoUppercase_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectPasswordFormat("abcdef1!"), Is.False);
        }

        [Test]
        public void IsCorrectPasswordFormat_NoLowercase_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectPasswordFormat("ABCDEF1!"), Is.False);
        }

        [Test]
        public void IsCorrectPasswordFormat_NoDigit_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectPasswordFormat("Abcdefg!"), Is.False);
        }

        [Test]
        public void IsCorrectPasswordFormat_NoSpecialChar_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectPasswordFormat("Abcdefg1"), Is.False);
        }

        [Test]
        public void IsCorrectPhoneNumberFormat_ValidNumber_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectPhoneNumberFormat("0711111111"), Is.True);
        }

        [Test]
        public void IsCorrectPhoneNumberFormat_ContainsLetters_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectPhoneNumberFormat("07111abc"), Is.False);
        }

        [Test]
        public void IsCorrectPhoneNumberFormat_Empty_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectPhoneNumberFormat(string.Empty), Is.False);
        }

        [Test]
        public void IsCorrectUsernameFormat_ValidUsername_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectUsernameFormat("john_doe"), Is.True);
        }

        [Test]
        public void IsCorrectUsernameFormat_ContainsDigits_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectUsernameFormat("john123"), Is.False);
        }

        [Test]
        public void IsCorrectUsernameFormat_ContainsSpecialChars_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectUsernameFormat("john@doe"), Is.False);
        }

        [Test]
        public void IsCorrectUsernameFormat_OnlyLetters_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectUsernameFormat("johndoe"), Is.True);
        }

        [Test]
        public void IsCorrectEmailFormat_MultipleAts_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectEmailFormat("user@sub@domain.com"), Is.True);
        }

        [Test]
        public void IsCorrectEmailFormat_SubdomainEmail_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectEmailFormat("user@sub.domain.com"), Is.True);
        }

        [Test]
        public void IsCorrectEmailFormat_WhitespaceOnly_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectEmailFormat("   "), Is.False);
        }

        [Test]
        public void IsCorrectEmailFormat_DotBeforeAt_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectEmailFormat("user.name@domain.com"), Is.True);
        }

        [Test]
        public void IsCorrectEmailFormat_PlusInLocal_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectEmailFormat("user+tag@domain.com"), Is.True);
        }

        [Test]
        public void IsCorrectPasswordFormat_NullPassword_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectPasswordFormat(null!), Is.False);
        }

        [Test]
        public void IsCorrectPasswordFormat_EmptyPassword_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectPasswordFormat(string.Empty), Is.False);
        }

        [Test]
        public void IsCorrectPasswordFormat_WhitespaceOnly_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectPasswordFormat("   "), Is.False);
        }

        [Test]
        public void IsCorrectPasswordFormat_ExactlyEightChars_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectPasswordFormat("Abcdef1!"), Is.True);
        }

        [Test]
        public void IsCorrectPasswordFormat_LongValidPassword_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectPasswordFormat("Abcdefghij1!"), Is.True);
        }

        [Test]
        public void IsCorrectPasswordFormat_AllCriteriaMet_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectPasswordFormat("Test123!@"), Is.True);
        }

        [Test]
        public void IsCorrectPhoneNumberFormat_NullPhone_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectPhoneNumberFormat(null!), Is.False);
        }

        [Test]
        public void IsCorrectPhoneNumberFormat_WhitespaceOnly_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectPhoneNumberFormat("   "), Is.False);
        }

        [Test]
        public void IsCorrectPhoneNumberFormat_SingleDigit_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectPhoneNumberFormat("5"), Is.True);
        }

        [Test]
        public void IsCorrectPhoneNumberFormat_WithDashes_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectPhoneNumberFormat("071-111-111"), Is.False);
        }

        [Test]
        public void IsCorrectPhoneNumberFormat_WithSpaces_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectPhoneNumberFormat("071 111 111"), Is.False);
        }

        [Test]
        public void IsCorrectPhoneNumberFormat_WithPlusSign_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectPhoneNumberFormat("+40711111111"), Is.False);
        }

        [Test]
        public void IsCorrectUsernameFormat_NullUsername_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectUsernameFormat(null!), Is.False);
        }

        [Test]
        public void IsCorrectUsernameFormat_EmptyUsername_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectUsernameFormat(string.Empty), Is.False);
        }

        [Test]
        public void IsCorrectUsernameFormat_WhitespaceOnly_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectUsernameFormat("   "), Is.False);
        }

        [Test]
        public void IsCorrectUsernameFormat_WithUnderscore_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectUsernameFormat("john_doe_smith"), Is.True);
        }

        [Test]
        public void IsCorrectUsernameFormat_SingleChar_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectUsernameFormat("a"), Is.True);
        }

        [Test]
        public void IsCorrectUsernameFormat_AllUppercase_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectUsernameFormat("JOHN"), Is.True);
        }

        [Test]
        public void IsCorrectUsernameFormat_MixedCase_ReturnsTrue()
        {
            Assert.That(this.validationService.IsCorrectUsernameFormat("JohnDoe"), Is.True);
        }

        [Test]
        public void IsCorrectUsernameFormat_WithSpaces_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectUsernameFormat("john doe"), Is.False);
        }

        [Test]
        public void IsCorrectUsernameFormat_WithDot_ReturnsFalse()
        {
            Assert.That(this.validationService.IsCorrectUsernameFormat("john.doe"), Is.False);
        }
    }
}
