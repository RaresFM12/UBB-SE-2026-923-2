namespace UBB_SE_2026_923_2.Web.ViewModels
{
    using System.Collections.Generic;

    public class AdminStatisticsViewModel
    {
        public List<TopItemViewModel> TopItems { get; set; } = new List<TopItemViewModel>();

        public Dictionary<string, int> TopSubstances { get; set; } = new Dictionary<string, int>();
    }
}
