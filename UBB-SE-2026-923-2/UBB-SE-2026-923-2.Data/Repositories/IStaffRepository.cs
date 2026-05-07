using System.Collections.Generic;
using System.Threading.Tasks;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    public interface IStaffRepository
    {
        List<IStaff> LoadAllStaff();

        IStaff? GetStaffById(int staffId);

        Task<IReadOnlyList<(int DoctorId, string FirstName, string LastName)>> GetAllDoctorsAsync();

        Task UpdateStatusAsync(int staffId, string status);
    }
}
