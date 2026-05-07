namespace UBB_SE_2026_923_2.Repositories
{
    using System.Collections.Generic;

    public interface IHighRiskMedicineRepository
    {
        IReadOnlyList<(string MedicineName, string WarningMessage)> GetAllHighRiskMedicines();
    }
}
