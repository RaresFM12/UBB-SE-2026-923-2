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

        private readonly IDbContextFactory<AppDbContext> databaseContextFactory;

        public SQLSubstancesRepository(IDbContextFactory<AppDbContext> databaseContextFactory)
        {
            this.databaseContextFactory = databaseContextFactory ?? throw new ArgumentNullException(nameof(databaseContextFactory));
        }

        public void AddSubstance(string name, float lethalDose, string description)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            databaseContext.Substances.Add(new Substance(name, lethalDose, description));
            databaseContext.SaveChanges();
        }

        public Substance GetSubstanceByName(string name)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.Substances.AsNoTracking()
                .FirstOrDefault(substance => substance.Name == name)!;
        }

        public List<Substance> GetAllSubstances()
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.Substances.AsNoTracking().ToList();
        }

        public void RemoveSubstanceByName(string name)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var substanceToRemove = databaseContext.Substances
                .FirstOrDefault(substance => substance.Name == name);

            if (substanceToRemove is null)
            {
                return;
            }

            // Cascade is configured on Substance → ItemSubstance, but rows may
            // already be loaded by other operations; remove them defensively
            // so SaveChanges does not collide with FK constraints.
            var itemSubstanceLinksToRemove = databaseContext.ItemSubstances
                .Where(itemSubstanceLink => itemSubstanceLink.Substance.Name == name);

            databaseContext.ItemSubstances.RemoveRange(itemSubstanceLinksToRemove);
            databaseContext.Substances.Remove(substanceToRemove);
            databaseContext.SaveChanges();
        }

        public void UpdateSubstanceByName(Substance substance)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var trackedSubstance = databaseContext.Substances
                .FirstOrDefault(existingSubstance => existingSubstance.Name == substance.Name);

            if (trackedSubstance is null)
            {
                return;
            }

            trackedSubstance.LethalDose = substance.LethalDose;
            trackedSubstance.Description = substance.Description;
            databaseContext.SaveChanges();
        }

        public bool SubstanceExists(string name)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.Substances.AsNoTracking()
                .Any(substance => substance.Name == name);
        }

        public Dictionary<string, int> GetTop30Substances()
        {
            // Replicates the legacy aggregation: substances ranked by the number
            // of (substance, order-item) pairs for orders picked up in the last
            // month, which matches the original query semantics.
            // We start from OrderItems (filtered to recent orders) and expand
            // to ItemSubstances so we count occurrences across qualifying orders.
            DateOnly oneMonthAgo = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));

            using var databaseContext = this.databaseContextFactory.CreateDbContext();

            return databaseContext.OrderItems
                .AsNoTracking()
                .Where(orderItem => orderItem.Order.PickUpDate >= oneMonthAgo)
                .SelectMany(orderItem => orderItem.Item.ItemSubstanceEntries)
                .Select(itemSubstanceLink => itemSubstanceLink.Substance.Name)
                .GroupBy(substanceName => substanceName)
                .Select(substanceGroup => new
                {
                    Name = substanceGroup.Key,
                    NumberOfOrders = substanceGroup.Count(),
                })
                .OrderByDescending(substanceRow => substanceRow.NumberOfOrders)
                .Take(TopSubstancesLimit)
                .ToDictionary(
                    substanceRow => substanceRow.Name,
                    substanceRow => substanceRow.NumberOfOrders);
        }
    }
}
