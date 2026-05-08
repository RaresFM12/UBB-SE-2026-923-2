namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using UBB_SE_2026_923_2.Data;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// EF Core implementation of <see cref="IPharmacyHandoverRepository"/>.
    /// </summary>
    public class PharmacyHandoverRepository : IPharmacyHandoverRepository
    {
        private readonly IDbContextFactory<AppDbContext> databaseContextFactory;

        public PharmacyHandoverRepository(IDbContextFactory<AppDbContext> databaseContextFactory)
        {
            this.databaseContextFactory = databaseContextFactory ?? throw new ArgumentNullException(nameof(databaseContextFactory));
        }

        public IReadOnlyList<PharmacyHandover> GetAllPharmacyHandovers()
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.PharmacyHandovers.AsNoTracking().ToList();
        }
    }
}