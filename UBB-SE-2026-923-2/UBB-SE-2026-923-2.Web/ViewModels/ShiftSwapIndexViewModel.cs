namespace UBB_SE_2026_923_2.Web.ViewModels
{
    using UBB_SE_2026_923_2.Models;

    public class ShiftSwapIndexViewModel
    {
        public List<Doctor> Doctors { get; set; } = new();
        public List<Shift> FutureShifts { get; set; } = new();
        public List<IStaff> EligibleColleagues { get; set; } = new();
        public int? SelectedDoctorId { get; set; }
        public int? SelectedShiftId { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public bool AlreadyRequested { get; set; } = false;
        public HashSet<int> PendingShiftIds { get; set; } = new();
    }

    public class IncomingSwapRequestsViewModel
    {
        public List<Doctor> Doctors { get; set; } = new();
        public List<ShiftSwapRequest> Requests { get; set; } = new();
        public int? SelectedDoctorId { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
    }
}
