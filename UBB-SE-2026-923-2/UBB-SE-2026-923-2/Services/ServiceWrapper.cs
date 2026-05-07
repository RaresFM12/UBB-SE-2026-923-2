namespace UBB_SE_2026_923_2.Services
{
    using Microsoft.Extensions.DependencyInjection;
    using UBB_SE_2026_923_2.Repositories;

    public static class ServiceWrapper
    {
        public static UserAccountService UserAccountService { get; private set; } = null!;

        public static void Initialize()
        {
            // Resolve the EF Core users repository from the DI container that
            // App built before invoking us. The other two collaborators have
            // no I/O and are cheap to instantiate directly.
            IUsersRepository userRepository = App.Services.GetRequiredService<IUsersRepository>();
            ISecurityService securityService = new SecurityService();
            IUserValidationService validationService = new UserValidationService();

            UserAccountService = new UserAccountService(userRepository, securityService, validationService);
        }
    }
}
