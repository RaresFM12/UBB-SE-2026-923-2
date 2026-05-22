namespace UBB_SE_2026_923_2.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Web.ViewModels;

    [Authorize]
    public class UserAccountsController : Controller
    {
        private readonly IUserAccountService userAccountService;

        public UserAccountsController(IUserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Index(string? query)
        {
            if (this.LoadCurrentUser() == null)
            {
                return this.Forbid();
            }

            var model = new UserAccountListViewModel
            {
                Query = query ?? string.Empty,
            };

            try
            {
                List<User> users = this.userAccountService.SearchUsers(model.Query);
                model.Users = users.Select(MapListItem).ToList();
            }
            catch (Exception exception)
            {
                model.ErrorMessage = exception.Message;
            }

            if (this.TempData.TryGetValue("ErrorMessage", out var errorMessage))
            {
                model.ErrorMessage ??= errorMessage?.ToString();
            }

            return this.View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Details(int id)
        {
            if (this.LoadCurrentUser() == null)
            {
                return this.Forbid();
            }

            User? user = this.FindUserById(id);
            if (user == null)
            {
                return this.NotFound();
            }

            return this.View(MapDetails(user));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Create()
        {
            return this.View(new UserAccountCreateViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserAccountCreateViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            try
            {
                string role = string.IsNullOrWhiteSpace(model.Role) ? "Client" : model.Role;
                this.userAccountService.Register(
                    model.Email,
                    model.Password,
                    model.ConfirmPassword,
                    model.Username ?? string.Empty,
                    model.PhoneNumber ?? string.Empty,
                    role);
            }
            catch (Exception exception)
            {
                this.ModelState.AddModelError(string.Empty, exception.Message);
                return this.View(model);
            }

            if (this.User.Identity?.IsAuthenticated == true)
            {
                return this.RedirectToAction(nameof(this.Index));
            }

            return this.RedirectToAction("Index", "Login");
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            User? currentUser = this.LoadCurrentUser();
            if (currentUser == null)
            {
                return this.Forbid();
            }

            if (id.HasValue && id.Value != currentUser.Id)
            {
                return this.Forbid();
            }

            var model = new UserAccountEditViewModel
            {
                Id = currentUser.Id,
                Email = currentUser.Email,
                Username = currentUser.Username,
                PhoneNumber = currentUser.PhoneNumber,
            };

            if (this.TempData.TryGetValue("SuccessMessage", out var successMessage))
            {
                model.SuccessMessage = successMessage?.ToString();
            }

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UserAccountEditViewModel model)
        {
            User? currentUser = this.LoadCurrentUser();
            if (currentUser == null)
            {
                return this.Forbid();
            }

            model.Email = currentUser.Email;

            if (model.Id != currentUser.Id)
            {
                return this.Forbid();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            try
            {
                this.userAccountService.UpdateProfile(model.Username ?? string.Empty, model.PhoneNumber ?? string.Empty);
            }
            catch (Exception exception)
            {
                this.ModelState.AddModelError(string.Empty, exception.Message);
                return this.View(model);
            }

            this.TempData["SuccessMessage"] = "Profile updated.";
            return this.RedirectToAction(nameof(this.Edit), new { id = model.Id });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (this.LoadCurrentUser() == null)
            {
                return this.Forbid();
            }

            User? user = this.FindUserById(id);
            if (user == null)
            {
                return this.NotFound();
            }

            return this.View(MapDelete(user));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (this.LoadCurrentUser() == null)
            {
                return this.Forbid();
            }

            User? user = this.FindUserById(id);
            if (user == null)
            {
                return this.NotFound();
            }

            try
            {
                this.userAccountService.DisableAccount(user);
            }
            catch (Exception exception)
            {
                this.TempData["ErrorMessage"] = exception.Message;
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Promote(int id)
        {
            if (this.LoadCurrentUser() == null)
            {
                return this.Forbid();
            }

            User? user = this.FindUserById(id);
            if (user == null)
            {
                return this.NotFound();
            }

            try
            {
                this.userAccountService.PromoteToAdmin(user);
            }
            catch (Exception exception)
            {
                this.TempData["ErrorMessage"] = exception.Message;
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (this.LoadCurrentUser() == null)
            {
                return this.Forbid();
            }

            return this.View(new UserAccountChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(UserAccountChangePasswordViewModel model)
        {
            User? currentUser = this.LoadCurrentUser();
            if (currentUser == null)
            {
                return this.Forbid();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            try
            {
                this.userAccountService.ChangePassword(model.OldPassword, model.NewPassword, model.ConfirmPassword);
            }
            catch (Exception exception)
            {
                this.ModelState.AddModelError(string.Empty, exception.Message);
                return this.View(model);
            }

            this.TempData["SuccessMessage"] = "Password updated.";
            return this.RedirectToAction(nameof(this.Edit), new { id = currentUser.Id });
        }

        private User? LoadCurrentUser()
        {
            if (this.User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            string? idValue = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idValue, out int userId))
            {
                return null;
            }

            return this.userAccountService.LoadCurrentUser(userId);
        }

        private User? FindUserById(int id)
        {
            List<User> users = this.userAccountService.SearchUsers($"id:{id}");
            return users.FirstOrDefault();
        }

        private static UserAccountListItemViewModel MapListItem(User user)
        {
            return new UserAccountListItemViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsAdmin = user.IsAdmin,
                IsDisabled = user.IsDisabled,
            };
        }

        private static UserAccountDetailsViewModel MapDetails(User user)
        {
            return new UserAccountDetailsViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsAdmin = user.IsAdmin,
                IsDisabled = user.IsDisabled,
                LoyaltyPoints = user.LoyaltyPoints,
            };
        }

        private static UserAccountDeleteViewModel MapDelete(User user)
        {
            return new UserAccountDeleteViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsAdmin = user.IsAdmin,
                IsDisabled = user.IsDisabled,
            };
        }
    }
}
