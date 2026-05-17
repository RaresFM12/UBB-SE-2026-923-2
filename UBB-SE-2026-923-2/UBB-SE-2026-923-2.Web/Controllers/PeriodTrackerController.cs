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

        [HttpGet]
        public IActionResult Index(int monthOffset = 0)
        {
            var state = _periodTrackerService.GetTrackerState();
            var notesDict = _periodTrackerService.GetNotes();

            var viewModel = new PeriodTrackerViewModel
            {
                HasPeriodTracker = state.HasPeriodTracker,
                StartPeriodDate = state.StartPeriodDate.DateTime,
                CycleDays = state.CycleDays,
                PeriodLasts = state.PeriodLasts,
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
                // Run the analytical calendar engine mirroring your desktop project formulas
                RunCalendarCalculations(viewModel);
                PopulateRecommendedProducts(viewModel);
            }

            return View(viewModel);
        }

        private void RunCalendarCalculations(PeriodTrackerViewModel vm)
        {
            DateTime today = DateTime.Today;
            DateTime computedStart = vm.StartPeriodDate.Date;

            // Fast-forward or reverse logic tracking the exact actual period occurrence
            while (computedStart.AddDays(vm.CycleDays) <= today)
            {
                computedStart = computedStart.AddDays(vm.CycleDays);
            }
            while (computedStart > today)
            {
                computedStart = computedStart.AddDays(-vm.CycleDays);
            }

            // Apply manual Month Offset variations via user navigation button interactions
            computedStart = computedStart.AddDays(vm.MonthOffset * vm.CycleDays);

            DateTime endPeriod = computedStart.AddDays(vm.PeriodLasts);
            DateTime startLowFertility = endPeriod.AddDays(1);
            DateTime endLowFertility = computedStart.AddDays(8);
            DateTime startOvulation = computedStart.AddDays(11);
            DateTime endOvulation = computedStart.AddDays(15);
            DateTime nextPeriod = computedStart.AddDays(vm.CycleDays);

            // Seed internal statistics display components
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

            // --- 3 STATS CALCULATIONS ---
            // 1. In which day of the cycle you are right now
            vm.CurrentDayOfCycle = (int)(today - computedStart).TotalDays + 1;

            // 2. Days left until the next period
            double daysLeftPeriod = Math.Max(0, Math.Ceiling((nextPeriod - today).TotalDays));
            vm.NextPeriodDistanceString = $"{daysLeftPeriod} days left";

            // 3. Days left until ovulation
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
                // If passed, calculate until next month's ovulation window
                DateTime nextMonthOvulation = nextPeriod.AddDays(11);
                vm.DaysUntilOvulation = (int)Math.Ceiling((nextMonthOvulation - today).TotalDays);
                vm.OvulationDistanceString = $"In {vm.DaysUntilOvulation} days";
            }
        }

        private void PopulateRecommendedProducts(PeriodTrackerViewModel vm)
        {
            var wellnessItems = _wellnessItemsService.GetWellnessItems();
            float discountModifier = vm.IsInMenstrualPhase ? 20.0f : 0.0f; // Menstrual phase extra 20% discount trigger

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

        [HttpPost]
        public IActionResult Calculate(DateTime startPeriodDate, int cycleDays, int periodLasts, int pmsOption)
        {
            // Business Rule Validation limits mapped natively
            if (periodLasts < 1 || periodLasts > 9 || cycleDays < 20 || cycleDays > 45)
            {
                return RedirectToAction(nameof(Index));
            }

            _periodTrackerService.UpdatePeriodTracker(startPeriodDate, cycleDays, periodLasts, pmsOption);
            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult CreateNote(string noteBody)
        {
            var notes = _periodTrackerService.GetNotes();
            if (notes.Count < 4)
            {
                // Accept empty entries or dynamic texts cleanly
                _periodTrackerService.AddNote(noteBody ?? "New Health Entry");
                _periodTrackerService.SaveCurrentUser();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult EditNote(int noteId, string noteBody, bool isDone)
        {
            _periodTrackerService.UpdateNote(noteId, noteBody ?? string.Empty, isDone);
            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult RemoveNote(int noteId)
        {
            _periodTrackerService.DeleteNote(noteId);
            _periodTrackerService.SaveCurrentUser();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult AddProductToBasket(int itemId, float discountPercentage)
        {
            // Connect directly to your interface implementation
            _basketService.AddToBasket(itemId, 1, discountPercentage);
            return RedirectToAction(nameof(Index));
        }
    }
}