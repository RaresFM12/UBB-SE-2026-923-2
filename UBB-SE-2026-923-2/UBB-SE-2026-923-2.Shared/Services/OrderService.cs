namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.ViewModels.Orders;

    public class OrderService : IOrderService
    {
        private const float MinimumDiscount = 0f;
        private const float MaximumDiscount = 1f;
        private const float PercentageDivisor = 100f;
        private const decimal PriceRoundingFactor = 100m;
        private const int NotFoundIndex = -1;
        private const float NoExtraDiscount = 0f;
        private const int EmptyQuantity = 0;

        public ISubstancesRepository SubstancesRepository { get; private set; }

        public IItemsRepository ItemsRepository { get; private set; }

        public IUsersRepository UsersRepository { get; private set; }

        public IOrdersRepository OrdersRepository { get; private set; }

        public IEvaluationsRepository EvaluationsRepository { get; private set; }

        public IPrescriptionService PrescriptionService { get; private set; }

        private IBasketRepository? basketRepository;
        private readonly User injectedActiveUser;

        public User ActiveUser
        {
            get { return this.injectedActiveUser ?? ServiceWrapper.UserAccountService.CurrentUser; }
        }

        public OrderService()
        {
            // Parameterless overload kept for legacy call sites. Pull EF Core
            // repositories from the application service provider; the
            // evaluations repository still uses the legacy ADO.NET connection
            // string until it is migrated in Phase 2.
            this.SubstancesRepository = UBB_SE_2026_923_2.Shared.SharedServiceProvider.Services.GetRequiredService<ISubstancesRepository>();
            this.ItemsRepository = UBB_SE_2026_923_2.Shared.SharedServiceProvider.Services.GetRequiredService<IItemsRepository>();
            this.UsersRepository = UBB_SE_2026_923_2.Shared.SharedServiceProvider.Services.GetRequiredService<IUsersRepository>();
            this.OrdersRepository = UBB_SE_2026_923_2.Shared.SharedServiceProvider.Services.GetRequiredService<IOrdersRepository>();
            this.EvaluationsRepository = UBB_SE_2026_923_2.Shared.SharedServiceProvider.Services.GetRequiredService<IEvaluationsRepository>();
            this.basketRepository = UBB_SE_2026_923_2.Shared.SharedServiceProvider.Services.GetService<IBasketRepository>();
            this.PrescriptionService = new PrescriptionService(this.ItemsRepository, this.EvaluationsRepository);
        }

        public OrderService(
            ISubstancesRepository substancesRepository,
            IItemsRepository itemsRepository,
            IUsersRepository usersRepository,
            IOrdersRepository ordersRepository,
            User activeUser,
            IEvaluationsRepository? evaluationsRepository = null,
            IBasketRepository? basketRepository = null)
        {
            this.SubstancesRepository = substancesRepository;
            this.ItemsRepository = itemsRepository;
            this.UsersRepository = usersRepository;
            this.OrdersRepository = ordersRepository;
            this.EvaluationsRepository = evaluationsRepository ?? UBB_SE_2026_923_2.Shared.SharedServiceProvider.Services.GetRequiredService<IEvaluationsRepository>();
            this.PrescriptionService = new PrescriptionService(itemsRepository, this.EvaluationsRepository);
            this.injectedActiveUser = activeUser;
            this.basketRepository = basketRepository;
        }

        private IBasketRepository? BasketRepository =>
            this.basketRepository ??= UBB_SE_2026_923_2.Shared.SharedServiceProvider.Services.GetService<IBasketRepository>();

        private static Dictionary<int, BasketEntry> CloneBasket(Dictionary<int, BasketEntry> basket)
        {
            return basket.ToDictionary(
                entry => entry.Key,
                entry => new BasketEntry(entry.Value.Quantity, entry.Value.ExtraDiscountPercentage));
        }

        private void RestoreBasketFromSharedStore()
        {
            User activeUser = this.ActiveUser;
            IBasketRepository? repository = this.BasketRepository;
            if (activeUser == null || repository == null)
            {
                return;
            }

            activeUser.Basket = CloneBasket(repository.GetBasket(activeUser.Id));
        }

        private void SaveBasketToSharedStore()
        {
            User activeUser = this.ActiveUser;
            IBasketRepository? repository = this.BasketRepository;
            if (activeUser == null || repository == null)
            {
                return;
            }

            repository.SaveBasket(activeUser.Id, activeUser.Basket);
        }

        private void ClearBasketFromSharedStore()
        {
            User activeUser = this.ActiveUser;
            IBasketRepository? repository = this.BasketRepository;
            if (activeUser == null || repository == null)
            {
                return;
            }

            repository.ClearBasket(activeUser.Id);
        }

        private float NormalizeDiscount(float discount)
        {
            if (discount > MaximumDiscount)
            {
                discount /= PercentageDivisor;
            }

            if (discount < MinimumDiscount)
            {
                return MinimumDiscount;
            }

            if (discount > MaximumDiscount)
            {
                return MaximumDiscount;
            }

            return discount;
        }

        private static float RoundDownTo2Decimals(float value)
        {
            decimal temp = Math.Truncate((decimal)value * PriceRoundingFactor) / PriceRoundingFactor;
            return (float)temp;
        }

        private static string BuildImagePath(string originalPath)
        {
            if (string.IsNullOrWhiteSpace(originalPath))
            {
                return "/images/product-placeholder.svg";
            }

            if (originalPath.StartsWith("ms-appx://", StringComparison.OrdinalIgnoreCase))
            {
                return "/images/product-placeholder.svg";
            }

            string normalizedPath = originalPath.Replace("\\", "/").TrimStart('.', '/');
            if (!string.IsNullOrWhiteSpace(normalizedPath))
            {
                return "/" + normalizedPath;
            }

            return "/images/product-placeholder.svg";
        }

        private BasketItemViewModel BuildBasketItemViewModel(int itemId, BasketEntry basketEntry)
        {
            Item currentItem = this.ItemsRepository.GetItemById(itemId);

            float baseItemDiscount = this.NormalizeDiscount(currentItem.DiscountPercentage);
            float extraItemDiscount = this.NormalizeDiscount(basketEntry.ExtraDiscountPercentage);
            float userDiscount = NoExtraDiscount;

            if (this.ActiveUser.UserDiscounts.ContainsKey(currentItem.Id))
            {
                userDiscount = this.NormalizeDiscount(this.ActiveUser.UserDiscounts[currentItem.Id]);
            }

            BasketItemViewModel basketItem = new BasketItemViewModel(
                currentItem.Id,
                BuildImagePath(currentItem.ImagePath),
                currentItem.Name,
                currentItem.Producer,
                basketEntry.Quantity,
                baseItemDiscount,
                extraItemDiscount,
                userDiscount,
                currentItem.Price);

            this.RecalculateBasketItemPrices(basketItem);

            return basketItem;
        }

        public void AddToBasket(int itemId, int quantityToBuy)
        {
            this.AddItemToBasket(itemId, quantityToBuy, NoExtraDiscount);
        }

        public void AddItemToBasket(int itemId, int quantityToBuy, float extraDiscountPercentage = NoExtraDiscount)
        {
            this.RestoreBasketFromSharedStore();

            if (this.ActiveUser.Basket.ContainsKey(itemId))
            {
                this.ActiveUser.Basket[itemId].Quantity += quantityToBuy;

                if (extraDiscountPercentage > this.ActiveUser.Basket[itemId].ExtraDiscountPercentage)
                {
                    this.ActiveUser.Basket[itemId].ExtraDiscountPercentage = extraDiscountPercentage;
                }

                this.SaveBasketToSharedStore();
                return;
            }

            this.ActiveUser.AddItemToBasket(itemId, quantityToBuy, extraDiscountPercentage);
            this.SaveBasketToSharedStore();
        }

        public void UpdateBasketItemQuantity(int itemId, int newQuantityToBuy)
        {
            this.RestoreBasketFromSharedStore();

            this.ActiveUser.Basket[itemId].Quantity = newQuantityToBuy;

            if (this.ActiveUser.Basket[itemId].Quantity <= EmptyQuantity)
            {
                this.ActiveUser.RemoveItemFromBasket(itemId);
            }

            this.SaveBasketToSharedStore();
        }

        public void RemoveFromBasket(int itemIdToRemove)
        {
            this.RestoreBasketFromSharedStore();
            this.ActiveUser.RemoveItemFromBasket(itemIdToRemove);
            this.SaveBasketToSharedStore();
        }

        public List<BasketItemViewModel> GetBasketItems()
        {
            this.RestoreBasketFromSharedStore();

            List<BasketItemViewModel> basketItems = new();
            List<int> invalidItemIds = new();

            if (this.ActiveUser == null)
            {
                return basketItems;
            }

            foreach (KeyValuePair<int, BasketEntry> item in this.ActiveUser.Basket)
            {
                try
                {
                    basketItems.Add(this.BuildBasketItemViewModel(item.Key, item.Value));
                }
                catch
                {
                    invalidItemIds.Add(item.Key);
                }
            }

            foreach (int invalidItemId in invalidItemIds)
            {
                this.ActiveUser.RemoveItemFromBasket(invalidItemId);
            }

            if (invalidItemIds.Count > 0)
            {
                this.SaveBasketToSharedStore();
            }

            return basketItems;
        }

        public void RecalculateBasketItemPrices(BasketItemViewModel basketItem)
        {
            float finalPriceBeforeDiscount = RoundDownTo2Decimals(
                basketItem.InitialPricePerBox * basketItem.ItemQuantityInBasket);

            float discountedPrice = finalPriceBeforeDiscount;
            discountedPrice *= MaximumDiscount - basketItem.BaseItemDiscount;
            discountedPrice *= MaximumDiscount - basketItem.ExtraItemDiscount;
            discountedPrice *= MaximumDiscount - basketItem.ItemActiveUserDiscount;

            basketItem.SetFinalPrices(
                finalPriceBeforeDiscount,
                RoundDownTo2Decimals(Math.Max(MinimumDiscount, discountedPrice)));
        }

        public Tuple<float, float> CalculateBasketTotalSum(IEnumerable<BasketItemViewModel> basketItems)
        {
            float totalBefore = basketItems.Sum(item => item.FinalPriceBeforeDiscount);
            float totalAfter = basketItems.Sum(item => item.FinalPriceAfterDiscount);
            return new Tuple<float, float>(totalBefore, totalAfter);
        }

        public Dictionary<int, int> FillBasketFromPrescription(string prescriptionId)
        {
            return this.PrescriptionService.GetItemsFromPrescription(prescriptionId, this.ActiveUser.UserDiscounts);
        }

        public void ApplyPrescriptionToBasket(string prescriptionId)
        {
            Dictionary<int, int> prescriptionItems = this.FillBasketFromPrescription(prescriptionId);

            if (prescriptionItems.Count == 0)
            {
                throw new ArgumentException("Medicine couldn't be retrieved");
            }

            foreach (KeyValuePair<int, int> itemEntry in prescriptionItems)
            {
                this.AddItemToBasket(itemEntry.Key, itemEntry.Value, NoExtraDiscount);
            }
        }

        public void CompleteOrder(int orderId, Dictionary<int, Tuple<int, float>> updatedQuantities)
        {
            Order orderToComplete = this.OrdersRepository.GetOrder(orderId);
            DateTime timeNow = DateTime.Now;
            DateOnly currentDate = new DateOnly(timeNow.Year, timeNow.Month, timeNow.Day);

            foreach (KeyValuePair<int, Tuple<int, float>> itemQuantityEntry in updatedQuantities)
            {
                int currentItemId = itemQuantityEntry.Key;
                int preferredItemQuantity = itemQuantityEntry.Value.Item1;
                Item itemToVerify = this.ItemsRepository.GetItemById(currentItemId);

                if (itemToVerify.GetQuantityAtSpecifiedDate(currentDate) < preferredItemQuantity)
                {
                    throw new ArgumentException(
                        "We don't have enough of " + itemToVerify.Name +
                        " - " + itemToVerify.Producer + "; " +
                        "delete the item from the order if you wish to complete it");
                }
            }

            orderToComplete.IsCompleted = true;

            foreach (KeyValuePair<int, Tuple<int, float>> itemEntryInOrder in orderToComplete.ItemQuantitiesWithFinalPrice.ToList())
            {
                orderToComplete.RemoveItemFromOrder(itemEntryInOrder.Key);
            }

            foreach (KeyValuePair<int, Tuple<int, float>> itemQuantityEntry in updatedQuantities)
            {
                orderToComplete.AddItemToOrder(
                    itemQuantityEntry.Key,
                    itemQuantityEntry.Value.Item1,
                    itemQuantityEntry.Value.Item2);
            }

            this.OrdersRepository.UpdateOrder(orderToComplete);

            foreach (KeyValuePair<int, Tuple<int, float>> itemQuantityEntry in updatedQuantities)
            {
                int currentItemId = itemQuantityEntry.Key;
                int itemQuantityToSubtract = itemQuantityEntry.Value.Item1;
                Item itemToUpdate = this.ItemsRepository.GetItemById(currentItemId);

                itemToUpdate.RemoveQuantityFromItem(itemQuantityToSubtract, currentDate);
                this.ItemsRepository.UpdateItemById(itemToUpdate);
            }
        }

        public void ModifyIncompleteOrder(
            int orderIdToModify,
            Dictionary<int, Tuple<int, float>> updatedQuantities,
            DateOnly updatedPickUpDate)
        {
            Order orderToModify = this.OrdersRepository.GetOrder(orderIdToModify);

            DateOnly todayDate = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            if (updatedPickUpDate <= todayDate)
            {
                throw new ArgumentException("The new pick-up date must be later than the current date");
            }

            foreach (KeyValuePair<int, Tuple<int, float>> itemQuantityEntry in updatedQuantities)
            {
                int currentItemId = itemQuantityEntry.Key;
                int preferredItemQuantity = itemQuantityEntry.Value.Item1;
                Item itemToVerify = this.ItemsRepository.GetItemById(currentItemId);

                if (itemToVerify.GetQuantityAtSpecifiedDate(updatedPickUpDate) < preferredItemQuantity)
                {
                    throw new ArgumentException(
                        "On " + updatedPickUpDate.ToString("yyyy.MM.dd") + ", " +
                        "we will have only " + itemToVerify.GetQuantityAtSpecifiedDate(updatedPickUpDate) +
                        " boxes of " + itemToVerify.Name + " - " + itemToVerify.Producer);
                }
            }

            orderToModify.PickUpDate = updatedPickUpDate;

            foreach (KeyValuePair<int, Tuple<int, float>> itemEntryInOrder in orderToModify.ItemQuantitiesWithFinalPrice.ToList())
            {
                orderToModify.RemoveItemFromOrder(itemEntryInOrder.Key);
            }

            foreach (KeyValuePair<int, Tuple<int, float>> itemQuantityEntry in updatedQuantities)
            {
                orderToModify.AddItemToOrder(
                    itemQuantityEntry.Key,
                    itemQuantityEntry.Value.Item1,
                    itemQuantityEntry.Value.Item2);
            }

            this.OrdersRepository.UpdateOrder(orderToModify);
        }

        private void AddOrderWithItems(int clientId, DateOnly pickUpDate, Dictionary<int, Tuple<int, float>> items, bool isCompleted = false, bool isExpired = false)
        {
            int newOrderId = this.OrdersRepository.AddOrder(clientId, pickUpDate, isCompleted, isExpired);
            Order newOrder = new Order(newOrderId, new User { Id = clientId }, pickUpDate, isCompleted, isExpired);

            foreach (KeyValuePair<int, Tuple<int, float>> item in items)
            {
                int itemId = item.Key;
                int itemQuantity = item.Value.Item1;
                float finalPrice = item.Value.Item2;

                newOrder.AddItemToOrder(itemId, itemQuantity, finalPrice);
            }

            this.OrdersRepository.UpdateOrder(newOrder);
        }

        public void PlaceOrderFromBasket(DateOnly chosenPickUpDate)
        {
            this.RestoreBasketFromSharedStore();

            Dictionary<int, Tuple<int, float>> itemInformationForOrder = new Dictionary<int, Tuple<int, float>>();

            foreach (KeyValuePair<int, BasketEntry> basketItemEntry in this.ActiveUser.Basket)
            {
                Item currentItem = this.ItemsRepository.GetItemById(basketItemEntry.Key);
                int currentItemQuantity = basketItemEntry.Value.Quantity;
                float extraDiscountAmount = this.NormalizeDiscount(basketItemEntry.Value.ExtraDiscountPercentage);

                int itemQuantityAtPickUpDate = currentItem.GetQuantityAtSpecifiedDate(chosenPickUpDate);

                if (currentItemQuantity > itemQuantityAtPickUpDate)
                {
                    throw new ArgumentException(
                        "On " + chosenPickUpDate.ToString("yyyy.MM.dd") + ", " +
                        "we will have only " + itemQuantityAtPickUpDate + " boxes " +
                        "of " + currentItem.Name + " by " + currentItem.Producer + " " +
                        "instead of " + currentItemQuantity + ".");
                }

                float itemDiscountAmount = this.NormalizeDiscount(currentItem.DiscountPercentage);
                float userDiscountAmount = MinimumDiscount;

                if (this.ActiveUser.UserDiscounts.ContainsKey(currentItem.Id))
                {
                    userDiscountAmount = this.NormalizeDiscount(this.ActiveUser.UserDiscounts[currentItem.Id]);
                }

                float finalPriceCalculation = currentItemQuantity * currentItem.Price;
                finalPriceCalculation *= MaximumDiscount - itemDiscountAmount;
                finalPriceCalculation *= MaximumDiscount - extraDiscountAmount;
                finalPriceCalculation *= MaximumDiscount - userDiscountAmount;

                itemInformationForOrder.Add(
                    currentItem.Id,
                    new Tuple<int, float>(currentItemQuantity, finalPriceCalculation));
            }

            this.AddOrderWithItems(this.ActiveUser.Id, chosenPickUpDate, itemInformationForOrder);
            this.ActiveUser.Basket.Clear();
            this.ClearBasketFromSharedStore();
        }

        public void ResubmitExpiredOrder(int orderIdToResubmit, DateOnly chosenPickUpDate)
        {
            Order expiredOrder = this.OrdersRepository.GetOrder(orderIdToResubmit);
            Dictionary<int, Tuple<int, float>> itemInformationForOrder = expiredOrder.ItemQuantitiesWithFinalPrice;

            foreach (KeyValuePair<int, Tuple<int, float>> orderItemEntry in itemInformationForOrder)
            {
                Item currentItem = this.ItemsRepository.GetItemById(orderItemEntry.Key);
                int currentItemQuantity = orderItemEntry.Value.Item1;
                int itemQuantityAtPickUpDate = currentItem.GetQuantityAtSpecifiedDate(chosenPickUpDate);

                if (currentItemQuantity > itemQuantityAtPickUpDate)
                {
                    throw new ArgumentException(
                        "On " + chosenPickUpDate.ToString("yyyy.MM.dd") + ", " +
                        "we will have only " + itemQuantityAtPickUpDate + " boxes " +
                        "of " + currentItem.Name + " by " + currentItem.Producer + " " +
                        "instead of " + currentItemQuantity + ".");
                }
            }

            this.AddOrderWithItems(this.ActiveUser.Id, chosenPickUpDate, itemInformationForOrder);
            this.OrdersRepository.RemoveOrder(orderIdToResubmit); // or mark as no longer expired
        }

        public void CancelOrder(int orderIdToCancel)
        {
            Order orderToCancel = this.OrdersRepository.GetOrder(orderIdToCancel);
            orderToCancel.IsExpired = true;
            this.OrdersRepository.UpdateOrder(orderToCancel);
        }

        public void ExpireOverdueOrders()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            List<Order> allOrders = this.OrdersRepository.GetAllOrders();
            foreach (Order order in allOrders)
            {
                if (!order.IsExpired && !order.IsCompleted && today > order.PickUpDate.AddDays(Order.OrderExpirationDays))
                {
                    order.IsExpired = true;
                    this.OrdersRepository.UpdateOrder(order);
                }
            }
        }
    }
}
