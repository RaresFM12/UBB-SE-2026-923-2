namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;

    public class UserAccountService : IUserAccountService
    {
        private const string IdSearchPrefix = "id:";
        private const string UsernameSearchPrefix = "username:";
        private const string EmailSearchPrefix = "mail:";

        public User? CurrentUser { get; private set; }

        public IUsersRepository UsersRepository { get; private set; }

        private readonly ISecurityService securityService;
        private readonly IUserValidationService userValidationService;

        public UserAccountService(
            IUsersRepository usersRepository,
            ISecurityService securityService,
            IUserValidationService userValidationService)
        {
            this.CurrentUser = null;
            this.UsersRepository = usersRepository;
            this.securityService = securityService;
            this.userValidationService = userValidationService;
        }

        public void Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("E-mail cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be empty.");
            }

            if (!this.userValidationService.IsCorrectEmailFormat(email))
            {
                throw new Exception("Not a valid e-mail");
            }

            User foundUser = this.UsersRepository.GetUserByEmail(email);

            if (foundUser == null)
            {
                throw new Exception("E-mail not found");
            }

            if (foundUser.IsDisabled)
            {
                throw new Exception("Account disabled");
            }

            if (!this.securityService.VerifyPassword(password, foundUser.PasswordHash))
            {
                throw new Exception("Incorrect password");
            }

            this.CurrentUser = foundUser;
        }

        public User? LoadCurrentUser(int userId)
        {
            if (userId <= 0)
            {
                this.CurrentUser = null;
                return null;
            }

            this.CurrentUser = this.UsersRepository.GetUserById(userId);
            return this.CurrentUser;
        }

        public void Register(
            string email,
            string password,
            string confirmPassword,
            string username,
            string phoneNumber,
            string role = "Client")
        {
            if (!this.userValidationService.IsCorrectEmailFormat(email))
            {
                throw new Exception("Not a valid email format\nmust be <text>@<text>.<text>");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("Password cannot be empty.");
            }

            if (password != confirmPassword)
            {
                throw new Exception("Passwords don't match.");
            }

            if (!this.userValidationService.IsCorrectPasswordFormat(password))
            {
                throw new Exception("Password must be 8+ characters and include uppercase, lowercase, digit, and a special character (!@#%^*).");
            }

            if (username != null && !this.userValidationService.IsCorrectUsernameFormat(username))
            {
                throw new Exception("Username is not valid, must contain only letters and/or _");
            }

            phoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? string.Empty : phoneNumber;

            if (!string.IsNullOrEmpty(phoneNumber) && !this.userValidationService.IsCorrectPhoneNumberFormat(phoneNumber))
            {
                throw new Exception("Phone number must contain only digits");
            }

            User user = this.UsersRepository.GetUserByEmail(email);
            if (user != null)
            {
                throw new Exception("Email already linked to an account");
            }

            string hashedPassword = this.securityService.HashPassword(password);
            bool discountNotificationsSetting = false;
            bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            this.UsersRepository.AddUser(email, phoneNumber, hashedPassword, username, discountNotificationsSetting, isAdmin: isAdmin, role: role);
            this.CurrentUser = this.UsersRepository.GetUserByEmail(email);
        }

        public void UpdateProfile(string newUsername, string newPhoneNumber)
        {
            if (this.CurrentUser == null)
            {
                throw new Exception("Not logged in");
            }

            if (string.IsNullOrEmpty(newUsername))
            {
                newUsername = this.CurrentUser.Email.Split("@")[0];
            }
            else if (!this.userValidationService.IsCorrectUsernameFormat(newUsername))
            {
                throw new Exception("Invalid new username");
            }

            if (!string.IsNullOrEmpty(newPhoneNumber) &&
                !this.userValidationService.IsCorrectPhoneNumberFormat(newPhoneNumber))
            {
                throw new Exception("Invalid new phone number");
            }

            newPhoneNumber = string.IsNullOrEmpty(newPhoneNumber)
                ? this.CurrentUser.PhoneNumber
                : newPhoneNumber;

            this.CurrentUser.PhoneNumber = newPhoneNumber;
            this.CurrentUser.Username = newUsername;
            this.UsersRepository.UpdateUser(this.CurrentUser);
        }

        public void ChangePassword(string oldPassword, string newPassword, string confirmNewPassword)
        {
            if (this.CurrentUser == null)
            {
                throw new Exception("Not logged in");
            }

            if (!this.securityService.VerifyPassword(oldPassword, this.CurrentUser.PasswordHash))
            {
                throw new Exception("Incorrect password");
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new Exception("New password cannot be empty.");
            }

            if (!this.userValidationService.IsCorrectPasswordFormat(newPassword))
            {
                throw new Exception("Password must be 8+ characters and include uppercase, lowercase, digit, and a special character (!@#%^*).");
            }

            if (newPassword != confirmNewPassword)
            {
                throw new Exception("Passwords don't match");
            }

            string newPasswordHash = this.securityService.HashPassword(newPassword);

            this.CurrentUser.PasswordHash = newPasswordHash;
            this.UsersRepository.UpdateUser(this.CurrentUser);
        }

        public List<User> SearchUsers(string query)
        {
            if (this.CurrentUser == null)
                throw new Exception("Not logged in");

            if (!this.CurrentUser.IsAdmin)
                throw new Exception($"Current user with id={this.CurrentUser.Id} not an admin");

            query = query.Trim();
            List<User> queriedUsers = this.UsersRepository.GetAllUsers();

            if (string.IsNullOrWhiteSpace(query))
                return queriedUsers;

            if (query.StartsWith(IdSearchPrefix))
            {
                try
                {
                    int id = int.Parse(query.Substring(IdSearchPrefix.Length));
                    return queriedUsers.Where(user => user.Id == id).ToList();
                }
                catch (FormatException) { }
            }

            if (query.StartsWith(UsernameSearchPrefix))
            {
                string username = query.Substring(UsernameSearchPrefix.Length);
                return queriedUsers.Where(user => user.Username.Contains(username)).ToList();
            }

            if (query.StartsWith(EmailSearchPrefix))
            {
                string mail = query.Substring(EmailSearchPrefix.Length);
                return queriedUsers.Where(user => user.Email.Contains(mail)).ToList();
            }

            // general search across email, username and phone
            return queriedUsers.Where(user =>
                user.Email.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                user.Username.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                user.PhoneNumber.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        public void PromoteToAdmin(User client)
        {
            if (this.CurrentUser == null)
            {
                throw new Exception("Not logged in");
            }

            if (!this.CurrentUser.IsAdmin)
            {
                throw new Exception($"Current user with id={this.CurrentUser.Id} not an admin");
            }

            if (client.IsAdmin || client.IsDisabled)
            {
                return;
            }

            client.IsAdmin = true;
            this.UsersRepository.UpdateUser(client);
        }

        public void DisableAccount(User client)
        {
            if (this.CurrentUser == null)
            {
                throw new Exception("Not logged in");
            }

            if (!this.CurrentUser.IsAdmin)
            {
                throw new Exception($"Current user with id={this.CurrentUser.Id} not an admin");
            }

            if (client.IsAdmin || client.IsDisabled)
            {
                return;
            }

            client.IsDisabled = true;
            this.UsersRepository.UpdateUser(client);
        }

        public void Logout()
        {
            this.CurrentUser = null;
        }
    }
}