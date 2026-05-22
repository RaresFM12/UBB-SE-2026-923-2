namespace UBB_SE_2026_923_2.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Web.ViewModels;

    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private const string BatchDateFormat = "yyyy-MM-dd";
        private const string SubstanceLineDelimiter = ":";
        private static readonly string[] LineDelimiters = { "\r\n", "\n" };

        private readonly IAdminService adminService;

        public AdminController(IAdminService adminService)
        {
            this.adminService = adminService;
        }

        // ---- Items ----

        public IActionResult Index(string searchQuery = "", bool showExpiredOnly = false)
        {
            List<Item> items = this.LoadItems(searchQuery, showExpiredOnly);

            var viewModel = new ItemIndexViewModel
            {
                Items = items,
                SearchQuery = searchQuery,
                ShowExpiredOnly = showExpiredOnly,
            };

            return this.View(viewModel);
        }

        public IActionResult Details(int id)
        {
            Item item = this.adminService.GetItemById(id);
            if (item == null)
            {
                return this.NotFound();
            }

            return this.View(item);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return this.View(new ItemViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ItemViewModel viewModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(viewModel);
            }

            Dictionary<string, float> activeSubstances = ParseSubstancesText(viewModel.SubstancesText);
            Dictionary<DateOnly, int> batches = ParseBatchesText(viewModel.BatchesText);

            var newItem = new Item(
                viewModel.Name,
                viewModel.Producer,
                viewModel.Category,
                viewModel.Price,
                viewModel.NumberOfPills,
                activeSubstances,
                batches,
                viewModel.Quantity,
                viewModel.Label,
                viewModel.Description,
                viewModel.ImagePath,
                viewModel.DiscountPercentage);

            try
            {
                this.adminService.AddItem(newItem);
                return this.RedirectToAction(nameof(this.Index));
            }
            catch (ArgumentException argumentException)
            {
                this.ModelState.AddModelError(string.Empty, argumentException.Message);
                return this.View(viewModel);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Item item = this.adminService.GetItemById(id);
            if (item == null)
            {
                return this.NotFound();
            }

            ItemViewModel viewModel = MapItemToViewModel(item);
            return this.View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ItemViewModel viewModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(viewModel);
            }

            Dictionary<string, float> activeSubstances = ParseSubstancesText(viewModel.SubstancesText);
            Dictionary<DateOnly, int> batches = ParseBatchesText(viewModel.BatchesText);

            var updatedItem = new Item(
                id,
                viewModel.Name,
                viewModel.Producer,
                viewModel.Category,
                viewModel.Price,
                viewModel.NumberOfPills,
                viewModel.Label,
                viewModel.Description,
                viewModel.ImagePath,
                viewModel.DiscountPercentage,
                viewModel.Quantity)
            {
                ActiveSubstances = activeSubstances,
                Batches = batches,
            };

            try
            {
                this.adminService.UpdateItemById(id, updatedItem);
                return this.RedirectToAction(nameof(this.Index));
            }
            catch (ArgumentException argumentException)
            {
                this.ModelState.AddModelError(string.Empty, argumentException.Message);
                return this.View(viewModel);
            }
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            Item item = this.adminService.GetItemById(id);
            if (item == null)
            {
                return this.NotFound();
            }

            return this.View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            this.adminService.RemoveItemById(id);
            return this.RedirectToAction(nameof(this.Index));
        }

        // ---- Substances ----

        [HttpGet]
        public IActionResult Substances()
        {
            List<Substance> substances = this.adminService.GetAllSubstances();
            return this.View(substances);
        }

        [HttpGet]
        public IActionResult CreateSubstance()
        {
            return this.View(new SubstanceViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSubstance(SubstanceViewModel viewModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(viewModel);
            }

            var newSubstance = new Substance(viewModel.Name, viewModel.LethalDose, viewModel.Description);

            try
            {
                this.adminService.AddSubstance(newSubstance);
                return this.RedirectToAction(nameof(this.Substances));
            }
            catch (ArgumentException argumentException)
            {
                this.ModelState.AddModelError(string.Empty, argumentException.Message);
                return this.View(viewModel);
            }
        }

        [HttpGet]
        public IActionResult EditSubstance(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return this.NotFound();
            }

            Substance substance = this.adminService.GetSubstanceByName(name);
            if (substance == null)
            {
                return this.NotFound();
            }

            var viewModel = new SubstanceViewModel
            {
                Name = substance.Name,
                LethalDose = substance.LethalDose,
                Description = substance.Description,
            };

            return this.View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSubstance(string name, SubstanceViewModel viewModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(viewModel);
            }

            var updatedSubstance = new Substance(viewModel.Name, viewModel.LethalDose, viewModel.Description);

            try
            {
                this.adminService.UpdateSubstanceByName(name, updatedSubstance);
                return this.RedirectToAction(nameof(this.Substances));
            }
            catch (ArgumentException argumentException)
            {
                this.ModelState.AddModelError(string.Empty, argumentException.Message);
                return this.View(viewModel);
            }
        }

        [HttpGet]
        public IActionResult DeleteSubstance(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return this.NotFound();
            }

            Substance substance = this.adminService.GetSubstanceByName(name);
            if (substance == null)
            {
                return this.NotFound();
            }

            return this.View(substance);
        }

        [HttpPost, ActionName("DeleteSubstance")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSubstanceConfirmed(string name)
        {
            Substance substance = this.adminService.GetSubstanceByName(name);
            if (substance != null)
            {
                this.adminService.RemoveSubstanceByName(substance);
            }

            return this.RedirectToAction(nameof(this.Substances));
        }

        // ---- Statistics ----

        [HttpGet]
        public IActionResult Statistics()
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

            var statisticsViewModel = new AdminStatisticsViewModel
            {
                TopItems = rawTopItems.ConvertAll(MapTupleToTopItemViewModel),
                TopSubstances = topSubstances,
            };

            return this.View(statisticsViewModel);
        }

        // ---- Private helpers ----

        private List<Item> LoadItems(string searchQuery, bool showExpiredOnly)
        {
            if (showExpiredOnly)
            {
                return this.adminService.GetExpiredItems();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                return this.adminService.SearchItemsByName(searchQuery);
            }

            return this.adminService.GetAllItems();
        }

        private static ItemViewModel MapItemToViewModel(Item item)
        {
            string FormatSubstanceEntry(KeyValuePair<string, float> substanceEntry) =>
                $"{substanceEntry.Key}{SubstanceLineDelimiter}{substanceEntry.Value.ToString(CultureInfo.InvariantCulture)}";

            string FormatBatchEntry(KeyValuePair<DateOnly, int> batchEntry) =>
                $"{batchEntry.Key.ToString(BatchDateFormat, CultureInfo.InvariantCulture)}{SubstanceLineDelimiter}{batchEntry.Value}";

            return new ItemViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Producer = item.Producer,
                Price = item.Price,
                Category = item.Category,
                ImagePath = item.ImagePath,
                NumberOfPills = item.NumberOfPills,
                Quantity = item.Quantity,
                Label = item.Label,
                Description = item.Description,
                DiscountPercentage = item.DiscountPercentage,
                SubstancesText = string.Join(Environment.NewLine, item.ActiveSubstances.Select(FormatSubstanceEntry)),
                BatchesText = string.Join(Environment.NewLine, item.Batches.Select(FormatBatchEntry)),
            };
        }

        private static Dictionary<string, float> ParseSubstancesText(string substancesText)
        {
            var activeSubstances = new Dictionary<string, float>();

            if (string.IsNullOrWhiteSpace(substancesText))
            {
                return activeSubstances;
            }

            foreach (string line in substancesText.Split(LineDelimiters, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = line.Split(SubstanceLineDelimiter, 2);
                if (parts.Length != 2)
                {
                    continue;
                }

                string substanceName = parts[0].Trim();
                if (float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float concentration))
                {
                    activeSubstances[substanceName] = concentration;
                }
            }

            return activeSubstances;
        }

        private static Dictionary<DateOnly, int> ParseBatchesText(string batchesText)
        {
            var batches = new Dictionary<DateOnly, int>();

            if (string.IsNullOrWhiteSpace(batchesText))
            {
                return batches;
            }

            foreach (string line in batchesText.Split(LineDelimiters, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = line.Split(SubstanceLineDelimiter, 2);
                if (parts.Length != 2)
                {
                    continue;
                }

                bool dateIsValid = DateOnly.TryParseExact(
                    parts[0].Trim(),
                    BatchDateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateOnly expirationDate);

                bool quantityIsValid = int.TryParse(parts[1].Trim(), out int numberOfPacks);

                if (dateIsValid && quantityIsValid)
                {
                    batches[expirationDate] = numberOfPacks;
                }
            }

            return batches;
        }
    }
}
