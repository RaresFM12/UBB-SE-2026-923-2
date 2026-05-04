using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UBB_SE_2026_923_2.Data;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="IAppointmentRepository"/>.
    /// </summary>
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public AppointmentRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public async Task<IReadOnlyList<Appointment>> GetAllAppointmentsAsync()
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            var rows = await db.Appointments
                .AsNoTracking()
                .Select(a => new
                {
                    a.Id,
                    a.DoctorId,
                    a.PatientName,
                    a.Date,
                    a.StartTime,
                    a.EndTime,
                    a.Status,
                    a.Type,
                    a.Location,
                    a.Notes,
                })
                .ToListAsync();

            return rows
                .Select(row => new Appointment
                {
                    Id = row.Id,
                    DoctorId = row.DoctorId,
                    DoctorName = string.Empty,
                    PatientName = row.PatientName,
                    Date = row.Date,
                    StartTime = row.StartTime,
                    EndTime = row.EndTime,
                    Status = row.Status,
                    Type = row.Type,
                    Location = row.Location,
                    Notes = row.Notes,
                })
                .ToList();
        }

        public async Task AddAppointmentAsync(int patientId, int doctorId, DateTime startTime, DateTime endTime, string status)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();

            var appointment = new Appointment
            {
                DoctorId = doctorId,
                PatientName = patientId.ToString(),
                Date = startTime.Date,
                StartTime = startTime.TimeOfDay,
                EndTime = endTime.TimeOfDay,
                Status = status,
            };

            db.Appointments.Add(appointment);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAppointmentStatusAsync(int id, string status)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            var appointment = await db.Appointments.FirstOrDefaultAsync(a => a.Id == id);
            if (appointment is null)
            {
                return;
            }

            appointment.Status = status;
            await db.SaveChangesAsync();
        }
    }
}
