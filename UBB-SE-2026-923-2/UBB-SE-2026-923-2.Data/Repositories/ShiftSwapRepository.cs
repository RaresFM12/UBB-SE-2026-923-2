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

        public int AddShiftSwapRequest(ShiftSwapRequest request)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();

            var entity = new ShiftSwapRequest
            {
                ShiftId = request.ShiftId,
                RequesterId = request.RequesterId,
                ColleagueId = request.ColleagueId,
                RequestedAt = request.RequestedAt,
                Status = request.Status,
            };

            databaseContext.ShiftSwapRequests.Add(entity);
            databaseContext.SaveChanges();
            return entity.SwapId;
        }

        public IReadOnlyList<ShiftSwapRequest> GetAllShiftSwapRequests()
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.ShiftSwapRequests.AsNoTracking().ToList();
        }

        public ShiftSwapRequest? GetShiftSwapRequestById(int swapId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.ShiftSwapRequests.AsNoTracking().FirstOrDefault(swapRequest => swapRequest.SwapId == swapId);
        }

        public void UpdateShiftSwapRequestStatus(int swapId, string status)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var swapRequest = databaseContext.ShiftSwapRequests.FirstOrDefault(swapRequest => swapRequest.SwapId == swapId);
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

            databaseContext.SaveChanges();
        }
    }
}