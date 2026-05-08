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
        private readonly IDbContextFactory<AppDbContext> databaseContextFactory;

        public HangoutParticipantRepository(IDbContextFactory<AppDbContext> databaseContextFactory)
        {
            this.databaseContextFactory = databaseContextFactory ?? throw new ArgumentNullException(nameof(databaseContextFactory));
        }

        public IReadOnlyList<(int HangoutId, int StaffId)> GetAllParticipants()
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.HangoutParticipants
                .AsNoTracking()
                .Select(participant => new { participant.HangoutId, participant.StaffId })
                .AsEnumerable()
                .Select(participantRow => (participantRow.HangoutId, participantRow.StaffId))
                .ToList();
        }

        public void AddParticipant(int hangoutId, int staffId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            databaseContext.HangoutParticipants.Add(new HangoutParticipant
            {
                HangoutId = hangoutId,
                StaffId = staffId,
            });
            databaseContext.SaveChanges();
        }
    }
}