namespace UBB_SE_2026_923_2.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Web.Models;

    [Authorize]
    public class HangoutsController : Controller
    {
        private readonly IHangoutService hangoutService;

        public HangoutsController(IHangoutService hangoutService)
        {
            this.hangoutService = hangoutService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<Hangout> hangouts = this.hangoutService.GetAllHangouts();

            HangoutViewModel MapHangoutToViewModel(Hangout hangout) =>
                new HangoutViewModel
                {
                    HangoutId = hangout.HangoutID,
                    Title = hangout.Title,
                    Description = hangout.Description,
                    FormattedDate = hangout.FormattedDate,
                    ParticipantCount = hangout.ParticipantList.Count,
                    MaxParticipants = hangout.MaxParticipants,
                    IsFull = hangout.ParticipantList.Count >= hangout.MaxParticipants,
                };

            List<HangoutViewModel> viewModels = hangouts.ConvertAll(MapHangoutToViewModel);
            return this.View(viewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return this.View(new CreateHangoutViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateHangoutViewModel viewModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(viewModel);
            }

            // The service requires an IStaff instance to identify the creator.
            // The StaffID is provided explicitly in the form because staff identity
            // is separate from the web User authentication principal.
            var creator = new Staff { StaffID = viewModel.CreatorStaffId };

            try
            {
                this.hangoutService.CreateHangout(
                    viewModel.Title,
                    viewModel.Description,
                    viewModel.Date,
                    viewModel.MaxParticipants,
                    creator);

                return this.RedirectToAction(nameof(this.Index));
            }
            catch (ArgumentException argumentException)
            {
                this.ModelState.AddModelError(string.Empty, argumentException.Message);
                return this.View(viewModel);
            }
            catch (InvalidOperationException operationException)
            {
                this.ModelState.AddModelError(string.Empty, operationException.Message);
                return this.View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Join(int hangoutId, int staffId)
        {
            var joiningStaff = new Staff { StaffID = staffId };

            try
            {
                this.hangoutService.JoinHangout(hangoutId, joiningStaff);
            }
            catch (ArgumentException argumentException)
            {
                this.TempData["ErrorMessage"] = argumentException.Message;
            }
            catch (InvalidOperationException operationException)
            {
                this.TempData["ErrorMessage"] = operationException.Message;
            }

            return this.RedirectToAction(nameof(this.Index));
        }
    }
}
