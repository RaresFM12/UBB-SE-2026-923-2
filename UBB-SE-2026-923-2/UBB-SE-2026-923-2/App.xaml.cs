namespace UBB_SE_2026_923_2
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using UBB_SE_2026_923_2.Configuration;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Shared;
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
                // (and any other static-style entry points) can resolve
                // repositories rather than falling back to legacy ADO.NET ones.
                Services = ConfigureServices().BuildServiceProvider();

                // Expose the same provider to the Shared business-logic layer so
                // services that still resolve dependencies from the static
                // locator (parameterless constructors) keep working.
                SharedServiceProvider.Services = Services;

                ServiceWrapper.Initialize();
            }
            catch (Exception exception)
            {
                System.IO.File.WriteAllText(
                    System.IO.Path.Combine(AppContext.BaseDirectory, "crash.log"),
                    exception.ToString());
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

            // All HTTP-backed repositories and business services live in the
            // Shared class library and are registered through a single
            // extension method so the web project can reuse them.
            services.AddBusinessLogic(new Uri(AppSettings.WebApiBaseUrl), AppSettings.WebApiAccessKey);

            services.AddSingleton<DialogPresenter>();

            RegisterViewModels(services);

            return services;
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
