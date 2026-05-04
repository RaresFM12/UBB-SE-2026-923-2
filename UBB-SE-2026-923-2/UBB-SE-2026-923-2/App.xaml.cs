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
            services.AddScoped<IUsersRepository, SQLUsersRepository>();
            services.AddScoped<IItemsRepository, SQLItemsRepository>();
            services.AddScoped<IOrdersRepository, SQLOrdersRepository>();
            services.AddScoped<ISubstancesRepository, SQLSubstancesRepository>();

            // Legacy repositories will be replaced in the next step.
            services.AddSingleton<IShiftRepository, ShiftRepository>();
            services.AddSingleton<IShiftManagementShiftRepository, ShiftRepository>();
            services.AddSingleton<IPharmacyShiftRepository, ShiftRepository>();
            services.AddSingleton<IShiftManagementStaffRepository, StaffRepository>();
            services.AddSingleton<IStaffRepository, StaffRepository>();
            services.AddSingleton<IPharmacyStaffRepository, StaffRepository>();
            services.AddSingleton<IPharmacyHandoverRepository, PharmacyHandoverRepository>();
            services.AddSingleton<IShiftSwapRepository, ShiftSwapRepository>();
            services.AddSingleton<INotificationRepository, NotificationRepository>();
            services.AddSingleton<IAppointmentRepository, AppointmentRepository>();
            services.AddSingleton<IHangoutRepository, HangoutRepository>();
            services.AddSingleton<IHangoutParticipantRepository, HangoutParticipantRepository>();
            services.AddSingleton<IEvaluationsRepository, EvaluationsRepository>();
            services.AddSingleton<IERDispatchRepository, ERDispatchRepository>();
            services.AddSingleton<IHighRiskMedicineRepository, HighRiskMedicineRepository>();
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
