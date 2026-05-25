using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.Web.ViewModels;

namespace UBB_SE_2026_923_2.Web.Controllers
{
    [Authorize(Roles = "Client")]
    public class PeriodTrackerController : Controller
    {
        private readonly IPeriodTrackerService _periodTrackerService;
        private readonly IBasketService _basketService;

        public PeriodTrackerController(
            IPeriodTrackerService periodTrackerService,
            IBasketService basketService)
        {
            _periodTrackerService = periodTrackerService;
            _basketService = basketService;
        }

        // GET: PeriodTracker
        [HttpGet]
        public IActionResult Index(int monthOffset = 0)
        {
            return View(ToViewModel(_periodTrackerService.GetDashboardSnapshot(monthOffset)));
        }

        // GET: PeriodTracker/Details
        [HttpGet]
        public IActionResult Details(int id = 0)
        {
            var state = _periodTrackerService.GetTrackerState();
            if (!state.HasPeriodTracker)
            {
                return RedirectToAction(nameof(Create));
            }

            return View(ToViewModel(_periodTrackerService.GetDashboardSnapshot(0)));
        }

        // GET: PeriodTracker/Create
        [HttpGet]
        public IActionResult Create()
        {
            var state = _periodTrackerService.GetTrackerState();
            if (state.HasPeriodTracker) return RedirectToAction(nameof(Index));

            return View(new PeriodTrackerViewModel());
        }

        // POST: PeriodTracker/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PeriodTrackerInputModel inputModel)
        {

            if (inputModel.StartPeriodDate == default || inputModel.CycleDays < 20 || inputModel.CycleDays > 45 || inputModel.PeriodLasts < 1 || inputModel.PeriodLasts > 9 || inputModel.PMSOption < 0 || inputModel.PMSOption > 3)
            {
                // Rebuild the dashboard model strictly for returning the user to the form
                return View(new PeriodTrackerViewModel
                {
                    StartPeriodDate = DateOnly.FromDateTime(inputModel.StartPeriodDate),
                    CycleDays = inputModel.CycleDays,
                    PeriodLasts = inputModel.PeriodLasts,
                    PMSOption = inputModel.PMSOption
                });
            }

            _periodTrackerService.UpdatePeriodTracker(inputModel.StartPeriodDate, inputModel.CycleDays, inputModel.PeriodLasts, inputModel.PMSOption);
            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        // GET: PeriodTracker/Edit
        [HttpGet]
        public IActionResult Edit(int id = 0)
        {
            var state = _periodTrackerService.GetTrackerState();
            if (!state.HasPeriodTracker)
            {
                return RedirectToAction(nameof(Create));
            }

            return View(ToViewModel(state));
        }

        // POST: PeriodTracker/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PeriodTrackerInputModel inputModel)
        {
            if (inputModel.StartPeriodDate == default || inputModel.CycleDays < 20 || inputModel.CycleDays > 45 || inputModel.PeriodLasts < 1 || inputModel.PeriodLasts > 9 || inputModel.PMSOption < 0 || inputModel.PMSOption > 3)
            {
                return View(new PeriodTrackerViewModel
                {
                    StartPeriodDate = DateOnly.FromDateTime(inputModel.StartPeriodDate),
                    CycleDays = inputModel.CycleDays,
                    PeriodLasts = inputModel.PeriodLasts,
                    PMSOption = inputModel.PMSOption
                });
            }

            _periodTrackerService.UpdatePeriodTracker(inputModel.StartPeriodDate, inputModel.CycleDays, inputModel.PeriodLasts, inputModel.PMSOption);
            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        // GET: PeriodTracker/Delete
        [HttpGet]
        public IActionResult Delete(int id = 0)
        {
            var state = _periodTrackerService.GetTrackerState();
            if (!state.HasPeriodTracker)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(ToViewModel(state));
        }

        // POST: PeriodTracker/Delete
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id = 0)
        {
            _periodTrackerService.UpdatePeriodTracker(DateTimeOffset.MinValue, 0, 0, 0);

            var existingNotes = _periodTrackerService.GetNotes();
            foreach (var noteId in existingNotes.Keys.ToList())
            {
                _periodTrackerService.DeleteNote(noteId);
            }

            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        /* Standard Sub-Actions used by the view forms */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateNote([FromForm] string noteBody)
        {
            _periodTrackerService.AddNote(noteBody ?? "New Entry");
            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditNote([FromForm] int noteId, [FromForm] string noteBody, [FromForm] bool isDone = false)
        {
            _periodTrackerService.UpdateNote(noteId, noteBody ?? string.Empty, isDone);
            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveNote([FromForm] int noteId)
        {
            _periodTrackerService.DeleteNote(noteId);
            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProductToBasket(int itemId, float discountPercentage)
        {
            _basketService.AddToBasket(itemId, 1, discountPercentage);
            BasketStore.Save(ServiceWrapper.UserAccountService.CurrentUser);
            return RedirectToAction(nameof(Index));
        }

        private static PeriodTrackerViewModel ToViewModel(PeriodTrackerState state)
        {
            return new PeriodTrackerViewModel
            {
                HasPeriodTracker = state.HasPeriodTracker,
                StartPeriodDate = DateOnly.FromDateTime(state.StartPeriodDate.DateTime),
                CycleDays = (int)state.CycleDays,
                PeriodLasts = (int)state.PeriodLasts,
                PMSOption = state.PremenstrualSyndromeOption,
            };
        }

        private static PeriodTrackerViewModel ToViewModel(PeriodTrackerDashboardSnapshot snapshot)
        {
            return new PeriodTrackerViewModel
            {
                HasPeriodTracker = snapshot.HasPeriodTracker,
                StartPeriodDate = DateOnly.FromDateTime(snapshot.StartPeriodDate),
                CycleDays = snapshot.CycleDays,
                PeriodLasts = snapshot.PeriodLasts,
                PMSOption = snapshot.PMSOption,
                MonthOffset = snapshot.MonthOffset,
                CurrentMonthName = snapshot.CurrentMonthName,
                PeriodIntervalText = snapshot.PeriodIntervalText,
                LowFertilityIntervalText = snapshot.LowFertilityIntervalText,
                OvulationIntervalText = snapshot.OvulationIntervalText,
                PmsIntervalText = snapshot.PmsIntervalText,
                CurrentPhaseString = snapshot.CurrentPhaseString,
                NextPeriodDateString = snapshot.NextPeriodDateString,
                NextPeriodDistanceString = snapshot.NextPeriodDistanceString,
                IsInMenstrualPhase = snapshot.IsInMenstrualPhase,
                CurrentDayOfCycle = snapshot.CurrentDayOfCycle,
                DaysUntilOvulation = snapshot.DaysUntilOvulation,
                OvulationDistanceString = snapshot.OvulationDistanceString,
                Notes = snapshot.Notes.Select(note => new WebNoteItemViewModel
                {
                    NoteId = note.NoteId,
                    NoteBody = note.NoteBody,
                    IsDone = note.IsDone,
                }).ToList(),
                ShopItems = snapshot.ShopItems.Select(item => new WebShopItemViewModel
                {
                    RawItem = item.RawItem,
                    DisplayPrice = item.DisplayPrice,
                    HasDiscountApplied = item.HasDiscountApplied,
                }).ToList(),
            };
        }
    }
}
