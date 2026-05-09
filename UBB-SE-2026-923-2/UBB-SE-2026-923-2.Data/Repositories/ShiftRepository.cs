namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using UBB_SE_2026_923_2.Data;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// EF Core implementation of <see cref="IShiftRepository"/>,
    /// <see cref="IShiftManagementShiftRepository"/> and
    /// <see cref="IPharmacyShiftRepository"/>. Shifts are loaded with their
    /// <see cref="Staff"/> navigation populated; the legacy
    /// <see cref="Shift.AppointedStaff"/> shim returns the same instance.
    /// </summary>
    public class ShiftRepository : IShiftRepository, IShiftManagementShiftRepository, IPharmacyShiftRepository
    {
        private readonly IDbContextFactory<AppDbContext> databaseContextFactory;

        public ShiftRepository(IDbContextFactory<AppDbContext> databaseContextFactory)
        {
            this.databaseContextFactory = databaseContextFactory ?? throw new ArgumentNullException(nameof(databaseContextFactory));
        }

        public IReadOnlyList<Shift> GetAllShifts()
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.Shifts
                .AsNoTracking()
                .Include(shift => shift.Staff)
                .ToList();
        }

        public void AddShift(Shift newShift)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();

            int staffIdentifier = newShift.Staff?.StaffID ?? newShift.AppointedStaff.StaffID;
            var staffMember = databaseContext.StaffMembers.Find(staffIdentifier);
            if (staffMember == null)
            {
                staffMember = new Staff { StaffID = staffIdentifier, Role = "Staff" };
                databaseContext.Attach(staffMember);
            }

            var newShiftEntity = new Shift
            {
                Staff = staffMember,
                Location = newShift.Location,
                StartTime = newShift.StartTime,
                EndTime = newShift.EndTime,
                Status = newShift.Status,
            };

            databaseContext.Shifts.Add(newShiftEntity);
            databaseContext.SaveChanges();
        }

        public void UpdateShiftStatus(int shiftId, ShiftStatus status)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var shiftToUpdate = databaseContext.Shifts.FirstOrDefault(shift => shift.Id == shiftId);
            if (shiftToUpdate is null)
            {
                return;
            }

            shiftToUpdate.Status = status;
            databaseContext.SaveChanges();
        }

        public void UpdateShiftStaffId(int shiftId, int newStaffId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var shiftToUpdate = databaseContext.Shifts.FirstOrDefault(shift => shift.Id == shiftId);
            if (shiftToUpdate is null)
            {
                return;
            }

            var newStaffMember = databaseContext.StaffMembers.Find(newStaffId);
            if (newStaffMember is null)
            {
                return;
            }

            shiftToUpdate.Staff = newStaffMember;
            databaseContext.SaveChanges();
        }

        public void DeleteShift(int shiftId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var shiftToDelete = databaseContext.Shifts.FirstOrDefault(shift => shift.Id == shiftId);
            if (shiftToDelete is null)
            {
                return;
            }

            databaseContext.Shifts.Remove(shiftToDelete);
            databaseContext.SaveChanges();
        }
    }
}
