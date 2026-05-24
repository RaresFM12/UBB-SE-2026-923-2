namespace UBB_SE_2026_923_2.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Web.ViewModels;
    using BasketItemViewModel = UBB_SE_2026_923_2.ViewModels.Orders.BasketItemViewModel;

    [Authorize(Roles = "Client,Admin")]
    public class BasketController : Controller
    {
        private readonly IOrderService orderService;

        public BasketController(IOrderService orderService)
        {
            this.orderService = orderService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return this.View(this.BuildViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(BasketAddItemViewModel viewModel)
        {
            if (!this.ModelState.IsValid)
            {
                this.TempData["ErrorMessage"] = "Choose a valid quantity.";
                return this.RedirectToBasketOrSource(viewModel);
            }

            try
            {
                this.orderService.AddItemToBasket(viewModel.ItemId, viewModel.Quantity, viewModel.ExtraDiscountPercentage);
                BasketStore.Save(this.orderService.ActiveUser);
                this.TempData["SuccessMessage"] = "Item added to basket.";
            }
            catch (ArgumentException exception)
            {
                this.TempData["ErrorMessage"] = exception.Message;
            }

            return this.RedirectToBasketOrSource(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(BasketQuantityViewModel viewModel)
        {
            if (!this.ModelState.IsValid)
            {
                this.TempData["ErrorMessage"] = "Choose a valid quantity.";
                return this.RedirectToAction(nameof(this.Index));
            }

            try
            {
                this.orderService.UpdateBasketItemQuantity(viewModel.ItemId, viewModel.Quantity);
                BasketStore.Save(this.orderService.ActiveUser);
                this.TempData["SuccessMessage"] = "Basket updated.";
            }
            catch (ArgumentException exception)
            {
                this.TempData["ErrorMessage"] = exception.Message;
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int itemId)
        {
            try
            {
                this.orderService.RemoveFromBasket(itemId);
                BasketStore.Save(this.orderService.ActiveUser);
                this.TempData["SuccessMessage"] = "Item removed from basket.";
            }
            catch (ArgumentException exception)
            {
                this.TempData["ErrorMessage"] = exception.Message;
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApplyPrescription(BasketViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.PrescriptionId))
            {
                this.TempData["ErrorMessage"] = "Enter a prescription ID.";
                return this.RedirectToAction(nameof(this.Index));
            }

            try
            {
                this.orderService.ApplyPrescriptionToBasket(viewModel.PrescriptionId);
                BasketStore.Save(this.orderService.ActiveUser);
                this.TempData["SuccessMessage"] = "Prescription items added to basket.";
            }
            catch (ArgumentException exception)
            {
                this.TempData["ErrorMessage"] = exception.Message;
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        private BasketViewModel BuildViewModel()
        {
            List<BasketItemViewModel> basketItems = this.orderService.GetBasketItems();
            Tuple<float, float> totals = this.orderService.CalculateBasketTotalSum(basketItems);

            return new BasketViewModel
            {
                Items = basketItems,
                TotalBeforeDiscount = totals.Item1,
                TotalAfterDiscount = totals.Item2,
                SuccessMessage = this.ReadTempData("SuccessMessage"),
                ErrorMessage = this.ReadTempData("ErrorMessage"),
            };
        }

        private IActionResult RedirectToBasketOrSource(BasketAddItemViewModel viewModel)
        {
            if (!string.IsNullOrWhiteSpace(viewModel.ReturnController) &&
                !string.IsNullOrWhiteSpace(viewModel.ReturnAction))
            {
                return this.RedirectToAction(viewModel.ReturnAction, viewModel.ReturnController);
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        private string? ReadTempData(string key)
        {
            return this.TempData.TryGetValue(key, out object? value) ? value?.ToString() : null;
        }
    }
}
