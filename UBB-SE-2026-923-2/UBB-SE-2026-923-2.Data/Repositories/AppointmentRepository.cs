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
            return await databaseContext.Appointments
                .AsNoTracking()
                .Include(appointment => appointment.Doctor)
                .ToListAsync();
        }

        public async Task AddAppointmentAsync(int patientId, int doctorId, DateTime startTime, DateTime endTime, string status)
        {
            await using var databaseContext = await this.databaseContextFactory.CreateDbContextAsync();

            var assignedDoctor = doctorId == 0 ? null : await databaseContext.Doctors.FindAsync(doctorId);

            var newAppointment = new Appointment
            {
                Doctor = assignedDoctor,
                PatientName = patientId.ToString(),
                Date = startTime.Date,
                StartTime = startTime.TimeOfDay,
                EndTime = endTime.TimeOfDay,
                Status = status,
            };

            databaseContext.Appointments.Add(newAppointment);
            await databaseContext.SaveChangesAsync();
        }

        public async Task UpdateAppointmentStatusAsync(int appointmentId, string status)
        {
            await using var databaseContext = await this.databaseContextFactory.CreateDbContextAsync();
            var appointmentRecord = await databaseContext.Appointments
                .FirstOrDefaultAsync(appointment => appointment.Id == appointmentId);

            if (appointmentRecord is null)
            {
                return;
            }

            appointmentRecord.Status = status;
            await databaseContext.SaveChangesAsync();
        }
    }
}
