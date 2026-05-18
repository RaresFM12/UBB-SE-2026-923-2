namespace UBB_SE_2026_923_2.Web.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// Read model for the dispatch dashboard (the controller's <c>Index</c>).
    /// Requests are pre-grouped by status so the view stays logic-free.
    /// </summary>
    public class ErDispatchDashboardViewModel
    {
        public IReadOnlyList<ERRequest> Pending { get; init; } = new List<ERRequest>();

        public IReadOnlyList<ERRequest> Assigned { get; init; } = new List<ERRequest>();

        public IReadOnlyList<ERRequest> Unmatched { get; init; } = new List<ERRequest>();

        public IReadOnlyList<ERRequest> Cancelled { get; init; } = new List<ERRequest>();
    }

    public class CreateERRequestViewModel
    {
        [Required(ErrorMessage = "Specialization is required.")]
        [Display(Name = "Specialization")]
        public string Specialization { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;
    }

    public class EditERRequestStatusViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Specialization")]
        public string Specialization { get; set; } = string.Empty;

        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        // ASSIGNED / UNMATCHED are outcomes the dispatch engine owns and must
        // not be settable by hand (the service rejects them too); an admin may
        // only re-open (PENDING) or cancel (CANCELLED) a request.
        public static IReadOnlyList<string> AvailableStatuses { get; } = new[]
        {
            "PENDING",
            "CANCELLED",
        };
    }

    /// <summary>
    /// Model for the <c>_RequestTable</c> partial: one status-grouped table on
    /// the dashboard. <see cref="Kind"/> drives which row actions render.
    /// </summary>
    public class ErDispatchTableViewModel
    {
        public string Title { get; init; } = string.Empty;

        public string HeaderCss { get; init; } = string.Empty;

        public string Kind { get; init; } = string.Empty;

        public IReadOnlyList<ERRequest> Rows { get; init; } = new List<ERRequest>();
    }

    public class OverrideViewModel
    {
        public int RequestId { get; set; }

        public string RequestSummary { get; set; } = string.Empty;

        public int SelectedDoctorId { get; set; }

        public IReadOnlyList<DoctorProfile> Candidates { get; init; } = new List<DoctorProfile>();
    }
}
