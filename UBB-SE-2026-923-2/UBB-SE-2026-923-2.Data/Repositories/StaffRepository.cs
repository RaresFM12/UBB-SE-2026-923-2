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
    /// EF Core implementation of <see cref="IStaffRepository"/>,
    /// <see cref="IShiftManagementStaffRepository"/> and
    /// <see cref="IPharmacyStaffRepository"/>. Reads pull from the TPH-mapped
    /// <c>Staff</c> table — EF Core materializes <see cref="Doctor"/> or
    /// <see cref="Pharmacyst"/> instances based on the <c>Role</c> discriminator.
    /// </summary>
    public class StaffRepository : IShiftManagementStaffRepository, IStaffRepository, IPharmacyStaffRepository
    {
        private const string DoctorRoleLabel = "Doctor";

        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public StaffRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public List<IStaff> LoadAllStaff()
        {
            using var db = dbContextFactory.CreateDbContext();
            // TPH: query the base set, return only the concrete subtypes the
            // legacy code surfaced (Doctor / Pharmacyst). EF picks the right
            // .NET type based on the Role discriminator.
            return db.StaffMembers
                .AsNoTracking()
                .Where(s => s is Doctor || s is Pharmacyst)
                .ToList()
                .Cast<IStaff>()
                .ToList();
        }

        public IStaff? GetStaffById(int staffId)
        {
            using var db = dbContextFactory.CreateDbContext();
            return db.StaffMembers
                .AsNoTracking()
                .Where(s => s.StaffID == staffId && (s is Doctor || s is Pharmacyst))
                .FirstOrDefault() as IStaff;
        }

        public List<Pharmacyst> GetPharmacists()
        {
            using var db = dbContextFactory.CreateDbContext();
            return db.Pharmacysts.AsNoTracking().ToList();
        }

        public async Task<IReadOnlyList<(int DoctorId, string FirstName, string LastName)>> GetAllDoctorsAsync()
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            var rows = await db.Doctors
                .AsNoTracking()
                .Select(d => new { d.StaffID, d.FirstName, d.LastName })
                .ToListAsync();

            return rows
                .Select(row => (row.StaffID, row.FirstName, row.LastName))
                .ToList();
        }

        public async Task UpdateStatusAsync(int staffId, string status)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            var staff = await db.StaffMembers.FirstOrDefaultAsync(s => s.StaffID == staffId);
            if (staff is null)
            {
                return;
            }

            staff.Status = status;
            await db.SaveChangesAsync();
        }

        public void UpdateStaffAvailability(int staffId, bool isAvailable, DoctorStatus status = DoctorStatus.OFF_DUTY)
        {
            using var db = dbContextFactory.CreateDbContext();
            var staff = db.StaffMembers.FirstOrDefault(s => s.StaffID == staffId);
            if (staff is null)
            {
                return;
            }

            staff.Available = isAvailable;
            staff.Status = status.ToString();
            if (staff is Doctor doctor)
            {
                doctor.DoctorStatus = status;
            }

            db.SaveChanges();
        }

        public void UpdateStaff(IStaff staff)
        {
            using var db = dbContextFactory.CreateDbContext();
            var existing = db.StaffMembers.FirstOrDefault(s => s.StaffID == staff.StaffID);
            if (existing is null)
            {
                return;
            }

            existing.FirstName = staff.FirstName;
            existing.LastName = staff.LastName;
            existing.ContactInfo = staff.ContactInfo;
            existing.Available = staff.Available;

            if (existing is Doctor existingDoctor && staff is Doctor incomingDoctor)
            {
                existingDoctor.LicenseNumber = incomingDoctor.LicenseNumber;
                existingDoctor.Specialization = incomingDoctor.Specialization;
                existingDoctor.DoctorStatus = incomingDoctor.DoctorStatus;
                existingDoctor.Status = incomingDoctor.DoctorStatus.ToString();
            }
            else if (existing is Pharmacyst existingPharmacist && staff is Pharmacyst incomingPharmacist)
            {
                existingPharmacist.Certification = incomingPharmacist.Certification;
            }

            db.SaveChanges();
        }
    }
}
