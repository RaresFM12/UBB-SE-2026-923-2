using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Services
{
    public class CurrentUserServiceAdapter : RaresICurrentUserService
    {
        public User RaresCurrentUser => ServiceWrapper.UserAccountService.CurrentUser;
    }
}