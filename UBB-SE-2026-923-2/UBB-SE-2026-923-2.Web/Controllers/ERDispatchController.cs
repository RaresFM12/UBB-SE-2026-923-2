namespace UBB_SE_2026_923_2.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBB_SE_2026_923_2.Models;
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
        private const int OverrideWindowDays = 3;
        private const int MinutesPerHour = 60;
        private const int HoursPerDay = 24;
        private const int NearEndMinutes = OverrideWindowDays * HoursPerDay * MinutesPerHour;
        private const int SimulatedRequestCount = 3;

        private const string PendingStatus = "PENDING";
        private const string AssignedStatus = "ASSIGNED";
        private const string UnmatchedStatus = "UNMATCHED";
        private const string CancelledStatus = "CANCELLED";
        private const string DashboardStateTempDataKey = "ERDispatchDashboardState";
        private const string UnknownDoctorName = "Unknown";
        private const string ManualOverrideHintText = "Manual override accepts IN_EXAMINATION doctors whose active shift ends within 3 days.";

        private readonly IERDispatchService dispatchService;

        public ERDispatchController(IERDispatchService dispatchService)
        {
            this.dispatchService = dispatchService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? selectedRequestId = null)
        {
            var dashboard = this.LoadDashboardState();
            if (selectedRequestId.HasValue)
            {
                dashboard.SelectedRequestId = selectedRequestId;
            }

            await this.LoadOverrideCandidatesAsync(dashboard);
            this.SaveDashboardState(dashboard);
            this.TempData.Keep(DashboardStateTempDataKey);

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
            var dashboard = this.LoadDashboardState();
            dashboard.StatusMessage = $"Simulated {createdIds.Count} incoming request(s) from Clinical Team. Click Run Dispatch.";
            dashboard.ManualInterventionHint = "Incoming ER requests were added as PENDING.";
            this.SaveDashboardState(dashboard);
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DispatchAll()
        {
            var results = await this.dispatchService.DispatchAllPendingAsync();
            var dashboard = new ErDispatchDashboardViewModel
            {
                StatusMessage = "Dispatching...",
                ManualInterventionHint = ManualOverrideHintText,
                SessionMatches = results
                    .Where(result => result.IsSuccess)
                    .Select(ToSuccessfulMatch)
                    .ToList(),
                SessionUnmatched = results
                    .Where(result => !result.IsSuccess && result.Request != null)
                    .Select(ToUnmatchedRequest)
                    .ToList(),
            };

            dashboard.StatusMessage = $"{dashboard.SessionMatches.Count} matched, {dashboard.SessionUnmatched.Count} unmatched";
            dashboard.SelectedRequestId = dashboard.SessionUnmatched.FirstOrDefault()?.RequestId;
            if (dashboard.SelectedRequestId == null)
            {
                dashboard.ManualInterventionHint = "No unmatched requests. Override not needed.";
            }

            await this.LoadOverrideCandidatesAsync(dashboard);
            this.SaveDashboardState(dashboard);
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
                Candidates = candidates.Select(OverrideCandidateViewModel.From).ToList(),
            };
            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Override(int id, int selectedDoctorId)
        {
            var result = await this.dispatchService.ManualOverrideAsync(id, selectedDoctorId, NearEndMinutes);
            var dashboard = this.LoadDashboardState();

            if (result.IsSuccess && result.Request != null)
            {
                dashboard.SessionUnmatched = dashboard.SessionUnmatched
                    .Where(request => request.RequestId != id)
                    .ToList();

                var matches = dashboard.SessionMatches.ToList();
                matches.Add(ToSuccessfulMatch(result));
                dashboard.SessionMatches = matches;
                dashboard.StatusMessage = $"{dashboard.SessionMatches.Count} matched, {dashboard.SessionUnmatched.Count} unmatched";
            }

            dashboard.ManualInterventionHint = result.Message;
            dashboard.SelectedRequestId = dashboard.SessionUnmatched.FirstOrDefault()?.RequestId;
            dashboard.SelectedDoctorId = null;
            if (dashboard.SessionUnmatched.Count == 0 && result.IsSuccess)
            {
                dashboard.ManualInterventionHint = "Override applied. No unmatched requests left.";
            }

            await this.LoadOverrideCandidatesAsync(dashboard);
            this.SaveDashboardState(dashboard);
            return this.RedirectToAction(nameof(this.Index));
        }

        private static bool IsStatus(string? actual, string expected) =>
            string.Equals((actual ?? string.Empty).Trim(), expected, StringComparison.OrdinalIgnoreCase);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Refresh()
        {
            this.TempData.Remove(DashboardStateTempDataKey);
            return this.RedirectToAction(nameof(this.Index));
        }

        private ErDispatchDashboardViewModel LoadDashboardState()
        {
            if (this.TempData.TryGetValue(DashboardStateTempDataKey, out var serializedState)
                && serializedState is string stateJson
                && !string.IsNullOrWhiteSpace(stateJson))
            {
                return JsonSerializer.Deserialize<ErDispatchDashboardViewModel>(stateJson) ?? new ErDispatchDashboardViewModel();
            }

            return new ErDispatchDashboardViewModel();
        }

        private void SaveDashboardState(ErDispatchDashboardViewModel dashboard)
        {
            this.TempData[DashboardStateTempDataKey] = JsonSerializer.Serialize(dashboard);
        }

        private async Task LoadOverrideCandidatesAsync(ErDispatchDashboardViewModel dashboard)
        {
            dashboard.OverrideCandidates = new List<OverrideCandidateViewModel>();
            dashboard.SelectedDoctorId = null;

            if (dashboard.SelectedRequestId == null)
            {
                return;
            }

            var selectedRequestExists = dashboard.SessionUnmatched.Any(request => request.RequestId == dashboard.SelectedRequestId);
            if (!selectedRequestExists)
            {
                dashboard.SelectedRequestId = dashboard.SessionUnmatched.FirstOrDefault()?.RequestId;
            }

            if (dashboard.SelectedRequestId == null)
            {
                return;
            }

            var candidates = await this.dispatchService.GetManualOverrideCandidatesAsync(dashboard.SelectedRequestId.Value, NearEndMinutes);
            dashboard.OverrideCandidates = candidates.Select(OverrideCandidateViewModel.From).ToList();
            dashboard.ManualInterventionHint = dashboard.OverrideCandidates.Count == 0
                ? "No eligible override doctor found (need IN_EXAMINATION doctor ending within 3 days)."
                : $"Found {dashboard.OverrideCandidates.Count} eligible override candidate(s).";
        }

        private static UnmatchedERRequestViewModel ToUnmatchedRequest(ERDispatchResult result) =>
            new UnmatchedERRequestViewModel
            {
                RequestId = result.Request.Id,
                RequestSpecialization = result.Request.Specialization,
                RequestLocation = result.Request.Location,
                NoMatchReason = result.Message,
            };

        private static SuccessfulERMatchViewModel ToSuccessfulMatch(ERDispatchResult result) =>
            new SuccessfulERMatchViewModel
            {
                RequestId = result.Request.Id,
                AssignedDoctor = result.MatchedDoctorName ?? UnknownDoctorName,
                Specialization = result.Request.Specialization,
                MatchReason = result.MatchReason,
            };
    }
}
