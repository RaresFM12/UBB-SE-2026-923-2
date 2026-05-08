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
    /// EF Core implementation of <see cref="IStaffRepository"/>,
    /// <see cref="IShiftManagementStaffRepository"/> and
    /// <see cref="IPharmacyStaffRepository"/>. Reads pull from the TPH-mapped
    /// <c>Staff</c> table — EF Core materializes <see cref="Doctor"/> or
    /// <see cref="Pharmacyst"/> instances based on the <c>Role</c> discriminator.
    /// </summary>
    public class StaffRepository : IShiftManagementStaffRepository, IStaffRepository, IPharmacyStaffRepository
    {
        private readonly IDbContextFactory<AppDbContext> databaseContextFactory;

        public StaffRepository(IDbContextFactory<AppDbContext> databaseContextFactory)
        {
            this.databaseContextFactory = databaseContextFactory ?? throw new ArgumentNullException(nameof(databaseContextFactory));
        }

        public List<IStaff> LoadAllStaff()
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();

            // TPH: query the base set, return only the concrete subtypes the
            // legacy code surfaced (Doctor / Pharmacyst). EF picks the right
            // .NET type based on the Role discriminator.
            return databaseContext.StaffMembers
                .AsNoTracking()
                .Where(staffMember => staffMember is Doctor || staffMember is Pharmacyst)
                .ToList()
                .Cast<IStaff>()
                .ToList();
        }

        public IStaff? GetStaffById(int staffId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.StaffMembers
                .AsNoTracking()
                .Where(staffMember => staffMember.StaffID == staffId && (staffMember is Doctor || staffMember is Pharmacyst))
                .FirstOrDefault() as IStaff;
        }

        public List<Pharmacyst> GetPharmacists()
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.Pharmacysts.AsNoTracking().ToList();
        }

        public async Task<IReadOnlyList<(int DoctorId, string FirstName, string LastName)>> GetAllDoctorsAsync()
        {
            await using var databaseContext = await this.databaseContextFactory.CreateDbContextAsync();
            var doctorRows = await databaseContext.Doctors
                .AsNoTracking()
                .Select(doctor => new { doctor.StaffID, doctor.FirstName, doctor.LastName })
                .ToListAsync();

            return doctorRows
                .Select(doctorRow => (doctorRow.StaffID, doctorRow.FirstName, doctorRow.LastName))
                .ToList();
        }

        public async Task UpdateStatusAsync(int staffId, string status)
        {
            await using var databaseContext = await this.databaseContextFactory.CreateDbContextAsync();
            var staffMember = await databaseContext.StaffMembers.FirstOrDefaultAsync(staff => staff.StaffID == staffId);
            if (staffMember is null)
            {
                return;
            }

            staffMember.Status = status;
            await databaseContext.SaveChangesAsync();
        }

        public void UpdateStaffAvailability(int staffId, bool isAvailable, DoctorStatus status = DoctorStatus.OFF_DUTY)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var staffMember = databaseContext.StaffMembers.FirstOrDefault(staff => staff.StaffID == staffId);
            if (staffMember is null)
            {
                return;
            }

            staffMember.Available = isAvailable;
            staffMember.Status = status.ToString();
            if (staffMember is Doctor doctor)
            {
                doctor.DoctorStatus = status;
            }

            databaseContext.SaveChanges();
        }

        public void UpdateStaff(IStaff staff)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var existingStaffMember = databaseContext.StaffMembers.FirstOrDefault(trackedStaff => trackedStaff.StaffID == staff.StaffID);
            if (existingStaffMember is null)
            {
                return;
            }

            existingStaffMember.FirstName = staff.FirstName;
            existingStaffMember.LastName = staff.LastName;
            existingStaffMember.ContactInfo = staff.ContactInfo;
            existingStaffMember.Available = staff.Available;

            if (existingStaffMember is Doctor existingDoctor && staff is Doctor incomingDoctor)
            {
                existingDoctor.LicenseNumber = incomingDoctor.LicenseNumber;
                existingDoctor.Specialization = incomingDoctor.Specialization;
                existingDoctor.DoctorStatus = incomingDoctor.DoctorStatus;
                existingDoctor.Status = incomingDoctor.DoctorStatus.ToString();
            }
            else if (existingStaffMember is Pharmacyst existingPharmacist && staff is Pharmacyst incomingPharmacist)
            {
                existingPharmacist.Certification = incomingPharmacist.Certification;
            }

            databaseContext.SaveChanges();
        }
    }
}