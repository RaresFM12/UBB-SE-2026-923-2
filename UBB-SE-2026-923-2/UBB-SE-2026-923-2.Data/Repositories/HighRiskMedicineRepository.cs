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
        private readonly IDbContextFactory<AppDbContext> databaseContextFactory;

        public HighRiskMedicineRepository(IDbContextFactory<AppDbContext> databaseContextFactory)
        {
            this.databaseContextFactory = databaseContextFactory ?? throw new ArgumentNullException(nameof(databaseContextFactory));
        }

        public IReadOnlyList<(string MedicineName, string WarningMessage)> GetAllHighRiskMedicines()
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.HighRiskMedicines
                .AsNoTracking()
                .Select(medicine => new { medicine.MedicineName, medicine.WarningMessage })
                .AsEnumerable()
                .Select(medicineRow => (medicineRow.MedicineName, medicineRow.WarningMessage))
                .ToList();
        }
    }
}