namespace UBB_SE_2026_923_2.IntegrationTests.Fakes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;

    public sealed class FakeAppointmentRepository : IAppointmentRepository
    {
        private readonly List<Appointment> appointments = new();
        private readonly IStaffRepository staffRepository;
        private int nextId = 1;

        public FakeAppointmentRepository(IStaffRepository staffRepository)
        {
            this.staffRepository = staffRepository;
        }

        public IReadOnlyList<Appointment> Appointments => this.appointments;

        public void Seed(params Appointment[] entries)
        {
            if (entries.Length == 0)
            {
                return;
            }

            foreach (var entry in entries)
            {
                if (entry.Id == 0)
                {
                    entry.Id = this.nextId++;
                }
                else
                {
                    this.nextId = Math.Max(this.nextId, entry.Id + 1);
                }

                this.appointments.Add(entry);
            }
        }

        public Task<IReadOnlyList<Appointment>> GetAllAppointmentsAsync()
        {
            return Task.FromResult<IReadOnlyList<Appointment>>(this.appointments.ToList());
        }

        public Task AddAppointmentAsync(int patientId, int doctorId, DateTime startTime, DateTime endTime, string status)
        {
            var doctor = this.staffRepository.GetStaffById(doctorId) as Doctor ?? new Doctor { StaffID = doctorId };
            var appointment = new Appointment
            {
                Id = this.nextId++,
                PatientName = patientId.ToString(),
                Doctor = doctor,
                Date = startTime.Date,
                StartTime = startTime.TimeOfDay,
                EndTime = endTime.TimeOfDay,
                Status = status,
            };

            this.appointments.Add(appointment);
            return Task.CompletedTask;
        }

        public Task UpdateAppointmentStatusAsync(int appointmentId, string status)
        {
            var appointment = this.appointments.FirstOrDefault(item => item.Id == appointmentId);
            if (appointment != null)
            {
                appointment.Status = status;
            }

            return Task.CompletedTask;
        }
    }
}
