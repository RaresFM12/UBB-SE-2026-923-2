using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UBB_SE_2026_923_2.Data;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="IPharmacyHandoverRepository"/>.
    /// </summary>
    public class PharmacyHandoverRepository : IPharmacyHandoverRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public PharmacyHandoverRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public IReadOnlyList<PharmacyHandover> GetAllPharmacyHandovers()
        {
            using var db = dbContextFactory.CreateDbContext();
            return db.PharmacyHandovers.AsNoTracking().ToList();
        }
    }
}
