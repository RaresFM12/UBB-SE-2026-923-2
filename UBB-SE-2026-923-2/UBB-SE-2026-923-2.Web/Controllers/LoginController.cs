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
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Web.Models;

    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly IUsersRepository usersRepository;
        private readonly ISecurityService securityService;
        private readonly IUserValidationService userValidationService;

        public LoginController(
            IUsersRepository usersRepository,
            ISecurityService securityService,
            IUserValidationService userValidationService)
        {
            this.usersRepository = usersRepository;
            this.securityService = securityService;
            this.userValidationService = userValidationService;
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
                if (!this.userValidationService.IsCorrectEmailFormat(model.Email))
                {
                    this.ModelState.AddModelError(string.Empty, "Not a valid e-mail.");
                    return this.View(model);
                }

                User foundUser = this.usersRepository.GetUserByEmail(model.Email);

                if (foundUser == null)
                {
                    this.ModelState.AddModelError(string.Empty, "E-mail not found.");
                    return this.View(model);
                }

                if (foundUser.IsDisabled)
                {
                    this.ModelState.AddModelError(string.Empty, "Account is disabled.");
                    return this.View(model);
                }

                if (!this.securityService.VerifyPassword(model.Password, foundUser.PasswordHash))
                {
                    this.ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return this.View(model);
                }

                List<Claim> claims = new()
                {
                    new Claim(ClaimTypes.NameIdentifier, foundUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, foundUser.Username ?? foundUser.Email),
                    new Claim(ClaimTypes.Email, foundUser.Email),
                    new Claim(ClaimTypes.Role, string.IsNullOrWhiteSpace(foundUser.Role) ? "Client" : foundUser.Role),
                };

                if (foundUser.IsAdmin)
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

                if (!string.IsNullOrEmpty(model.ReturnUrl) && this.Url.IsLocalUrl(model.ReturnUrl))
                {
                    return this.Redirect(model.ReturnUrl);
                }

                return this.RedirectToAction("Index", "Home");
            }
            catch (Exception exception)
            {
                this.ModelState.AddModelError(string.Empty, exception.Message);
                return this.View(model);
            }
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
    }
}
