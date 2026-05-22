namespace UBB_SE_2026_923_2.Web.Controllers
{
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Web.ViewModels;

    [Authorize(Roles = "Client")]
    public class NotificationsController : Controller
    {
        private readonly IAdminService adminService;
        private readonly IUserAccountService userAccountService;

        public NotificationsController(IAdminService adminService, IUserAccountService userAccountService)
        {
            this.adminService = adminService;
            this.userAccountService = userAccountService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var idClaim = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out int userId))
            {
                return this.RedirectToAction("Index", "Login");
            }

            this.userAccountService.LoadCurrentUser(userId);
            var currentUser = this.userAccountService.CurrentUser;

            if (currentUser == null)
            {
                return this.RedirectToAction("Index", "Login");
            }

            var notifications = this.adminService.GetNotificationsForUser(currentUser);

            var viewModel = new NotificationsIndexViewModel
            {
                Notifications = notifications
                    .Select(notification => new NotificationItemViewModel
                    {
                        Title = notification.Title,
                        Body = notification.Message,
                        ActionButtonText = notification.ActionButtonText,
                    })
                    .ToList(),
            };

            return this.View(viewModel);
        }
    }
}
