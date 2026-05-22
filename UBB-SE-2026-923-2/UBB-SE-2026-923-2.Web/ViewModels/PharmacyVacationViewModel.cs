namespace UBB_SE_2026_923_2.Web.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public sealed class PharmacyVacationViewModel
    {
        public int? PharmacistStaffId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public IReadOnlyList<SelectListItem> Pharmacists { get; set; } = new List<SelectListItem>();

        public string? StatusMessage { get; set; }

        public string StatusCssClass { get; set; } = "alert-info";
    }
}
