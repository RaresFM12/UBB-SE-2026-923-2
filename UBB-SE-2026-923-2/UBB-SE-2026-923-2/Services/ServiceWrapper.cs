using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.Services
{
    public static class ServiceWrapper
    {
        public static UserAccountService UserAccountService { get; private set; }

        public static void Initialize()
        {
            IUsersRepository userRepository = new SQLUsersRepository();
            ISecurityService securityService = new SecurityService();
            IUserValidationService validationService = new UserValidationService();

            UserAccountService = new UserAccountService(userRepository, securityService, validationService);
        }
    }
}
