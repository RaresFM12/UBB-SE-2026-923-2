namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBB_SE_2026_923_2.Models;

    public interface IAppointmentRepository
    {
        Task<IReadOnlyList<Appointment>> GetAllAppointmentsAsync();

        Task AddAppointmentAsync(int patientId, int doctorId, DateTime startTime, DateTime endTime, string status);

        Task UpdateAppointmentStatusAsync(int appointmentId, string status);
    }
}