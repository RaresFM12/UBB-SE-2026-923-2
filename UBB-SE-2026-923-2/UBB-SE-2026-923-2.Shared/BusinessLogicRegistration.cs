namespace UBB_SE_2026_923_2.Shared
{
    using System;
    using System.Net.Http;
    using Microsoft.Extensions.DependencyInjection;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;

    /// <summary>
    /// Registers every service and HTTP-backed repository shared between the
    /// desktop and web front ends. Both hosts call this from their composition
    /// root so they end up with the same business-logic graph.
    /// </summary>
    public static class BusinessLogicRegistration
    {
        public static IServiceCollection AddBusinessLogic(this IServiceCollection services, Uri webApiBaseAddress, string webApiAccessKey)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (webApiBaseAddress is null)
            {
                throw new ArgumentNullException(nameof(webApiBaseAddress));
            }

            if (string.IsNullOrWhiteSpace(webApiAccessKey))
            {
                throw new ArgumentException("Web API access key is required.", nameof(webApiAccessKey));
            }

            RegisterHttpClient(services, webApiBaseAddress, webApiAccessKey);
            RegisterRepositories(services);
            RegisterServices(services);

            return services;
        }

        private static void RegisterHttpClient(IServiceCollection services, Uri baseAddress, string webApiAccessKey)
        {
            services.AddSingleton<HttpClient>(_ =>
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = baseAddress,
                };

                httpClient.DefaultRequestHeaders.Add("X-Api-Key", webApiAccessKey);
                return httpClient;
            });
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            // Pharmacy-side repositories.
            services.AddSingleton<IUsersRepository, HttpUsersRepository>();
            services.AddSingleton<IBasketRepository, HttpBasketRepository>();
            services.AddSingleton<IItemsRepository, HttpItemsRepository>();
            services.AddSingleton<IOrdersRepository, HttpOrdersRepository>();
            services.AddSingleton<ISubstancesRepository, HttpSubstancesRepository>();

            // Staff: one HTTP-backed instance forwarded to all three staff-repository interfaces.
            services.AddSingleton<HttpStaffRepository>();
            services.AddSingleton<IStaffRepository>(serviceProvider=> serviceProvider.GetRequiredService<HttpStaffRepository>());
            services.AddSingleton<IShiftManagementStaffRepository>(serviceProvider=> serviceProvider.GetRequiredService<HttpStaffRepository>());
            services.AddSingleton<IPharmacyStaffRepository>(serviceProvider=> serviceProvider.GetRequiredService<HttpStaffRepository>());

            // Shifts: one HTTP-backed instance forwarded to all three shift-repository interfaces.
            services.AddSingleton<HttpShiftRepository>();
            services.AddSingleton<IShiftRepository>(serviceProvider=> serviceProvider.GetRequiredService<HttpShiftRepository>());
            services.AddSingleton<IShiftManagementShiftRepository>(serviceProvider=> serviceProvider.GetRequiredService<HttpShiftRepository>());
            services.AddSingleton<IPharmacyShiftRepository>(serviceProvider => serviceProvider.GetRequiredService<HttpShiftRepository>());

            services.AddSingleton<IPharmacyHandoverRepository, HttpPharmacyHandoverRepository>();
            services.AddSingleton<IShiftSwapRepository, HttpShiftSwapRepository>();
            services.AddSingleton<INotificationRepository, HttpNotificationRepository>();
            services.AddSingleton<IAppointmentRepository, HttpAppointmentRepository>();
            services.AddSingleton<IHangoutRepository, HttpHangoutRepository>();
            services.AddSingleton<IHangoutParticipantRepository, HttpHangoutParticipantRepository>();
            services.AddSingleton<IEvaluationsRepository, HttpEvaluationsRepository>();
            services.AddSingleton<IERDispatchRepository, HttpERDispatchRepository>();
            services.AddSingleton<IHighRiskMedicineRepository, HttpHighRiskMedicineRepository>();
        }

        private static void RegisterServices(IServiceCollection services)
        {
            // Cross-cutting / current-user.
            services.AddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<RaresICurrentUserService, CurrentUserServiceAdapter>();

            // Account / auth collaborators. Scoped so the Web project gets a
            // fresh instance per request; the desktop builds its own instance
            // via ServiceWrapper.Initialize and is unaffected.
            services.AddSingleton<ISecurityService, SecurityService>();
            services.AddSingleton<IUserValidationService, UserValidationService>();
            services.AddScoped<IUserAccountService, UserAccountService>();

            // Feature services.
            services.AddSingleton<IAdminService, AdminService>();
            services.AddSingleton<IDoctorAppointmentService, DoctorAppointmentService>();
            services.AddSingleton<IERDispatchService, ERDispatchService>();
            services.AddSingleton<IFatigueAuditService, FatigueAuditService>();
            services.AddSingleton<IHangoutService, HangoutService>();
            services.AddSingleton<IMedicalEvaluationService, MedicalEvaluationService>();
            services.AddSingleton<IPharmacyScheduleService, PharmacyScheduleService>();
            services.AddSingleton<IPharmacyVacationService, PharmacyVacationService>();
            services.AddSingleton<IShiftManagementService, ShiftManagementService>();
            services.AddSingleton<IShiftSwapService, ShiftSwapService>();
            services.AddSingleton<IProductCatalogueService, ProductCatalogueService>();
            services.AddSingleton<IWellnessItemsService, WellnessItemsService>();

            // OrderService has a parameterless constructor that falls back to
            // SharedServiceProvider for legacy desktop call sites; PrescriptionService
            // is constructed inside OrderService and also registered for direct use.
            services.AddSingleton<IOrderService, OrderService>();
            services.AddSingleton<IBasketService, BasketService>();
            services.AddSingleton<IPrescriptionService>(serviceProvider=>
                new PrescriptionService(
                    serviceProvider.GetRequiredService<IItemsRepository>(),
                    serviceProvider.GetRequiredService<IEvaluationsRepository>()));

            services.AddSingleton<IPeriodTrackerService, PeriodTrackerService>();
            services.AddSingleton<IPeriodTrackerServiceFactory, PeriodTrackerServiceFactory>();

            static ISalaryComputationService CreateSalaryComputationService(IServiceProvider serviceProvider) =>
                new SalaryComputationService(
                    serviceProvider.GetRequiredService<IPharmacyHandoverRepository>(),
                    serviceProvider.GetRequiredService<IHangoutRepository>(),
                    serviceProvider.GetRequiredService<IHangoutParticipantRepository>(),
                    serviceProvider.GetRequiredService<IStaffRepository>(),
                    serviceProvider.GetRequiredService<IShiftManagementShiftRepository>());
            services.AddSingleton<ISalaryComputationService>(CreateSalaryComputationService);
        }
    }
}
