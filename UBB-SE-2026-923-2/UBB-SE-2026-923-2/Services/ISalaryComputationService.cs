namespace UBB_SE_2026_923_2.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBB_SE_2026_923_2.Models;

    public interface ISalaryComputationService
    {
        Task<double> ComputeSalaryDoctorAsync(Doctor doctor, List<Shift> monthlyShifts, int month, int year);

        Task<double> ComputeSalaryPharmacistAsync(Pharmacyst pharmacist, List<Shift> monthlyShifts, int month, int year);

        List<IStaff> GetAllStaff();

        List<Shift> GetAllShifts();
    }
}
