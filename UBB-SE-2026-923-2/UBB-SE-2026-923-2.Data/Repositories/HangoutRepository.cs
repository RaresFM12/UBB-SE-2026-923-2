namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using UBB_SE_2026_923_2.Data;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// EF Core implementation of <see cref="IHangoutRepository"/>. Participants
    /// are managed through <see cref="IHangoutParticipantRepository"/> and the
    /// <see cref="Hangout.HangoutParticipantEntries"/> navigation collection.
    /// </summary>
    public class HangoutRepository : IHangoutRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public HangoutRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public int AddHangout(string title, string description, DateTime date, int maxParticipants)
        {
            using var db = this.dbContextFactory.CreateDbContext();

            var hangout = new Hangout
            {
                Title = title,
                Description = description ?? string.Empty,
                Date = date,
                MaxParticipants = maxParticipants,
            };

            db.Hangouts.Add(hangout);
            db.SaveChanges();
            return hangout.HangoutID;
        }

        public List<Hangout> GetAllHangouts()
        {
            using var db = this.dbContextFactory.CreateDbContext();
            return db.Hangouts.AsNoTracking().ToList();
        }

        public Hangout? GetHangoutById(int hangoutId)
        {
            using var db = this.dbContextFactory.CreateDbContext();
            return db.Hangouts.AsNoTracking().FirstOrDefault(h => h.HangoutID == hangoutId);
        }
    }
}
