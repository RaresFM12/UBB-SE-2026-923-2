namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class UserValidationService : IUserValidationService
    {
        private const string EmailPattern =
            @"^.+@.+\..+";

        private const string PasswordPattern =
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#%^*])[A-Za-z\d!@#%^*]{8,}$";

        private const string PhoneNumberPattern =
            @"^[0-9]+$";

        private const string UsernamePattern =
            @"^[A-Za-z_]+$";

        public bool IsCorrectEmailFormat(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return Regex.IsMatch(email.Trim(), EmailPattern);
        }

        public bool IsCorrectPasswordFormat(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            return Regex.IsMatch(password, PasswordPattern);
        }

        public bool IsCorrectPhoneNumberFormat(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            return Regex.IsMatch(phoneNumber.Trim(), PhoneNumberPattern);
        }

        public bool IsCorrectUsernameFormat(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            return Regex.IsMatch(username.Trim(), UsernamePattern);
        }
    }
}
