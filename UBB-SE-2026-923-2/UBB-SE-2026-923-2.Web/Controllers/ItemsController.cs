namespace UBB_SE_2026_923_2.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Web.ViewModels;

    [Authorize(Roles = "Admin")]
    public class ItemsController : Controller
    {
        private const string BatchDateFormat = "yyyy-MM-dd";
        private const string SubstanceLineDelimiter = ":";
        private static readonly string[] LineDelimiters = { "\r\n", "\n" };

        private readonly IAdminService adminService;

        public ItemsController(IAdminService adminService)
        {
            this.adminService = adminService;
        }

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
            int quantity = batches.Values.Sum();

            var newItem = new Item(
                viewModel.Name,
                viewModel.Producer,
                viewModel.Category,
                viewModel.Price,
                viewModel.NumberOfPills,
                activeSubstances,
                batches,
                quantity,
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
            int quantity = batches.Values.Sum();

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
                quantity)
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
            const int ExpectedPartCount = 2;
            var activeSubstances = new Dictionary<string, float>();

            if (string.IsNullOrWhiteSpace(substancesText))
            {
                return activeSubstances;
            }

            foreach (string line in substancesText.Split(LineDelimiters, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = line.Split(SubstanceLineDelimiter, ExpectedPartCount);
                if (parts.Length != ExpectedPartCount)
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
            const int ExpectedPartCount = 2;
            var batches = new Dictionary<DateOnly, int>();

            if (string.IsNullOrWhiteSpace(batchesText))
            {
                return batches;
            }

            foreach (string line in batchesText.Split(LineDelimiters, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = line.Split(SubstanceLineDelimiter, ExpectedPartCount);
                if (parts.Length != ExpectedPartCount)
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
