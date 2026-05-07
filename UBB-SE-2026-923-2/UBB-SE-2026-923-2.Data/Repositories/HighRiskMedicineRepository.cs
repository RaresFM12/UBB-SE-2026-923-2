namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using UBB_SE_2026_923_2.Data;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// EF Core implementation of <see cref="IHighRiskMedicineRepository"/>.
    /// Reads from the <see cref="HighRiskMedicine"/> reference table.
    /// </summary>
    public class HighRiskMedicineRepository : IHighRiskMedicineRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public HighRiskMedicineRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public IReadOnlyList<(string MedicineName, string WarningMessage)> GetAllHighRiskMedicines()
        {
            using var db = this.dbContextFactory.CreateDbContext();
            return db.HighRiskMedicines
                .AsNoTracking()
                .Select(m => new { m.MedicineName, m.WarningMessage })
                .AsEnumerable()
                .Select(row => (row.MedicineName, row.WarningMessage))
                .ToList();
        }
    }
}
