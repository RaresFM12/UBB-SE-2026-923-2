using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UBB_SE_2026_923_2.Data;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="IShiftSwapRepository"/>.
    /// </summary>
    public class ShiftSwapRepository : IShiftSwapRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public ShiftSwapRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public int AddShiftSwapRequest(ShiftSwapRequest request)
        {
            using var db = dbContextFactory.CreateDbContext();

            var entity = new ShiftSwapRequest
            {
                ShiftId = request.ShiftId,
                RequesterId = request.RequesterId,
                ColleagueId = request.ColleagueId,
                RequestedAt = request.RequestedAt,
                Status = request.Status,
            };

            db.ShiftSwapRequests.Add(entity);
            db.SaveChanges();
            return entity.SwapId;
        }

        public IReadOnlyList<ShiftSwapRequest> GetAllShiftSwapRequests()
        {
            using var db = dbContextFactory.CreateDbContext();
            return db.ShiftSwapRequests.AsNoTracking().ToList();
        }

        public ShiftSwapRequest? GetShiftSwapRequestById(int swapId)
        {
            using var db = dbContextFactory.CreateDbContext();
            return db.ShiftSwapRequests.AsNoTracking().FirstOrDefault(s => s.SwapId == swapId);
        }

        public void UpdateShiftSwapRequestStatus(int swapId, string status)
        {
            using var db = dbContextFactory.CreateDbContext();
            var swapRequest = db.ShiftSwapRequests.FirstOrDefault(s => s.SwapId == swapId);
            if (swapRequest is null)
            {
                return;
            }

            // Status is stored as a string column via HasConversion<string>;
            // parse the textual value back into the enum so EF writes the
            // canonical form on the next save.
            if (Enum.TryParse<ShiftSwapRequestStatus>(status, true, out var parsedStatus))
            {
                swapRequest.Status = parsedStatus;
            }

            db.SaveChanges();
        }
    }
}
