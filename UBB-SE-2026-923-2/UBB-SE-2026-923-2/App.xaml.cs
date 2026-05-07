namespace UBB_SE_2026_923_2
{
    using System;
    using System.Net.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using UBB_SE_2026_923_2.Configuration;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels;
    using UBB_SE_2026_923_2.ViewModels.Admin;
    using UBB_SE_2026_923_2.ViewModels.Doctor;
    using UBB_SE_2026_923_2.ViewModels.Pharmacy;
    using UBB_SE_2026_923_2.Views.Shell;

    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        private Window? window;

        public App()
        {
            try
            {
                InitializeComponent();

                // Build the DI container first so that ServiceWrapper.Initialize
                // (and any other static-style entry points) can resolve EF Core
                // repositories rather than falling back to the legacy ADO.NET ones.
                Services = ConfigureServices().BuildServiceProvider();
                ServiceWrapper.Initialize();
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText(
                    System.IO.Path.Combine(AppContext.BaseDirectory, "crash.log"),
                    ex.ToString());
                throw;
            }
        }

        protected override void OnLaunched(LaunchActivatedEventArgs eventArgs)
        {
            this.window = new MainWindow();
            this.window.Activate();
        }

        private static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            // The desktop project no longer talks to the database directly:
            // every repository is HTTP-backed and goes through the Web API.
            // EF Core lives in UBB-SE-2026-923-2.Data and is hosted by
            // UBB-SE-2026-923-2.WebApi.
            RegisterInfrastructure(services);
            RegisterRepositories(services);
            RegisterServices(services);
            RegisterViewModels(services);

            return services;
        }

        private static void RegisterInfrastructure(IServiceCollection services)
        {
            services.AddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<RaresICurrentUserService, CurrentUserServiceAdapter>();
            services.AddSingleton<DialogPresenter>();

            // Single HttpClient pointing at the local Web API. Replace BaseAddress
            // when deploying the API somewhere other than localhost.
            services.AddSingleton<HttpClient>(_ => new HttpClient
            {
                BaseAddress = new Uri(AppSettings.WebApiBaseUrl),
            });
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            // Every repository now takes IDbContextFactory<AppDbContext>, which
            // is resolved automatically by the DI container — no more explicit
            // connection-string factory delegates.

            // Pharmacy-side repositories.
            services.AddSingleton<IUsersRepository, HttpUsersRepository>();
            services.AddSingleton<IItemsRepository, HttpItemsRepository>();
            services.AddSingleton<IOrdersRepository, HttpOrdersRepository>();
            services.AddSingleton<ISubstancesRepository, HttpSubstancesRepository>();

            // Staff goes through the Web API. One HTTP-backed instance is
            // forwarded to all three staff-repository interfaces.
            services.AddSingleton<HttpStaffRepository>();
            services.AddSingleton<IStaffRepository>(sp => sp.GetRequiredService<HttpStaffRepository>());
            services.AddSingleton<IShiftManagementStaffRepository>(sp => sp.GetRequiredService<HttpStaffRepository>());
            services.AddSingleton<IPharmacyStaffRepository>(sp => sp.GetRequiredService<HttpStaffRepository>());

            // Shifts now go through the Web API. One HTTP-backed instance is
            // forwarded to all three shift-repository interfaces.
            services.AddSingleton<HttpShiftRepository>();
            services.AddSingleton<IShiftRepository>(sp => sp.GetRequiredService<HttpShiftRepository>());
            services.AddSingleton<IShiftManagementShiftRepository>(sp => sp.GetRequiredService<HttpShiftRepository>());
            services.AddSingleton<IPharmacyShiftRepository>(sp => sp.GetRequiredService<HttpShiftRepository>());

            // Hospital-side single-interface repositories.
            services.AddSingleton<IPharmacyHandoverRepository, HttpPharmacyHandoverRepository>();
            services.AddSingleton<IShiftSwapRepository, HttpShiftSwapRepository>();
            services.AddSingleton<INotificationRepository, HttpNotificationRepository>();

            // Appointments now go through the Web API instead of EF Core directly.
            services.AddSingleton<IAppointmentRepository, HttpAppointmentRepository>();
            services.AddSingleton<IHangoutRepository, HttpHangoutRepository>();
            services.AddSingleton<IHangoutParticipantRepository, HttpHangoutParticipantRepository>();
            services.AddSingleton<IEvaluationsRepository, HttpEvaluationsRepository>();
            services.AddSingleton<IERDispatchRepository, HttpERDispatchRepository>();
            services.AddSingleton<IHighRiskMedicineRepository, HttpHighRiskMedicineRepository>();
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IDoctorAppointmentService, DoctorAppointmentService>();
            services.AddSingleton<IERDispatchService, ERDispatchService>();
            services.AddSingleton<IFatigueAuditService, FatigueAuditService>();
            services.AddSingleton<IHangoutService, HangoutService>();
            services.AddSingleton<IPharmacyScheduleService, PharmacyScheduleService>();
            services.AddSingleton<IPharmacyVacationService, PharmacyVacationService>();
            services.AddSingleton<IShiftManagementService, ShiftManagementService>();
            services.AddSingleton<IShiftSwapService, ShiftSwapService>();
            static ISalaryComputationService CreateSalaryComputationService(IServiceProvider serviceProvider) =>
                new SalaryComputationService(
                    serviceProvider.GetRequiredService<IPharmacyHandoverRepository>(),
                    serviceProvider.GetRequiredService<IHangoutRepository>(),
                    serviceProvider.GetRequiredService<IHangoutParticipantRepository>(),
                    serviceProvider.GetRequiredService<IStaffRepository>(),
                    serviceProvider.GetRequiredService<IShiftManagementShiftRepository>());
            services.AddSingleton<ISalaryComputationService>(CreateSalaryComputationService);
            services.AddSingleton<IMedicalEvaluationService, MedicalEvaluationService>();
        }

        private static void RegisterViewModels(IServiceCollection services)
        {
            services.AddTransient<AdminShiftViewModel>();
            services.AddTransient<AdminAppointmentsViewModel>();
            services.AddTransient<ERDispatchViewModel>();
            services.AddTransient<FatigueShiftAuditViewModel>();
            services.AddTransient<DoctorScheduleViewModel>();
            services.AddTransient<MyScheduleViewModel>();
            services.AddTransient<PharmacyScheduleViewModel>();
            services.AddTransient<PharmacistVacationViewModel>();
            services.AddTransient<MedicalEvaluationViewModel>();

            static IncomingSwapRequestsViewModel CreateIncomingSwapRequestsViewModel(IServiceProvider serviceProvider) =>
                new IncomingSwapRequestsViewModel(
                    serviceProvider.GetRequiredService<IShiftSwapService>());
            services.AddTransient<IncomingSwapRequestsViewModel>(CreateIncomingSwapRequestsViewModel);

            static HangoutViewModel CreateHangoutViewModel(IServiceProvider serviceProvider) =>
                new HangoutViewModel(
                    serviceProvider.GetRequiredService<IHangoutService>(),
                    serviceProvider.GetRequiredService<IDoctorAppointmentService>());
            services.AddTransient<HangoutViewModel>(CreateHangoutViewModel);

            static SalaryComputationViewModel CreateSalaryComputationViewModel(IServiceProvider serviceProvider) =>
                new SalaryComputationViewModel(
                    serviceProvider.GetRequiredService<ISalaryComputationService>());
            services.AddTransient<SalaryComputationViewModel>(CreateSalaryComputationViewModel);
        }
    }
}
