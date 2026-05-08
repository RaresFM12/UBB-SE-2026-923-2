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

            int staffId = newShift.StaffId != 0 ? newShift.StaffId : newShift.AppointedStaff.StaffID;

            var entity = new Shift
            {
                StaffId = staffId,
                Location = newShift.Location,
                StartTime = newShift.StartTime,
                EndTime = newShift.EndTime,
                Status = newShift.Status,
            };

            databaseContext.Shifts.Add(entity);
            databaseContext.SaveChanges();
        }

        public void UpdateShiftStatus(int shiftId, ShiftStatus status)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var shift = databaseContext.Shifts.FirstOrDefault(shift => shift.Id == shiftId);
            if (shift is null)
            {
                return;
            }

            shift.Status = status;
            databaseContext.SaveChanges();
        }

        public void UpdateShiftStaffId(int shiftId, int newStaffId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var shift = databaseContext.Shifts.FirstOrDefault(shift => shift.Id == shiftId);
            if (shift is null)
            {
                return;
            }

            shift.StaffId = newStaffId;
            databaseContext.SaveChanges();
        }

        public void DeleteShift(int shiftId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var shift = databaseContext.Shifts.FirstOrDefault(shift => shift.Id == shiftId);
            if (shift is null)
            {
                return;
            }

            databaseContext.Shifts.Remove(shift);
            databaseContext.SaveChanges();
        }
    }
}