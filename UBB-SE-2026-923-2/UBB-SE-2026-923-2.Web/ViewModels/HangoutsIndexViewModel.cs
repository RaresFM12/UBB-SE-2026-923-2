namespace UBB_SE_2026_923_2.Web.ViewModels
{
    using System.Collections.Generic;

    public class HangoutsIndexViewModel
    {
        public List<HangoutViewModel> Hangouts { get; set; } = new List<HangoutViewModel>();

        public List<DoctorOptionViewModel> Doctors { get; set; } = new List<DoctorOptionViewModel>();
    }
}
