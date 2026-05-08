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
        private readonly IDbContextFactory<AppDbContext> databaseContextFactory;

        public HangoutRepository(IDbContextFactory<AppDbContext> databaseContextFactory)
        {
            this.databaseContextFactory = databaseContextFactory ?? throw new ArgumentNullException(nameof(databaseContextFactory));
        }

        public int AddHangout(string title, string description, DateTime date, int maximumParticipants)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();

            var hangout = new Hangout
            {
                Title = title,
                Description = description ?? string.Empty,
                Date = date,
                MaxParticipants = maximumParticipants,
            };

            databaseContext.Hangouts.Add(hangout);
            databaseContext.SaveChanges();
            return hangout.HangoutID;
        }

        public List<Hangout> GetAllHangouts()
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.Hangouts.AsNoTracking().ToList();
        }

        public Hangout? GetHangoutById(int hangoutId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.Hangouts.AsNoTracking().FirstOrDefault(hangout => hangout.HangoutID == hangoutId);
        }
    }
}