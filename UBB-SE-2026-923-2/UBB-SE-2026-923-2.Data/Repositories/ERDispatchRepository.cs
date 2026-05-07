using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UBB_SE_2026_923_2.Data;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="IERDispatchRepository"/>.
    /// </summary>
    public class ERDispatchRepository : IERDispatchRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public ERDispatchRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public int AddRequest(string specialization, string location, string status)
        {
            using var db = dbContextFactory.CreateDbContext();

            var request = new ERRequest
            {
                Specialization = specialization,
                Location = location,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                AssignedDoctorId = null,
                AssignedDoctorName = null,
            };

            db.ERRequests.Add(request);
            db.SaveChanges();
            return request.Id;
        }

        public IReadOnlyList<ERRequest> GetAllRequests()
        {
            using var db = dbContextFactory.CreateDbContext();
            return db.ERRequests.AsNoTracking().ToList();
        }

        public ERRequest? GetRequestById(int requestId)
        {
            using var db = dbContextFactory.CreateDbContext();
            return db.ERRequests.AsNoTracking().FirstOrDefault(r => r.Id == requestId);
        }

        public void UpdateRequestStatus(int requestId, string status, int? assignedDoctorId, string? assignedDoctorName)
        {
            using var db = dbContextFactory.CreateDbContext();
            var request = db.ERRequests.FirstOrDefault(r => r.Id == requestId);
            if (request is null)
            {
                return;
            }

            request.Status = status;
            request.AssignedDoctorId = assignedDoctorId;
            request.AssignedDoctorName = assignedDoctorName;
            db.SaveChanges();
        }
    }
}
