namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using UBB_SE_2026_923_2.Data;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// EF Core implementation of <see cref="ISubstancesRepository"/>. All
    /// data access goes through a short-lived <see cref="AppDbContext"/>
    /// obtained from <see cref="IDbContextFactory{AppDbContext}"/>; raw SQL
    /// commands have been removed.
    /// </summary>
    public class SQLSubstancesRepository : ISubstancesRepository
    {
        private const int TopSubstancesLimit = 30;

        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public SQLSubstancesRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public void AddSubstance(string name, float lethalDose, string description)
        {
            using var db = this.dbContextFactory.CreateDbContext();
            db.Substances.Add(new Substance(name, lethalDose, description));
            db.SaveChanges();
        }

        public Substance GetSubstanceByName(string name)
        {
            using var db = this.dbContextFactory.CreateDbContext();
            return db.Substances.AsNoTracking().FirstOrDefault(s => s.Name == name)!;
        }

        public List<Substance> GetAllSubstances()
        {
            using var db = this.dbContextFactory.CreateDbContext();
            return db.Substances.AsNoTracking().ToList();
        }

        public void RemoveSubstanceByName(string name)
        {
            using var db = this.dbContextFactory.CreateDbContext();
            var substance = db.Substances.FirstOrDefault(s => s.Name == name);
            if (substance is null)
            {
                return;
            }

            // Cascade is configured on Substance → ItemSubstance, but rows may
            // already be loaded by other operations; remove them defensively
            // so SaveChanges does not collide with FK constraints.
            var links = db.ItemSubstances.Where(link => link.SubstanceName == name);
            db.ItemSubstances.RemoveRange(links);
            db.Substances.Remove(substance);
            db.SaveChanges();
        }

        public void UpdateSubstanceByName(Substance substance)
        {
            using var db = this.dbContextFactory.CreateDbContext();
            var tracked = db.Substances.FirstOrDefault(s => s.Name == substance.Name);
            if (tracked is null)
            {
                return;
            }

            tracked.LethalDose = substance.LethalDose;
            tracked.Description = substance.Description;
            db.SaveChanges();
        }

        public bool SubstanceExists(string name)
        {
            using var db = this.dbContextFactory.CreateDbContext();
            return db.Substances.AsNoTracking().Any(s => s.Name == name);
        }

        public Dictionary<string, int> GetTop30Substances()
        {
            // Replicates the legacy aggregation: substances ranked by the number
            // of orders that contain at least one item using them, restricted
            // to orders picked up in the last month.
            DateOnly oneMonthAgo = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));

            using var db = this.dbContextFactory.CreateDbContext();

            return db.ItemSubstances
                .AsNoTracking()
                .SelectMany(link => db.OrderItems
                    .Where(oi => oi.ItemId == link.ItemId)
                    .Select(oi => new { link.SubstanceName, oi.OrderId }))
                .Join(
                    db.Orders.Where(o => o.PickUpDate >= oneMonthAgo),
                    pair => pair.OrderId,
                    order => order.Id,
                    (pair, _) => pair.SubstanceName)
                .GroupBy(name => name)
                .Select(group => new { Name = group.Key, NumberOfOrders = group.Count() })
                .OrderByDescending(row => row.NumberOfOrders)
                .Take(TopSubstancesLimit)
                .ToDictionary(row => row.Name, row => row.NumberOfOrders);
        }
    }
}
