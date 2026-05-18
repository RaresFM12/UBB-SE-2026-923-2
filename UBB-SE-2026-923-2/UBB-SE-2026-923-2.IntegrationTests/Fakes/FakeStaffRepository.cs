namespace UBB_SE_2026_923_2.IntegrationTests.Fakes
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;

    public sealed class FakeStaffRepository : IStaffRepository
    {
        private readonly List<IStaff> staff = new();

        public void Seed(params IStaff[] entries)
        {
            if (entries.Length == 0)
            {
                return;
            }

            this.staff.AddRange(entries);
        }

        public List<IStaff> LoadAllStaff() => this.staff.ToList();

        public IStaff? GetStaffById(int staffId) => this.staff.FirstOrDefault(item => item.StaffID == staffId);

        public Task<IReadOnlyList<(int DoctorId, string FirstName, string LastName)>> GetAllDoctorsAsync()
        {
            var doctors = this.staff
                .OfType<Doctor>()
                .Select(doctor => (doctor.StaffID, doctor.FirstName, doctor.LastName))
                .ToList();
            return Task.FromResult<IReadOnlyList<(int DoctorId, string FirstName, string LastName)>>(doctors);
        }

        public Task UpdateStatusAsync(int staffId, string status)
        {
            if (this.GetStaffById(staffId) is Staff staffMember)
            {
                staffMember.Status = status;
            }

            return Task.CompletedTask;
        }
    }
}
