namespace UBB_SE_2026_923_2.Web.Views
{
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Mvc;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Web.ViewModels;

    public class NotificationsViewComponent : ViewComponent
    {
        private readonly IAdminService adminService;
        private readonly IUserAccountService userAccountService;

        public NotificationsViewComponent(IAdminService adminService, IUserAccountService userAccountService)
        {
            this.adminService = adminService;
            this.userAccountService = userAccountService;
        }

        public IViewComponentResult Invoke()
        {
            if (this.UserClaimsPrincipal?.Identity?.IsAuthenticated != true)
            {
                return this.Content(string.Empty);
            }

            if (this.UserClaimsPrincipal.IsInRole("Client") != true)
            {
                return this.Content(string.Empty);
            }

            var idClaim = this.UserClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out int userId))
            {
                return this.Content(string.Empty);
            }

            this.userAccountService.LoadCurrentUser(userId);
            var currentUser = this.userAccountService.CurrentUser;

            if (currentUser == null)
            {
                return this.Content(string.Empty);
            }

            var notifications = this.adminService.GetNotificationsForUser(currentUser);

            var viewModel = new NotificationsDropdownViewModel
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
