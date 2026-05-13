namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IUserValidationService
    {
        bool IsCorrectEmailFormat(string email);

        bool IsCorrectPasswordFormat(string password);

        bool IsCorrectPhoneNumberFormat(string phoneNumber);

        bool IsCorrectUsernameFormat(string username);
    }
}