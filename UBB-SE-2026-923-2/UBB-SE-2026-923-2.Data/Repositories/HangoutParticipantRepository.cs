namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using UBB_SE_2026_923_2.Data;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// EF Core implementation of <see cref="IHangoutParticipantRepository"/>.
    /// </summary>
    public class HangoutParticipantRepository : IHangoutParticipantRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public HangoutParticipantRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public IReadOnlyList<(int HangoutId, int StaffId)> GetAllParticipants()
        {
            using var db = this.dbContextFactory.CreateDbContext();
            return db.HangoutParticipants
                .AsNoTracking()
                .Select(p => new { p.HangoutId, p.StaffId })
                .AsEnumerable()
                .Select(row => (row.HangoutId, row.StaffId))
                .ToList();
        }

        public void AddParticipant(int hangoutId, int staffId)
        {
            using var db = this.dbContextFactory.CreateDbContext();
            db.HangoutParticipants.Add(new HangoutParticipant
            {
                HangoutId = hangoutId,
                StaffId = staffId,
            });
            db.SaveChanges();
        }
    }
}
