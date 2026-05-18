namespace UBB_SE_2026_923_2.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Web.Models;

    [Authorize(Roles = "Admin")]
    public class StatisticsController : Controller
    {
        private readonly IAdminService adminService;

        public StatisticsController(IAdminService adminService)
        {
            this.adminService = adminService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<Tuple<int, string, int>> rawTopItems = this.adminService.GetTop30Items();
            Dictionary<string, int> topSubstances = this.adminService.GetTop30Substances();

            TopItemViewModel MapTupleToTopItemViewModel(Tuple<int, string, int> tuple) =>
                new TopItemViewModel
                {
                    ItemId = tuple.Item1,
                    ItemName = tuple.Item2,
                    OrderCount = tuple.Item3,
                };

            var viewModel = new AdminStatisticsViewModel
            {
                TopItems = rawTopItems.ConvertAll(MapTupleToTopItemViewModel),
                TopSubstances = topSubstances,
            };

            return this.View(viewModel);
        }
    }
}
