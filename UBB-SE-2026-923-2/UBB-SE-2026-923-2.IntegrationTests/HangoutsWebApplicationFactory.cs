using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.IntegrationTests;

public class HangoutsWebApplicationFactory : WebApplicationFactory<UBB_SE_2026_923_2.Web.Program>
{
    private const string WebApiBaseUrl = "https://localhost:7100/";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        void AddTestConfiguration(WebHostBuilderContext context, IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WebApiBaseUrl"] = WebApiBaseUrl,
            });
        }

        void RegisterTestServices(IServiceCollection services)
        {
            RemoveService<IHangoutService>(services);
            RemoveService<IDoctorAppointmentService>(services);

            services.AddSingleton<IHangoutService, FakeHangoutService>();
            services.AddSingleton<IDoctorAppointmentService, FakeDoctorAppointmentService>();

            services.AddAuthentication(defaultScheme: MvcTestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, MvcTestAuthHandler>(MvcTestAuthHandler.SchemeName, options => { });
        }

        builder.ConfigureAppConfiguration(AddTestConfiguration);
        builder.ConfigureServices(RegisterTestServices);
        builder.UseEnvironment("Development");
    }

    public static string ExtractAntiForgeryToken(string htmlContent)
    {
        Match match = Regex.Match(
            htmlContent,
            @"<input[^>]+name=""__RequestVerificationToken""[^>]+value=""([^""]+)""");
        return match.Groups[1].Value;
    }

    private static void RemoveService<TService>(IServiceCollection services)
    {
        ServiceDescriptor? descriptor = services.FirstOrDefault(serviceDescriptor => serviceDescriptor.ServiceType == typeof(TService));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }
}

public class FakeHangoutService : IHangoutService
{
    private const int CreatedHangoutId = 1;

    public int CreateHangout(string title, string description, DateTime date, int maxParticipants, IStaff creator) => CreatedHangoutId;

    public void JoinHangout(int hangoutId, IStaff staff) { }

    public List<Hangout> GetAllHangouts() => new List<Hangout>();
}

public class FakeDoctorAppointmentService : IDoctorAppointmentService
{
    private const int TestDoctorId = 1;
    private const string TestDoctorName = "Dr. Test";

    public Task<IReadOnlyList<(int DoctorId, string DoctorName)>> GetAllDoctorsAsync()
    {
        IReadOnlyList<(int DoctorId, string DoctorName)> doctors = new List<(int, string)>
        {
            (TestDoctorId, TestDoctorName),
        };
        return Task.FromResult(doctors);
    }

    public Task<IReadOnlyList<Appointment>> GetUpcomingAppointmentsAsync(int doctorUserId, DateTime fromDate, int skipCount, int takeCount) =>
        throw new NotImplementedException();

    public Task<Appointment?> GetAppointmentDetailsAsync(int appointmentId) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<Appointment>> GetAppointmentsForAdminAsync(int doctorId) =>
        throw new NotImplementedException();

    public Task CreateAppointmentAsync(string patientName, int doctorId, DateTime date, TimeSpan startTime) =>
        throw new NotImplementedException();

    public Task BookAppointmentAsync(Appointment appointment) =>
        throw new NotImplementedException();

    public Task FinishAppointmentAsync(Appointment appointment) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<Appointment>> GetAppointmentsInRangeAsync(int doctorId, DateTime fromDate, DateTime toDate) =>
        throw new NotImplementedException();

    public Task CancelAppointmentAsync(Appointment appointment) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<Shift>> GetShiftsForStaffInRangeAsync(int doctorId, DateTime fromDate, DateTime toDate) =>
        throw new NotImplementedException();

    public Task<int?> GetDoctorIdByEmailAsync(string email)
    {
        throw new NotImplementedException();
    }
}
