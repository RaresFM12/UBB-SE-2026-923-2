using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.Web.Models;

namespace UBB_SE_2026_923_2.Web.Controllers
{
    [Authorize(Roles = "Client")]
    public class PeriodTrackerController : Controller
    {
        private readonly IPeriodTrackerService _periodTrackerService;
        private readonly IWellnessItemsService _wellnessItemsService;
        private readonly IBasketService _basketService;

        public PeriodTrackerController(
            IPeriodTrackerService periodTrackerService,
            IWellnessItemsService wellnessItemsService,
            IBasketService basketService)
        {
            _periodTrackerService = periodTrackerService;
            _wellnessItemsService = wellnessItemsService;
            _basketService = basketService;
        }

        // GET: PeriodTracker
        [HttpGet]
        public IActionResult Index(int monthOffset = 0)
        {
            var state = _periodTrackerService.GetTrackerState();
            var notesDict = _periodTrackerService.GetNotes();

            var viewModel = new PeriodTrackerViewModel
            {
                HasPeriodTracker = state.HasPeriodTracker,
                StartPeriodDate = state.StartPeriodDate.DateTime,
                CycleDays = (int)state.CycleDays,
                PeriodLasts = (int)state.PeriodLasts,
                PMSOption = state.PremenstrualSyndromeOption,
                MonthOffset = monthOffset,
                Notes = notesDict.OrderBy(n => n.Key).Select(n => new WebNoteItemViewModel
                {
                    NoteId = n.Key,
                    NoteBody = n.Value.Item1,
                    IsDone = n.Value.Item2
                }).ToList()
            };

            if (viewModel.HasPeriodTracker && viewModel.CycleDays > 0)
            {
                RunCalendarCalculations(viewModel);
                PopulateRecommendedProducts(viewModel);
            }

            return View(viewModel);
        }

        // GET: PeriodTracker/Details
        [HttpGet]
        public IActionResult Details()
        {
            var state = _periodTrackerService.GetTrackerState();
            if (!state.HasPeriodTracker) return RedirectToAction(nameof(Create));

            var viewModel = new PeriodTrackerViewModel
            {
                HasPeriodTracker = state.HasPeriodTracker,
                StartPeriodDate = state.StartPeriodDate.DateTime,
                CycleDays = (int)state.CycleDays,
                PeriodLasts = (int)state.PeriodLasts,
                PMSOption = state.PremenstrualSyndromeOption
            };
            RunCalendarCalculations(viewModel);
            return View(viewModel);
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
        public IActionResult Create(PeriodTrackerViewModel model)
        {
            _periodTrackerService.UpdatePeriodTracker(model.StartPeriodDate, model.CycleDays, model.PeriodLasts, model.PMSOption);
            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        // GET: PeriodTracker/Edit
        [HttpGet]
        public IActionResult Edit()
        {
            var state = _periodTrackerService.GetTrackerState();
            if (!state.HasPeriodTracker) return RedirectToAction(nameof(Create));

            var viewModel = new PeriodTrackerViewModel
            {
                StartPeriodDate = state.StartPeriodDate.DateTime,
                CycleDays = (int)state.CycleDays,
                PeriodLasts = (int)state.PeriodLasts,
                PMSOption = state.PremenstrualSyndromeOption
            };
            return View(viewModel);
        }

        // POST: PeriodTracker/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PeriodTrackerViewModel model)
        {
            _periodTrackerService.UpdatePeriodTracker(model.StartPeriodDate, model.CycleDays, model.PeriodLasts, model.PMSOption);
            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        // GET: PeriodTracker/Delete
        [HttpGet]
        public IActionResult Delete()
        {
            var state = _periodTrackerService.GetTrackerState();
            if (!state.HasPeriodTracker) return RedirectToAction(nameof(Index));

            var viewModel = new PeriodTrackerViewModel
            {
                StartPeriodDate = state.StartPeriodDate.DateTime,
                CycleDays = (int)state.CycleDays
            };
            return View(viewModel);
        }

        // POST: PeriodTracker/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed()
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
        public IActionResult CreateNote(string noteBody)
        {
            _periodTrackerService.AddNote(noteBody ?? "New Entry");
            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditNote(int noteId, string noteBody, bool isDone)
        {
            _periodTrackerService.UpdateNote(noteId, noteBody ?? string.Empty, isDone);
            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveNote(int noteId)
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
            return RedirectToAction(nameof(Index));
        }

        private void RunCalendarCalculations(PeriodTrackerViewModel vm)
        {
            DateTime today = DateTime.Today;
            DateTime computedStart = vm.StartPeriodDate.Date;

            while (computedStart.AddDays(vm.CycleDays) <= today)
            {
                computedStart = computedStart.AddDays(vm.CycleDays);
            }
            while (computedStart > today)
            {
                computedStart = computedStart.AddDays(-vm.CycleDays);
            }

            computedStart = computedStart.AddDays(vm.MonthOffset * vm.CycleDays);

            DateTime endPeriod = computedStart.AddDays(vm.PeriodLasts);
            DateTime startLowFertility = endPeriod.AddDays(1);
            DateTime endLowFertility = computedStart.AddDays(8);
            DateTime startOvulation = computedStart.AddDays(11);
            DateTime endOvulation = computedStart.AddDays(15);
            DateTime nextPeriod = computedStart.AddDays(vm.CycleDays);

            vm.CurrentMonthName = computedStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
            vm.PeriodIntervalText = $"{computedStart.Day} {computedStart:MMMM} - {endPeriod.Day} {endPeriod:MMMM}";
            vm.LowFertilityIntervalText = vm.PeriodLasts < 8 ? $"{startLowFertility.Day} {startLowFertility:MMMM} - {endLowFertility.Day} {endLowFertility:MMMM}" : "No low fertility days this month";
            vm.OvulationIntervalText = $"{startOvulation.Day} {startOvulation:MMMM} - {endOvulation.Day} {endOvulation:MMMM}";

            if (vm.PMSOption > 0)
            {
                int pmsOffset = vm.PMSOption == 1 ? 3 : (vm.PMSOption == 2 ? 6 : 10);
                DateTime startPms = nextPeriod.AddDays(-pmsOffset);
                vm.PmsIntervalText = $"{startPms.Day} {startPms:MMMM} - {nextPeriod.Day} {nextPeriod:MMMM}";
            }
            else
            {
                vm.PmsIntervalText = "No registered PMS effects";
            }

            vm.IsInMenstrualPhase = today >= computedStart && today <= endPeriod;
            if (today >= computedStart && today <= endPeriod) vm.CurrentPhaseString = "Menstrual Phase";
            else if (today > endPeriod && today < startOvulation) vm.CurrentPhaseString = "Follicular Phase";
            else if (today >= startOvulation && today <= endOvulation) vm.CurrentPhaseString = "Ovulation Phase";
            else if (today > endOvulation && today < nextPeriod) vm.CurrentPhaseString = "Luteal Phase";
            else vm.CurrentPhaseString = "Out of Scope Cycle";

            vm.NextPeriodDateString = nextPeriod.ToString("d");
            vm.CurrentDayOfCycle = (int)(today - computedStart).TotalDays + 1;

            double daysLeftPeriod = Math.Max(0, Math.Ceiling((nextPeriod - today).TotalDays));
            vm.NextPeriodDistanceString = $"{daysLeftPeriod} days left";

            if (today < startOvulation)
            {
                vm.DaysUntilOvulation = (int)Math.Ceiling((startOvulation - today).TotalDays);
                vm.OvulationDistanceString = $"In {vm.DaysUntilOvulation} days";
            }
            else if (today >= startOvulation && today <= endOvulation)
            {
                vm.DaysUntilOvulation = 0;
                vm.OvulationDistanceString = "In Progress";
            }
            else
            {
                DateTime nextMonthOvulation = nextPeriod.AddDays(11);
                vm.DaysUntilOvulation = (int)Math.Ceiling((nextMonthOvulation - today).TotalDays);
                vm.OvulationDistanceString = $"In {vm.DaysUntilOvulation} days";
            }
        }

        private void PopulateRecommendedProducts(PeriodTrackerViewModel vm)
        {
            var wellnessItems = _wellnessItemsService.GetWellnessItems();
            float discountModifier = vm.IsInMenstrualPhase ? 20.0f : 0.0f;

            foreach (var item in wellnessItems)
            {
                float basePrice = (float)item.Price;
                float netPrice = discountModifier > 0 ? basePrice * 0.8f : basePrice;

                vm.ShopItems.Add(new WebShopItemViewModel
                {
                    RawItem = item,
                    DisplayPrice = netPrice,
                    HasDiscountApplied = vm.IsInMenstrualPhase
                });
            }
        }
    }
}