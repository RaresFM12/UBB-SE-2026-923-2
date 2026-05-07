using System.Collections.Generic;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    public interface IPharmacyHandoverRepository
    {
        IReadOnlyList<PharmacyHandover> GetAllPharmacyHandovers();
    }
}
