namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using UBB_SE_2026_923_2.Data;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// EF Core implementation of <see cref="IERDispatchRepository"/>.
    /// </summary>
    public class ERDispatchRepository : IERDispatchRepository
    {
        private readonly IDbContextFactory<AppDbContext> databaseContextFactory;

        public ERDispatchRepository(IDbContextFactory<AppDbContext> databaseContextFactory)
        {
            this.databaseContextFactory = databaseContextFactory ?? throw new ArgumentNullException(nameof(databaseContextFactory));
        }

        public int AddRequest(string specialization, string location, string status)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();

            var newERRequest = new ERRequest
            {
                Specialization = specialization,
                Location = location,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                AssignedDoctor = null,
            };

            databaseContext.ERRequests.Add(newERRequest);
            databaseContext.SaveChanges();
            return newERRequest.Id;
        }

        public IReadOnlyList<ERRequest> GetAllRequests()
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.ERRequests
                .AsNoTracking()
                .Include(emergencyRoomRequest => emergencyRoomRequest.AssignedDoctor)
                .ToList();
        }

        public ERRequest? GetRequestById(int requestId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.ERRequests
                .AsNoTracking()
                .Include(emergencyRoomRequest => emergencyRoomRequest.AssignedDoctor)
                .FirstOrDefault(emergencyRoomRequest => emergencyRoomRequest.Id == requestId);
        }

        public void UpdateRequestStatus(int requestId, string status, int? assignedDoctorId, string? assignedDoctorName)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var retrievedERRequest = databaseContext.ERRequests
                .FirstOrDefault(emergencyRoomRequest => emergencyRoomRequest.Id == requestId);

            if (retrievedERRequest is null)
            {
                return;
            }

            retrievedERRequest.Status = status;

            if (assignedDoctorId.HasValue)
            {
                retrievedERRequest.AssignedDoctor = databaseContext.Doctors.Find(assignedDoctorId.Value);
            }
            else
            {
                retrievedERRequest.AssignedDoctor = null;
            }

            // assignedDoctorName is intentionally ignored: the doctor's name is
            // available through AssignedDoctor.FullName and must not be stored as
            // a redundant denormalised field.
            databaseContext.SaveChanges();
        }
    }
}
