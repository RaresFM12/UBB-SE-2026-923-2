namespace UBB_SE_2026_923_2.IntegrationTests
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using UBB_SE_2026_923_2.IntegrationTests.Fakes;
    using UBB_SE_2026_923_2.Repositories;

    public sealed class WebMvcApplicationFactory : WebApplicationFactory<UBB_SE_2026_923_2.Web.Program>
    {
        public UBB_SE_2026_923_2.IntegrationTests.Fakes.FakeUsersRepository UsersRepository { get; } = new UBB_SE_2026_923_2.IntegrationTests.Fakes.FakeUsersRepository();

        public FakeStaffRepository StaffRepository { get; } = new FakeStaffRepository();

        public FakeAppointmentRepository AppointmentsRepository { get; }

        public FakeShiftRepository ShiftRepository { get; } = new FakeShiftRepository();

        public WebMvcApplicationFactory()
        {
            this.AppointmentsRepository = new FakeAppointmentRepository(this.StaffRepository);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                var settings = new Dictionary<string, string?>
                {
                    ["WebApiBaseUrl"] = "http://localhost",
                };

                config.AddInMemoryCollection(settings);
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IUsersRepository>();
                services.AddSingleton<IUsersRepository>(this.UsersRepository);

                services.RemoveAll<IStaffRepository>();
                services.AddSingleton<IStaffRepository>(this.StaffRepository);

                services.RemoveAll<IAppointmentRepository>();
                services.AddSingleton<IAppointmentRepository>(this.AppointmentsRepository);

                services.RemoveAll<IShiftRepository>();
                services.RemoveAll<IShiftManagementShiftRepository>();
                services.RemoveAll<IPharmacyShiftRepository>();
                services.AddSingleton<IShiftRepository>(this.ShiftRepository);
                services.AddSingleton<IShiftManagementShiftRepository>(this.ShiftRepository);
                services.AddSingleton<IPharmacyShiftRepository>(this.ShiftRepository);

                services.RemoveAll<IAntiforgery>();
                services.AddSingleton<IAntiforgery, FakeAntiforgery>();

                services.PostConfigure<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                });

                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName,
                        _ => { });

                services.PostConfigure<MvcOptions>(options =>
                {
                    options.Filters.Add(new IgnoreAntiforgeryTokenAttribute());
                });
            });

            builder.UseEnvironment("Development");
        }
    }
}
