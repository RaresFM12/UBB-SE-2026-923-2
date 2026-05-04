using System.Collections.Generic;

namespace UBB_SE_2026_923_2.Repositories
{
    public interface IHighRiskMedicineRepository
    {
        IReadOnlyList<(string MedicineName, string WarningMessage)> GetAllHighRiskMedicines();
    }
}
