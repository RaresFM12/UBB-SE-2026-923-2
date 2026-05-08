namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using UBB_SE_2026_923_2.Data;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// EF Core implementation of <see cref="IAppointmentRepository"/>.
    /// </summary>
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly IDbContextFactory<AppDbContext> databaseContextFactory;

        public AppointmentRepository(IDbContextFactory<AppDbContext> databaseContextFactory)
        {
            this.databaseContextFactory = databaseContextFactory ?? throw new ArgumentNullException(nameof(databaseContextFactory));
        }

        public async Task<IReadOnlyList<Appointment>> GetAllAppointmentsAsync()
        {
            await using var databaseContext = await this.databaseContextFactory.CreateDbContextAsync();
            var appointmentRows = await databaseContext.Appointments
                .AsNoTracking()
                .Select(appointment => new
                {
                    appointment.Id,
                    appointment.DoctorId,
                    appointment.PatientName,
                    appointment.Date,
                    appointment.StartTime,
                    appointment.EndTime,
                    appointment.Status,
                    appointment.Type,
                    appointment.Location,
                    appointment.Notes,
                })
                .ToListAsync();

            return appointmentRows
                .Select(appointmentRow => new Appointment
                {
                    Id = appointmentRow.Id,
                    DoctorId = appointmentRow.DoctorId,
                    DoctorName = string.Empty,
                    PatientName = appointmentRow.PatientName,
                    Date = appointmentRow.Date,
                    StartTime = appointmentRow.StartTime,
                    EndTime = appointmentRow.EndTime,
                    Status = appointmentRow.Status,
                    Type = appointmentRow.Type,
                    Location = appointmentRow.Location,
                    Notes = appointmentRow.Notes,
                })
                .ToList();
        }

        public async Task AddAppointmentAsync(int patientId, int doctorId, DateTime startTime, DateTime endTime, string status)
        {
            await using var databaseContext = await this.databaseContextFactory.CreateDbContextAsync();

            var appointment = new Appointment
            {
                DoctorId = doctorId,
                PatientName = patientId.ToString(),
                Date = startTime.Date,
                StartTime = startTime.TimeOfDay,
                EndTime = endTime.TimeOfDay,
                Status = status,
            };

            databaseContext.Appointments.Add(appointment);
            await databaseContext.SaveChangesAsync();
        }

        public async Task UpdateAppointmentStatusAsync(int appointmentId, string status)
        {
            await using var databaseContext = await this.databaseContextFactory.CreateDbContextAsync();
            var appointmentRecord = await databaseContext.Appointments.FirstOrDefaultAsync(appointment => appointment.Id == appointmentId);
            if (appointmentRecord is null)
            {
                return;
            }

            appointmentRecord.Status = status;
            await databaseContext.SaveChangesAsync();
        }
    }
}