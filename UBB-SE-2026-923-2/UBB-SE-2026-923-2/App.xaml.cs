using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using UBB_SE_2026_923_2.Configuration;
using UBB_SE_2026_923_2.Data;
using UBB_SE_2026_923_2.Repositories;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.ViewModels;
using UBB_SE_2026_923_2.ViewModels.Admin;
using UBB_SE_2026_923_2.ViewModels.Doctor;
using UBB_SE_2026_923_2.ViewModels.Pharmacy;
using UBB_SE_2026_923_2.Views.Shell;

namespace UBB_SE_2026_923_2
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        private Window? window;

        public App()
        {
            InitializeComponent();
            ServiceWrapper.Initialize();
            Services = ConfigureServices().BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs eventArgs)
        {
            window = new MainWindow();
            window.Activate();
        }

        private static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            // EF Core context registration.
            // AddDbContextFactory exposes IDbContextFactory<AppDbContext> (singleton),
            // which is the only safe way for our singleton repositories to obtain a
            // short-lived DbContext per call (DbContext itself is NOT thread-safe).
            // AddDbContext is also called so any service that prefers a directly-
            // injected (scoped) AppDbContext continues to work.
            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlServer(AppSettings.ConnectionString));
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(AppSettings.ConnectionString));

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
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            // Pharmacy-side repositories already migrated off raw SQL paths.
            services.AddScoped<IUsersRepository, SQLUsersRepository>();
            services.AddScoped<IItemsRepository, SQLItemsRepository>();
            services.AddScoped<IOrdersRepository, SQLOrdersRepository>();
            services.AddScoped<ISubstancesRepository, SQLSubstancesRepository>();

            // Legacy repositories: their constructors still take a raw connection
            // string, so they MUST be created through explicit factories until
            // Phase 2 rewires them onto IDbContextFactory<AppDbContext>.
            // StaffRepository implements three interfaces — register the concrete
            // type once and forward the interfaces to the same singleton instance.
            services.AddSingleton<StaffRepository>(_ => new StaffRepository(AppSettings.ConnectionString));
            services.AddSingleton<IStaffRepository>(sp => sp.GetRequiredService<StaffRepository>());
            services.AddSingleton<IShiftManagementStaffRepository>(sp => sp.GetRequiredService<StaffRepository>());
            services.AddSingleton<IPharmacyStaffRepository>(sp => sp.GetRequiredService<StaffRepository>());

            services.AddSingleton<ShiftRepository>(_ => new ShiftRepository(AppSettings.ConnectionString));
            services.AddSingleton<IShiftRepository>(sp => sp.GetRequiredService<ShiftRepository>());
            services.AddSingleton<IShiftManagementShiftRepository>(sp => sp.GetRequiredService<ShiftRepository>());
            services.AddSingleton<IPharmacyShiftRepository>(sp => sp.GetRequiredService<ShiftRepository>());

            services.AddSingleton<IPharmacyHandoverRepository>(_ => new PharmacyHandoverRepository(AppSettings.ConnectionString));
            services.AddSingleton<IShiftSwapRepository>(_ => new ShiftSwapRepository(AppSettings.ConnectionString));
            services.AddSingleton<INotificationRepository>(_ => new NotificationRepository(AppSettings.ConnectionString));
            services.AddSingleton<IAppointmentRepository>(_ => new AppointmentRepository(AppSettings.ConnectionString));
            services.AddSingleton<IHangoutRepository, HangoutRepository>();
            services.AddSingleton<IHangoutParticipantRepository>(_ => new HangoutParticipantRepository(AppSettings.ConnectionString));
            services.AddSingleton<IEvaluationsRepository>(_ => new EvaluationsRepository(AppSettings.ConnectionString));
            services.AddSingleton<IERDispatchRepository>(_ => new ERDispatchRepository(AppSettings.ConnectionString));
            services.AddSingleton<IHighRiskMedicineRepository>(_ => new HighRiskMedicineRepository(AppSettings.ConnectionString));
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
