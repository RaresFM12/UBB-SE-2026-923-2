using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.IntegrationTests;

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
}
