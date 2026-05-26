using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Repositories;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.Web.ViewModels;

namespace UBB_SE_2026_923_2.Web.Controllers
{
    public class ProductCatalogueController : Controller
    {
        private readonly IProductCatalogueService _catalogueService;
        private readonly IOrderService _orderService;
        private readonly IUsersRepository _usersRepository;
        const int ItemsPerPage = 12;

        public ProductCatalogueController(
            IProductCatalogueService catalogueService,
            IOrderService orderService,
            IUsersRepository usersRepository)
        {
            _catalogueService = catalogueService;
            _orderService = orderService;
            _usersRepository = usersRepository;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index(
            string searchText,
            List<string> categories,
            List<string> priceRanges,
            string stockFilter = "all",
            string discountFilter = "all",
            string sortBy = "default",
            string sortDirection = "asc",
            int pageIndex = 0)
        {
            var parsedPriceRanges = new List<(float, float)>();
            if (priceRanges != null)
            {
                if (priceRanges.Contains("0-49")) parsedPriceRanges.Add((0f, 49.99f));
                if (priceRanges.Contains("50-99")) parsedPriceRanges.Add((50f, 99.99f));
                if (priceRanges.Contains("100-199")) parsedPriceRanges.Add((100f, 199.99f));
                if (priceRanges.Contains("200-499")) parsedPriceRanges.Add((200f, 499.99f));
                if (priceRanges.Contains("500+")) parsedPriceRanges.Add((500f, float.MaxValue));
            }

            string serviceStockFilter = stockFilter == "in_stock" ? ProductCatalogueService.StockFilterInStock :
                                        stockFilter == "low_stock" ? ProductCatalogueService.StockFilterLowStock : null;

            bool? serviceDiscountFilter = discountFilter == "yes" ? true :
                                          discountFilter == "no" ? false : (bool?)null;

            string serviceSortBy = sortBy == "price" ? ProductCatalogueService.SortByPrice :
                                   sortBy == "newest" ? ProductCatalogueService.SortByNewest : null;
            bool isAscending = sortDirection == "asc";

            int pageSize = ItemsPerPage; 

            var rawItems = _catalogueService.GetItems(
                searchText,
                categories?.Any() == true ? categories : null,
                parsedPriceRanges.Any() ? parsedPriceRanges : null,
                serviceStockFilter,
                serviceDiscountFilter,
                null,
                isAscending,
                pageIndex,
                pageSize,
                serviceSortBy
            );

            var peekNextPage = _catalogueService.GetItems(
                searchText, categories?.Any() == true ? categories : null, parsedPriceRanges.Any() ? parsedPriceRanges : null,
                serviceStockFilter, serviceDiscountFilter, null, isAscending, pageIndex + 1, pageSize, serviceSortBy);

            var viewModel = new CatalogueIndexViewModel
            {
                SearchText = searchText,
                SelectedCategories = categories ?? new List<string>(),
                SelectedPriceRanges = priceRanges ?? new List<string>(),
                StockFilter = stockFilter,
                DiscountFilter = discountFilter,
                SortBy = sortBy,
                SortDirection = sortDirection,
                CurrentPage = pageIndex,
                HasNextPage = peekNextPage.Any(),
                Products = rawItems.Select(MapToViewModel).ToList()
            };

            return View(viewModel);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Details(int id)
        {
            var item = _catalogueService.GetItems(null, null, null, null, null, null, true, 0, int.MaxValue)
                                        .FirstOrDefault(item => item.Id == id);

            if (item == null) return NotFound();

            var viewModel = MapToViewModel(item);

            if (item.Quantity == 0 && User.Identity?.IsAuthenticated == true)
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out int userId))
                {
                    var currentUser = _usersRepository.GetUserById(userId);
                    if (currentUser != null)
                    {
                        viewModel.ShowStockAlertButton = true;
                        viewModel.IsStockAlertActive = currentUser.StockAlerts.Contains(id);
                    }
                }
            }

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStockAlert(int id)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out int userId))
            {
                return this.Forbid();
            }

            var currentUser = _usersRepository.GetUserById(userId);
            if (currentUser == null)
            {
                return this.Forbid();
            }

            if (currentUser.StockAlerts.Contains(id))
            {
                currentUser.RemoveStockAlertFromUser(id);
            }
            else
            {
                currentUser.AddStockAlertToUser(id);
            }

            _usersRepository.UpdateUser(currentUser);

            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int itemId, int quantity)
        {
            var item = _catalogueService.GetItems(null, null, null, null, null, null, true, 0, int.MaxValue)
                                        .FirstOrDefault(itemNew => itemNew.Id == itemId);

            if (item == null || quantity <= 0 || quantity > 50 || quantity > item.Quantity)
            {
                TempData["ErrorMessage"] = "Invalid quantity selected.";
                return RedirectToAction(nameof(Details), new { id = itemId });
            }

            try
            {
                _orderService.AddToBasket(itemId, quantity);
                BasketStore.Save(_orderService.ActiveUser);
                TempData["SuccessMessage"] = "Item added to basket successfully!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Item already in basket or stock unavailable.";
            }

            return RedirectToAction(nameof(Details), new { id = itemId });
        }

        private CatalogueItemViewModel MapToViewModel(UBB_SE_2026_923_2.Models.Item item)
        {
            string cleanImage = item.ImagePath?.TrimStart('/') ?? "";
            if (cleanImage.StartsWith("ms-appx:///")) cleanImage = cleanImage.Replace("ms-appx:///", "");

            string substancesText = item.ActiveSubstances != null && item.ActiveSubstances.Any()
                ? string.Join(", ", item.ActiveSubstances.Select(substance => $"{substance.Key} ({substance.Value})"))
                : "None";

            return new CatalogueItemViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Producer = item.Producer,
                Category = item.Category,
                Label = item.Label,
                NumberOfPills = item.NumberOfPills,
                SubstancesText = substancesText,
                OldPrice = (float)item.Price,
                DiscountPercentage = item.DiscountPercentage,
                Quantity = item.Quantity,
                ImagePath = cleanImage,
                Description = item.Description
            };
        }

        // Placeholders
        [Authorize(Roles = "Admin")][HttpGet] public IActionResult Create() => View();
        [Authorize(Roles = "Admin")][HttpPost][ValidateAntiForgeryToken] public IActionResult Create(object model) => RedirectToAction(nameof(Index));
        [Authorize(Roles = "Admin")][HttpGet] public IActionResult Edit(int id) => View();
        [Authorize(Roles = "Admin")][HttpPost][ValidateAntiForgeryToken] public IActionResult Edit(int id, object model) => RedirectToAction(nameof(Index));
        [Authorize(Roles = "Admin")][HttpGet] public IActionResult Delete(int id) => View();
        [Authorize(Roles = "Admin")][HttpPost, ActionName("Delete")][ValidateAntiForgeryToken] public IActionResult DeleteConfirmed(int id) => RedirectToAction(nameof(Index));
    }
}
