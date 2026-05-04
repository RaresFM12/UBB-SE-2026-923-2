using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UBB_SE_2026_923_2.Data;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="IShiftRepository"/>,
    /// <see cref="IShiftManagementShiftRepository"/> and
    /// <see cref="IPharmacyShiftRepository"/>. Shifts are loaded with their
    /// <see cref="Staff"/> navigation populated; the legacy
    /// <see cref="Shift.AppointedStaff"/> shim returns the same instance.
    /// </summary>
    public class ShiftRepository : IShiftRepository, IShiftManagementShiftRepository, IPharmacyShiftRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public ShiftRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public IReadOnlyList<Shift> GetAllShifts()
        {
            using var db = dbContextFactory.CreateDbContext();
            return db.Shifts
                .AsNoTracking()
                .Include(s => s.Staff)
                .ToList();
        }

        public void AddShift(Shift newShift)
        {
            using var db = dbContextFactory.CreateDbContext();

            int staffId = newShift.StaffId != 0 ? newShift.StaffId : newShift.AppointedStaff.StaffID;

            var entity = new Shift
            {
                StaffId = staffId,
                Location = newShift.Location,
                StartTime = newShift.StartTime,
                EndTime = newShift.EndTime,
                Status = newShift.Status,
            };

            db.Shifts.Add(entity);
            db.SaveChanges();
        }

        public void UpdateShiftStatus(int shiftId, ShiftStatus status)
        {
            using var db = dbContextFactory.CreateDbContext();
            var shift = db.Shifts.FirstOrDefault(s => s.Id == shiftId);
            if (shift is null)
            {
                return;
            }

            shift.Status = status;
            db.SaveChanges();
        }

        public void UpdateShiftStaffId(int shiftId, int newStaffId)
        {
            using var db = dbContextFactory.CreateDbContext();
            var shift = db.Shifts.FirstOrDefault(s => s.Id == shiftId);
            if (shift is null)
            {
                return;
            }

            shift.StaffId = newStaffId;
            db.SaveChanges();
        }

        public void DeleteShift(int shiftId)
        {
            using var db = dbContextFactory.CreateDbContext();
            var shift = db.Shifts.FirstOrDefault(s => s.Id == shiftId);
            if (shift is null)
            {
                return;
            }

            db.Shifts.Remove(shift);
            db.SaveChanges();
        }
    }
}
