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
                .Include(hangoutParticipant => hangoutParticipant.Hangout)
                .Include(hangoutParticipant => hangoutParticipant.Staff)
                .AsEnumerable()
                .Select(hangoutParticipant => (hangoutParticipant.Hangout.HangoutID, hangoutParticipant.Staff.StaffID))
                .ToList();
        }

        public void AddParticipant(int hangoutId, int staffId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();

            var hangoutToJoin = databaseContext.Hangouts.Find(hangoutId)
                ?? throw new ArgumentException($"Hangout with identifier {hangoutId} not found.", nameof(hangoutId));

            var participatingStaffMember = databaseContext.StaffMembers.Find(staffId)
                ?? throw new ArgumentException($"Staff member with identifier {staffId} not found.", nameof(staffId));

            databaseContext.HangoutParticipants.Add(new HangoutParticipant
            {
                Hangout = hangoutToJoin,
                Staff = participatingStaffMember,
            });

            databaseContext.SaveChanges();
        }
    }
}
