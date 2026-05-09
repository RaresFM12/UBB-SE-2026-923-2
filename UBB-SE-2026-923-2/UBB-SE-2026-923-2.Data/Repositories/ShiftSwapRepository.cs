namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using UBB_SE_2026_923_2.Data;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// EF Core implementation of <see cref="IShiftSwapRepository"/>.
    /// </summary>
    public class ShiftSwapRepository : IShiftSwapRepository
    {
        private readonly IDbContextFactory<AppDbContext> databaseContextFactory;

        public ShiftSwapRepository(IDbContextFactory<AppDbContext> databaseContextFactory)
        {
            this.databaseContextFactory = databaseContextFactory ?? throw new ArgumentNullException(nameof(databaseContextFactory));
        }

        public int AddShiftSwapRequest(ShiftSwapRequest shiftSwapRequest)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();

            var shiftForSwap = databaseContext.Shifts.Find(shiftSwapRequest.Shift.Id)
                ?? throw new ArgumentException($"Shift with identifier {shiftSwapRequest.Shift.Id} not found.", nameof(shiftSwapRequest));

            var requesterStaffMember = databaseContext.StaffMembers.Find(shiftSwapRequest.Requester.StaffID)
                ?? throw new ArgumentException($"Staff member with identifier {shiftSwapRequest.Requester.StaffID} not found.", nameof(shiftSwapRequest));

            var colleagueStaffMember = databaseContext.StaffMembers.Find(shiftSwapRequest.Colleague.StaffID)
                ?? throw new ArgumentException($"Staff member with identifier {shiftSwapRequest.Colleague.StaffID} not found.", nameof(shiftSwapRequest));

            var newShiftSwapRequestEntity = new ShiftSwapRequest
            {
                Shift = shiftForSwap,
                Requester = requesterStaffMember,
                Colleague = colleagueStaffMember,
                RequestedAt = shiftSwapRequest.RequestedAt,
                Status = shiftSwapRequest.Status,
            };

            databaseContext.ShiftSwapRequests.Add(newShiftSwapRequestEntity);
            databaseContext.SaveChanges();
            return newShiftSwapRequestEntity.SwapId;
        }

        public IReadOnlyList<ShiftSwapRequest> GetAllShiftSwapRequests()
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.ShiftSwapRequests
                .AsNoTracking()
                .Include(shiftSwapRequest => shiftSwapRequest.Shift)
                .Include(shiftSwapRequest => shiftSwapRequest.Requester)
                .Include(shiftSwapRequest => shiftSwapRequest.Colleague)
                .ToList();
        }

        public ShiftSwapRequest? GetShiftSwapRequestById(int swapId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.ShiftSwapRequests
                .AsNoTracking()
                .Include(shiftSwapRequest => shiftSwapRequest.Shift)
                .Include(shiftSwapRequest => shiftSwapRequest.Requester)
                .Include(shiftSwapRequest => shiftSwapRequest.Colleague)
                .FirstOrDefault(shiftSwapRequest => shiftSwapRequest.SwapId == swapId);
        }

        public void UpdateShiftSwapRequestStatus(int swapId, string status)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var shiftSwapRequestToUpdate = databaseContext.ShiftSwapRequests
                .FirstOrDefault(shiftSwapRequest => shiftSwapRequest.SwapId == swapId);

            if (shiftSwapRequestToUpdate is null)
            {
                return;
            }

            // Status is stored as a string column via HasConversion<string>;
            // parse the textual value back into the enum so EF writes the
            // canonical form on the next save.
            if (Enum.TryParse<ShiftSwapRequestStatus>(status, true, out var parsedStatus))
            {
                shiftSwapRequestToUpdate.Status = parsedStatus;
            }

            databaseContext.SaveChanges();
        }
    }
}
