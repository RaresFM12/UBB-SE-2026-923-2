namespace UBB_SE_2026_923_2.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Web.ViewModels;

    [Authorize(Roles = "Doctor")]
    public class HangoutsController : Controller
    {
        private readonly IHangoutService hangoutService;
        private readonly IDoctorAppointmentService doctorAppointmentService;

        public HangoutsController(IHangoutService hangoutService, IDoctorAppointmentService doctorAppointmentService)
        {
            this.hangoutService = hangoutService;
            this.doctorAppointmentService = doctorAppointmentService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Hangout> hangouts = this.hangoutService.GetAllHangouts();
            List<DoctorOptionViewModel> doctors = await this.LoadDoctorOptionsAsync();

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

            var viewModel = new HangoutsIndexViewModel
            {
                Hangouts = hangouts.ConvertAll(MapHangoutToViewModel),
                Doctors = doctors,
            };

            return this.View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            List<DoctorOptionViewModel> doctors = await this.LoadDoctorOptionsAsync();
            return this.View(new CreateHangoutViewModel { Doctors = doctors });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateHangoutViewModel viewModel)
        {
            if (!this.ModelState.IsValid)
            {
                viewModel.Doctors = await this.LoadDoctorOptionsAsync();
                return this.View(viewModel);
            }

            var creator = new Staff { StaffID = viewModel.SelectedDoctorId };

            try
            {
                this.hangoutService.CreateHangout(
                    viewModel.Title,
                    viewModel.Description,
                    viewModel.Date,
                    viewModel.MaxParticipantsCount,
                    creator);

                return this.RedirectToAction(nameof(this.Index));
            }
            catch (ArgumentException argumentException)
            {
                this.ModelState.AddModelError(string.Empty, argumentException.Message);
                viewModel.Doctors = await this.LoadDoctorOptionsAsync();
                return this.View(viewModel);
            }
            catch (InvalidOperationException operationException)
            {
                this.ModelState.AddModelError(string.Empty, operationException.Message);
                viewModel.Doctors = await this.LoadDoctorOptionsAsync();
                return this.View(viewModel);
            }
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            bool HasMatchingId(Hangout hangout) => hangout.HangoutID == id;
            Hangout hangout = this.hangoutService.GetAllHangouts().Find(HasMatchingId);
            if (hangout == null)
            {
                return this.NotFound();
            }

            HangoutViewModel viewModel = new HangoutViewModel
            {
                HangoutId = hangout.HangoutID,
                Title = hangout.Title,
                Description = hangout.Description,
                FormattedDate = hangout.FormattedDate,
                ParticipantCount = hangout.ParticipantList.Count,
                MaxParticipants = hangout.MaxParticipants,
                IsFull = hangout.ParticipantList.Count >= hangout.MaxParticipants,
            };

            return this.View(viewModel);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, object model)
        {
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            return this.View();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            return this.RedirectToAction(nameof(this.Index));
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

        private async Task<List<DoctorOptionViewModel>> LoadDoctorOptionsAsync()
        {
            IReadOnlyList<(int DoctorId, string DoctorName)> doctors =
                await this.doctorAppointmentService.GetAllDoctorsAsync();

            DoctorOptionViewModel MapDoctorToOption((int DoctorId, string DoctorName) doctor) =>
                new DoctorOptionViewModel
                {
                    DoctorId = doctor.DoctorId,
                    DoctorName = doctor.DoctorName,
                };

            var result = new List<DoctorOptionViewModel>();
            foreach ((int DoctorId, string DoctorName) doctor in doctors)
            {
                result.Add(MapDoctorToOption(doctor));
            }

            return result;
        }
    }
}
