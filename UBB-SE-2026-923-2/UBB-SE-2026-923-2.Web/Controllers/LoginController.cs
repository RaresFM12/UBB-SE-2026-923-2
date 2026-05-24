namespace UBB_SE_2026_923_2.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Web.ViewModels;

    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly IUserAccountService userAccountService;

        public LoginController(IUserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;
        }

        [HttpGet]
        public IActionResult Index(string? returnUrl = null)
        {
            return this.View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            try
            {
                // All credential validation, lookup, password verification and
                // disabled-account checks live in UserAccountService.Login.
                // The controller's only job is to translate the result into a
                // cookie or a model-state error.
                this.userAccountService.Login(model.Email, model.Password);
            }
            catch (Exception exception)
            {
                this.ModelState.AddModelError(string.Empty, exception.Message);
                return this.View(model);
            }

            User? user = this.userAccountService.CurrentUser;
            if (user == null)
            {
                this.ModelState.AddModelError(string.Empty, "Login failed.");
                return this.View(model);
            }

            await this.SignInWithClaimsAsync(user);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && this.Url.IsLocalUrl(model.ReturnUrl))
            {
                return this.Redirect(model.ReturnUrl);
            }

            return this.RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return this.RedirectToAction("Index", "Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return this.View();
        }

        private async Task SignInWithClaimsAsync(User user)
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, string.IsNullOrWhiteSpace(user.Role) ? "Client" : user.Role),
            };

            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new(identity);

            await this.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                });
        }
    }
}
