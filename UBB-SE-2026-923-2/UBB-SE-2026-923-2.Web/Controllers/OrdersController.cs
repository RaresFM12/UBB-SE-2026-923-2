namespace UBB_SE_2026_923_2.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Web.ViewModels;
    using BasketItemViewModel = UBB_SE_2026_923_2.ViewModels.Orders.BasketItemViewModel;

    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderService orderService;
        private readonly IUserAccountService userAccountService;

        public OrdersController(IOrderService orderService, IUserAccountService userAccountService)
        {
            this.orderService = orderService;
            this.userAccountService = userAccountService;
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpGet]
        public IActionResult Index(bool showExpiredOnly = false)
        {
            User? currentUser = this.LoadCurrentUser();
            if (currentUser == null)
            {
                return this.Forbid();
            }

            this.orderService.ExpireOverdueOrders();
            List<OrderListItemViewModel> orders = this.orderService.OrdersRepository.GetOrdersOfClient(currentUser.Id)
                .Where(order => !showExpiredOnly || order.IsExpired)
                .OrderByDescending(order => order.PickUpDate)
                .ThenByDescending(order => order.Id)
                .Select(this.MapOrderListItem)
                .ToList();

            var viewModel = new OrdersIndexViewModel
            {
                Orders = orders,
                ShowExpiredOnly = showExpiredOnly,
                SuccessMessage = this.ReadTempData("SuccessMessage"),
                ErrorMessage = this.ReadTempData("ErrorMessage"),
            };

            return this.View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Manage(string userEmail = "", int? orderId = null, bool incompleteOnly = false, bool expiredOnly = false)
        {
            this.orderService.ExpireOverdueOrders();
            List<Order> orders = this.orderService.OrdersRepository.GetAllOrders();

            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                orders = orders
                    .Where(order => this.GetClientEmail(order).Contains(userEmail.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (orderId.HasValue)
            {
                orders = orders.Where(order => order.Id == orderId.Value).ToList();
            }

            if (incompleteOnly)
            {
                orders = orders.Where(order => !order.IsCompleted && !order.IsExpired).ToList();
            }

            if (expiredOnly)
            {
                orders = orders.Where(order => order.IsExpired).ToList();
            }

            var viewModel = new OrderManagementViewModel
            {
                Orders = orders.OrderByDescending(order => order.Id).Select(this.MapOrderListItem).ToList(),
                UserEmail = userEmail,
                OrderId = orderId,
                IncompleteOnly = incompleteOnly,
                ExpiredOnly = expiredOnly,
                SuccessMessage = this.ReadTempData("SuccessMessage"),
                ErrorMessage = this.ReadTempData("ErrorMessage"),
            };

            return this.View(viewModel);
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpGet]
        public IActionResult Details(int id, bool adminView = false)
        {
            Order? order = this.FindOrderForCurrentUser(id, allowAdmin: true);
            if (order == null)
            {
                return this.NotFound();
            }

            OrderDetailsViewModel viewModel = this.MapDetails(order, adminView);
            viewModel.SuccessMessage = this.ReadTempData("SuccessMessage");
            viewModel.ErrorMessage = this.ReadTempData("ErrorMessage");
            return this.View(viewModel);
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return this.View(this.BuildCheckoutViewModel());
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(OrderCheckoutViewModel viewModel)
        {
            if (viewModel.PickUpDate <= DateOnly.FromDateTime(DateTime.Today))
            {
                this.ModelState.AddModelError(string.Empty, "The pick-up date must be at least one day after today.");
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(this.BuildCheckoutViewModel(viewModel.PickUpDate));
            }

            try
            {
                this.orderService.PlaceOrderFromBasket(viewModel.PickUpDate);
                BasketStore.Clear(this.orderService.ActiveUser);
                this.TempData["SuccessMessage"] = "Order placed successfully.";
                return this.RedirectToAction(nameof(this.Index));
            }
            catch (ArgumentException exception)
            {
                this.ModelState.AddModelError(string.Empty, exception.Message);
                return this.View(this.BuildCheckoutViewModel(viewModel.PickUpDate));
            }
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Order? order = this.FindOrderForCurrentUser(id, allowAdmin: true);
            if (order == null)
            {
                return this.NotFound();
            }

            if (!CanEdit(order))
            {
                this.TempData["ErrorMessage"] = "Only incomplete, active orders can be modified.";
                return this.RedirectToAction(nameof(this.Details), new { id });
            }

            return this.View(this.MapEdit(order, this.User.IsInRole("Admin")));
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(OrderEditViewModel viewModel)
        {
            Order? order = this.FindOrderForCurrentUser(viewModel.Id, allowAdmin: true);
            if (order == null)
            {
                return this.NotFound();
            }

            if (!CanEdit(order))
            {
                return this.Forbid();
            }

            Dictionary<int, Tuple<int, float>> updatedItems = BuildUpdatedItems(viewModel.Items);
            if (updatedItems.Count == 0)
            {
                this.ModelState.AddModelError(string.Empty, "The order must contain at least one item.");
            }

            if (!this.ModelState.IsValid)
            {
                viewModel.AdminView = this.User.IsInRole("Admin");
                viewModel.Total = viewModel.Items.Sum(item => item.FinalPrice);
                return this.View(viewModel);
            }

            try
            {
                this.orderService.ModifyIncompleteOrder(viewModel.Id, updatedItems, viewModel.PickUpDate);
                this.TempData["SuccessMessage"] = "Order changes saved.";
                return this.RedirectToAction(nameof(this.Details), new { id = viewModel.Id, adminView = this.User.IsInRole("Admin") });
            }
            catch (ArgumentException exception)
            {
                this.ModelState.AddModelError(string.Empty, exception.Message);
                viewModel.AdminView = this.User.IsInRole("Admin");
                viewModel.Total = viewModel.Items.Sum(item => item.FinalPrice);
                return this.View(viewModel);
            }
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            Order? order = this.FindOrderForCurrentUser(id, allowAdmin: true);
            if (order == null)
            {
                return this.NotFound();
            }

            return this.View(this.MapDetails(order, adminView: this.User.IsInRole("Admin")));
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            Order? order = this.FindOrderForCurrentUser(id, allowAdmin: true);
            if (order == null)
            {
                return this.NotFound();
            }

            if (order.IsCompleted)
            {
                this.TempData["ErrorMessage"] = "Completed orders cannot be cancelled.";
                return this.RedirectToAction(nameof(this.Details), new { id });
            }

            this.orderService.CancelOrder(id);
            this.TempData["SuccessMessage"] = "Order cancelled.";
            return this.User.IsInRole("Admin") ? this.RedirectToAction(nameof(this.Manage)) : this.RedirectToAction(nameof(this.Index));
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpGet]
        public IActionResult Resubmit(int id)
        {
            Order? order = this.FindOrderForCurrentUser(id, allowAdmin: true);
            if (order == null)
            {
                return this.NotFound();
            }

            if (!order.IsExpired)
            {
                this.TempData["ErrorMessage"] = "Only expired orders can be resubmitted.";
                return this.RedirectToAction(nameof(this.Details), new { id });
            }

            return this.View(this.MapResubmit(order));
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Resubmit(OrderResubmitViewModel viewModel)
        {
            if (viewModel.PickUpDate <= DateOnly.FromDateTime(DateTime.Today))
            {
                this.ModelState.AddModelError(string.Empty, "The pick-up date must be at least one day after today.");
            }

            Order? order = this.FindOrderForCurrentUser(viewModel.Id, allowAdmin: true);
            if (order == null)
            {
                return this.NotFound();
            }

            if (!this.ModelState.IsValid)
            {
                OrderResubmitViewModel hydratedViewModel = this.MapResubmit(order);
                hydratedViewModel.PickUpDate = viewModel.PickUpDate;
                return this.View(hydratedViewModel);
            }

            try
            {
                this.orderService.ResubmitExpiredOrder(viewModel.Id, viewModel.PickUpDate);
                this.TempData["SuccessMessage"] = "Order resubmitted successfully.";
                return this.RedirectToAction(nameof(this.Index));
            }
            catch (ArgumentException exception)
            {
                this.ModelState.AddModelError(string.Empty, exception.Message);
                OrderResubmitViewModel hydratedViewModel = this.MapResubmit(order);
                hydratedViewModel.PickUpDate = viewModel.PickUpDate;
                return this.View(hydratedViewModel);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Complete(OrderEditViewModel viewModel)
        {
            Dictionary<int, Tuple<int, float>> updatedItems = BuildUpdatedItems(viewModel.Items);
            if (updatedItems.Count == 0)
            {
                this.TempData["ErrorMessage"] = "The order must contain at least one item.";
                return this.RedirectToAction(nameof(this.Details), new { id = viewModel.Id, adminView = true });
            }

            try
            {
                this.orderService.CompleteOrder(viewModel.Id, updatedItems);
                this.TempData["SuccessMessage"] = "Order completed and inventory updated.";
            }
            catch (ArgumentException exception)
            {
                this.TempData["ErrorMessage"] = exception.Message;
            }

            return this.RedirectToAction(nameof(this.Details), new { id = viewModel.Id, adminView = true });
        }

        private User? LoadCurrentUser()
        {
            string? idValue = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idValue, out int userId))
            {
                return null;
            }

            return this.userAccountService.LoadCurrentUser(userId);
        }

        private Order? FindOrderForCurrentUser(int id, bool allowAdmin)
        {
            Order? order = this.orderService.OrdersRepository.GetOrder(id);
            if (order == null)
            {
                return null;
            }

            if (allowAdmin && this.User.IsInRole("Admin"))
            {
                return order;
            }

            User? currentUser = this.LoadCurrentUser();
            if (currentUser == null)
            {
                return null;
            }

            bool orderBelongsToCurrentUser = this.orderService.OrdersRepository
                .GetOrdersOfClient(currentUser.Id)
                .Any(clientOrder => clientOrder.Id == id);

            return orderBelongsToCurrentUser ? order : null;
        }

        private OrderCheckoutViewModel BuildCheckoutViewModel(DateOnly? pickUpDate = null)
        {
            List<BasketItemViewModel> basketItems = this.orderService.GetBasketItems();
            Tuple<float, float> totals = this.orderService.CalculateBasketTotalSum(basketItems);

            return new OrderCheckoutViewModel
            {
                PickUpDate = pickUpDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                Items = basketItems,
                TotalBeforeDiscount = totals.Item1,
                TotalAfterDiscount = totals.Item2,
            };
        }

        private OrderDetailsViewModel MapDetails(Order order, bool adminView)
        {
            List<OrderLineItemViewModel> items = this.MapLineItems(order);
            return new OrderDetailsViewModel
            {
                Id = order.Id,
                UserEmail = this.GetClientEmail(order),
                PickUpDate = order.PickUpDate,
                ExpirationDate = order.PickUpDate.AddDays(Order.OrderExpirationDays),
                IsCompleted = order.IsCompleted,
                IsExpired = order.IsExpired,
                AdminView = adminView,
                Items = items,
                Total = items.Sum(item => item.FinalPrice),
            };
        }

        private OrderEditViewModel MapEdit(Order order, bool adminView)
        {
            List<OrderLineItemViewModel> items = this.MapLineItems(order);
            return new OrderEditViewModel
            {
                Id = order.Id,
                PickUpDate = order.PickUpDate,
                AdminView = adminView,
                Items = items,
                Total = items.Sum(item => item.FinalPrice),
            };
        }

        private OrderResubmitViewModel MapResubmit(Order order)
        {
            List<OrderLineItemViewModel> items = this.MapLineItems(order);
            return new OrderResubmitViewModel
            {
                Id = order.Id,
                PickUpDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                Items = items,
                Total = items.Sum(item => item.FinalPrice),
            };
        }

        private OrderListItemViewModel MapOrderListItem(Order order)
        {
            return new OrderListItemViewModel
            {
                Id = order.Id,
                UserEmail = this.GetClientEmail(order),
                PickUpDate = order.PickUpDate,
                ExpirationDate = order.PickUpDate.AddDays(Order.OrderExpirationDays),
                IsCompleted = order.IsCompleted,
                IsExpired = order.IsExpired,
            };
        }

        private List<OrderLineItemViewModel> MapLineItems(Order order)
        {
            List<OrderLineItemViewModel> lineItems = new();
            foreach (KeyValuePair<int, Tuple<int, float>> entry in order.ItemQuantitiesWithFinalPrice)
            {
                Item? item = this.orderService.ItemsRepository.GetItemById(entry.Key);
                lineItems.Add(new OrderLineItemViewModel
                {
                    ItemId = item?.Id ?? entry.Key,
                    Name = item?.Name ?? $"Deleted item #{entry.Key}",
                    Producer = item?.Producer ?? "Unavailable",
                    Quantity = entry.Value.Item1,
                    FinalPrice = entry.Value.Item2,
                });
            }

            return lineItems;
        }

        private int? GetClientId(Order order)
        {
            if (order.Client != null)
            {
                return order.Client.Id;
            }

            if (order.ClientId > 0)
            {
                return order.ClientId;
            }

            foreach (User user in this.orderService.UsersRepository.GetAllUsers())
            {
                bool orderBelongsToUser = this.orderService.OrdersRepository
                    .GetOrdersOfClient(user.Id)
                    .Any(clientOrder => clientOrder.Id == order.Id);

                if (orderBelongsToUser)
                {
                    return user.Id;
                }
            }

            return null;
        }

        private string GetClientEmail(Order order)
        {
            int? clientId = this.GetClientId(order);
            if (!clientId.HasValue)
            {
                return "Unknown";
            }

            return this.orderService.UsersRepository.GetUserById(clientId.Value)?.Email ?? "Unknown";
        }

        private static bool CanEdit(Order order)
        {
            return !order.IsCompleted && !order.IsExpired;
        }

        private static Dictionary<int, Tuple<int, float>> BuildUpdatedItems(IEnumerable<OrderLineItemViewModel> items)
        {
            Dictionary<int, Tuple<int, float>> updatedItems = new();
            foreach (OrderLineItemViewModel item in items)
            {
                if (item.Quantity > 0)
                {
                    updatedItems[item.ItemId] = new Tuple<int, float>(item.Quantity, item.FinalPrice);
                }
            }

            return updatedItems;
        }

        private string? ReadTempData(string key)
        {
            return this.TempData.TryGetValue(key, out object? value) ? value?.ToString() : null;
        }
    }
}
