namespace UBB_SE_2026_923_2.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Web.ViewModels;

    /// <summary>
    /// Thin MVC front end over <see cref="IERDispatchService"/>. ER Dispatch is
    /// an admin-only console in the desktop app (registered under
    /// <c>case UserRole.Admin</c> in RoleDashboardPage), so the whole
    /// controller is gated to the Admin role.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class ERDispatchController : Controller
    {
        private const int NearEndMinutes = 30;
        private const int SimulatedRequestCount = 3;

        private const string PendingStatus = "PENDING";
        private const string AssignedStatus = "ASSIGNED";
        private const string UnmatchedStatus = "UNMATCHED";
        private const string CancelledStatus = "CANCELLED";

        private readonly IERDispatchService dispatchService;

        public ERDispatchController(IERDispatchService dispatchService)
        {
            this.dispatchService = dispatchService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var allRequests = await this.dispatchService.GetAllRequestsAsync();

            var dashboard = new ErDispatchDashboardViewModel
            {
                Pending = allRequests.Where(request => IsStatus(request.Status, PendingStatus)).ToList(),
                Assigned = allRequests.Where(request => IsStatus(request.Status, AssignedStatus)).ToList(),
                Unmatched = allRequests.Where(request => IsStatus(request.Status, UnmatchedStatus)).ToList(),
                Cancelled = allRequests.Where(request => IsStatus(request.Status, CancelledStatus)).ToList(),
            };

            return this.View(dashboard);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var request = await this.dispatchService.GetRequestByIdAsync(id);
            if (request == null)
            {
                return this.NotFound();
            }

            return this.View(request);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return this.View(new CreateERRequestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateERRequestViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            await this.dispatchService.CreateRequestAsync(model.Specialization, model.Location);
            this.TempData["StatusMessage"] = $"Created PENDING request: {model.Specialization} @ {model.Location}.";
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var request = await this.dispatchService.GetRequestByIdAsync(id);
            if (request == null)
            {
                return this.NotFound();
            }

            var model = new EditERRequestStatusViewModel
            {
                Id = request.Id,
                Specialization = request.Specialization,
                Location = request.Location,
                Status = request.Status,
            };
            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditERRequestStatusViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            try
            {
                // The status-transition rule lives in the service; the
                // controller only translates a rejected transition into a
                // model-state error (same pattern as LoginController).
                await this.dispatchService.UpdateRequestStatusAsync(model.Id, model.Status);
            }
            catch (InvalidOperationException exception)
            {
                this.ModelState.AddModelError(string.Empty, exception.Message);
                return this.View(model);
            }

            this.TempData["StatusMessage"] = $"Request #{model.Id} status set to {model.Status}.";
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await this.dispatchService.GetRequestByIdAsync(id);
            if (request == null)
            {
                return this.NotFound();
            }

            return this.View(request);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // The WebApi exposes no DELETE endpoint and the assignment forbids
            // changing it, so "delete" is a soft-cancel: status -> CANCELLED.
            await this.dispatchService.UpdateRequestStatusAsync(id, CancelledStatus);
            this.TempData["StatusMessage"] = $"Request #{id} cancelled.";
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Simulate()
        {
            var createdIds = await this.dispatchService.SimulateIncomingRequestsAsync(SimulatedRequestCount);
            this.TempData["StatusMessage"] = $"Simulated {createdIds.Count} incoming ER request(s).";
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DispatchAll()
        {
            var results = await this.dispatchService.DispatchAllPendingAsync();
            var matched = results.Count(result => result.IsSuccess);
            this.TempData["StatusMessage"] = $"{matched} matched, {results.Count - matched} unmatched.";
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dispatch(int id)
        {
            var result = await this.dispatchService.DispatchERRequestAsync(id);
            this.TempData["StatusMessage"] = result.Message;
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpGet]
        public async Task<IActionResult> Override(int id)
        {
            var request = await this.dispatchService.GetRequestByIdAsync(id);
            if (request == null)
            {
                return this.NotFound();
            }

            var candidates = await this.dispatchService.GetManualOverrideCandidatesAsync(id, NearEndMinutes);
            var model = new OverrideViewModel
            {
                RequestId = request.Id,
                RequestSummary = $"#{request.Id} - {request.Specialization} @ {request.Location}",
                Candidates = candidates,
            };
            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Override(int id, int selectedDoctorId)
        {
            var result = await this.dispatchService.ManualOverrideAsync(id, selectedDoctorId, NearEndMinutes);
            this.TempData["StatusMessage"] = result.Message;
            return this.RedirectToAction(nameof(this.Index));
        }

        private static bool IsStatus(string? actual, string expected) =>
            string.Equals((actual ?? string.Empty).Trim(), expected, StringComparison.OrdinalIgnoreCase);
    }
}
