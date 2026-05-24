using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Web.ViewModels
{
    public class DoctorScheduleViewModel
    {
        public List<Shift> Shifts { get; set; } = new();
        public List<Appointment> Appointments { get; set; } = new();
    }
}
