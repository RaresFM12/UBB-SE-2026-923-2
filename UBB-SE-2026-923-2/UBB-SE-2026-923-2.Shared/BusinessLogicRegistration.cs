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
        public static IServiceCollection AddBusinessLogic(this IServiceCollection services, Uri webApiBaseAddress)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (webApiBaseAddress is null)
            {
                throw new ArgumentNullException(nameof(webApiBaseAddress));
            }

            RegisterHttpClient(services, webApiBaseAddress);
            RegisterRepositories(services);
            RegisterServices(services);

            return services;
        }

        private static void RegisterHttpClient(IServiceCollection services, Uri baseAddress)
        {
            services.AddSingleton<HttpClient>(_ => new HttpClient
            {
                BaseAddress = baseAddress,
            });
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            // Pharmacy-side repositories.
            services.AddSingleton<IUsersRepository, HttpUsersRepository>();
            services.AddSingleton<IItemsRepository, HttpItemsRepository>();
            services.AddSingleton<IOrdersRepository, HttpOrdersRepository>();
            services.AddSingleton<ISubstancesRepository, HttpSubstancesRepository>();

            // Staff: one HTTP-backed instance forwarded to all three staff-repository interfaces.
            services.AddSingleton<HttpStaffRepository>();
            services.AddSingleton<IStaffRepository>(sp => sp.GetRequiredService<HttpStaffRepository>());
            services.AddSingleton<IShiftManagementStaffRepository>(sp => sp.GetRequiredService<HttpStaffRepository>());
            services.AddSingleton<IPharmacyStaffRepository>(sp => sp.GetRequiredService<HttpStaffRepository>());

            // Shifts: one HTTP-backed instance forwarded to all three shift-repository interfaces.
            services.AddSingleton<HttpShiftRepository>();
            services.AddSingleton<IShiftRepository>(sp => sp.GetRequiredService<HttpShiftRepository>());
            services.AddSingleton<IShiftManagementShiftRepository>(sp => sp.GetRequiredService<HttpShiftRepository>());
            services.AddSingleton<IPharmacyShiftRepository>(sp => sp.GetRequiredService<HttpShiftRepository>());

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
            services.AddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<RaresICurrentUserService, CurrentUserServiceAdapter>();

            services.AddSingleton<IDoctorAppointmentService, DoctorAppointmentService>();
            services.AddSingleton<IERDispatchService, ERDispatchService>();
            services.AddSingleton<IFatigueAuditService, FatigueAuditService>();
            services.AddSingleton<IHangoutService, HangoutService>();
            services.AddSingleton<IPharmacyScheduleService, PharmacyScheduleService>();
            services.AddSingleton<IPharmacyVacationService, PharmacyVacationService>();
            services.AddSingleton<IShiftManagementService, ShiftManagementService>();
            services.AddSingleton<IShiftSwapService, ShiftSwapService>();
            services.AddSingleton<IMedicalEvaluationService, MedicalEvaluationService>();

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
