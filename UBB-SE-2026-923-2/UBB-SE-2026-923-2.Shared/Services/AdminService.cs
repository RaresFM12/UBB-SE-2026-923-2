namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;

    public class AdminService : IAdminService
    {
        private const int EmptyQuantity = 0;
        private const int MinimumPositiveValue = 1;

        private const string StockAlertTitle = "Stock Alert";
        private const string ProductExpiredTitle = "Product Expired";
        private const string NewItemBackInStockMessage = "New item back in stock!";
        private const string ExpiredItemsMessage = "Some items have expired. Please check and remove them.";
        private const string GoToProductsActionText = "Go to products";
        private const string GoToProductsActionTextCapitalized = "Go to Products";
        private const string ProductExpiredBodyTemplate = "Product: {0} expired. Please remove it";

        private readonly IItemsRepository itemRepository;
        private readonly ISubstancesRepository substanceRepository;

        public AdminService()
        {
            // Parameterless overload kept for legacy call sites that don't go
            // through DI yet. Repositories are resolved from the application
            // service provider so the EF Core implementations are used.
            this.itemRepository = UBB_SE_2026_923_2.Shared.SharedServiceProvider.Services.GetRequiredService<IItemsRepository>();
            this.substanceRepository = UBB_SE_2026_923_2.Shared.SharedServiceProvider.Services.GetRequiredService<ISubstancesRepository>();
        }

        public List<Item> GetAllItems()
        {
            return this.itemRepository.GetAllItems();
        }

        public List<Substance> GetAllSubstances()
        {
            return this.substanceRepository.GetAllSubstances();
        }

        public List<Item> SearchItemsByName(string query)
        {
            string loweredQuery = (query ?? string.Empty).ToLower();
            return this.itemRepository.GetAllItems()
                .Where(item => item.Name.ToLower().Contains(loweredQuery))
                .ToList();
        }

        public Item GetItemById(int itemId)
        {
            return this.itemRepository.GetItemById(itemId);
        }

        public Substance GetSubstanceByName(string name)
        {
            return this.substanceRepository.GetSubstanceByName(name);
        }

        public bool SubstanceExists(string name)
        {
            return this.substanceRepository.SubstanceExists(name);
        }

        public AdminService(IItemsRepository itemRepo, ISubstancesRepository substanceRepo)
        {
            this.itemRepository = itemRepo;
            this.substanceRepository = substanceRepo;
        }

        public void AddItem(Item newItem)
        {
            try
            {
                this.ValidateItemForAdd(newItem);
                this.itemRepository.AddItemWithQuantity(newItem.Name, newItem.Producer, newItem.Category,
                                       newItem.Price, newItem.NumberOfPills, newItem.Quantity, newItem.ActiveSubstances, newItem.Batches,
                                       newItem.Label, newItem.Description, newItem.ImagePath,
                                       newItem.DiscountPercentage);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error adding item: {exception.Message}");
            }
        }

        public void AddItemWithQuantity(Item newItem)
        {
            try
            {
                this.ValidateItemForAdd(newItem);
                this.itemRepository.AddItemWithQuantity(newItem.Name, newItem.Producer, newItem.Category,
                                       newItem.Price, newItem.NumberOfPills,
                                       newItem.Quantity, newItem.ActiveSubstances, newItem.Batches,
                                       newItem.Label, newItem.Description, newItem.ImagePath,
                                       newItem.DiscountPercentage);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error adding item: {exception.Message}");
            }
        }

        public void RemoveItemById(int itemId)
        {
            this.itemRepository.RemoveItemById(itemId);
        }

        public void UpdateItemById(int itemId, Item updatedItem)
        {
            if (!this.itemRepository.ItemExists(itemId))
            {
                throw new ArgumentException("Item with the specified ID does not exist.");
            }

            Item previousItem = this.itemRepository.GetItemById(itemId);
            if (previousItem.Quantity == EmptyQuantity && updatedItem.Quantity >= MinimumPositiveValue)
            {
                this.SendNewStockNotification(updatedItem);
            }

            updatedItem.Id = itemId;
            this.itemRepository.UpdateItemById(updatedItem);
        }

        public void AddSubstance(Substance newSubstance)
        {
            if (this.substanceRepository.SubstanceExists(newSubstance.Name))
            {
                throw new ArgumentException("Substance " + newSubstance.Name + " exists already.");
            }

            this.substanceRepository.AddSubstance(newSubstance.Name, newSubstance.LethalDose, newSubstance.Description);
        }

        public void RemoveSubstanceByName(Substance substance)
        {
            if (!this.substanceRepository.SubstanceExists(substance.Name))
            {
                throw new ArgumentException("Substance " + substance.Name + " does NOT exist.");
            }

            this.substanceRepository.RemoveSubstanceByName(substance.Name);
        }

        public void UpdateSubstanceByName(string name, Substance substance)
        {
            if (!this.substanceRepository.SubstanceExists(substance.Name))
            {
                throw new ArgumentException("Substance " + substance.Name + "does NOT exist.");
            }

            this.substanceRepository.UpdateSubstanceByName(substance);
        }

        public Notification SendNewStockNotification(Item item)
        {
            string message = $"The item {item.Name} is back in stock with quantity {item.Quantity}," +
                $"number of pills {item.NumberOfPills!}," +
                $"producer {item.Producer}";
            Notification notification = new(StockAlertTitle, NewItemBackInStockMessage);
            return notification;
        }

        public List<Item> GetExpiredItems()
        {
            List<Item> expiredItems = new();
            List<Item> allItems = this.itemRepository.GetAllItems();
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            foreach (Item item in allItems)
            {
                foreach (var batch in item.Batches)
                {
                    if (batch.Key < currentDate)
                    {
                        expiredItems.Add(item);
                        break;
                    }
                }
            }

            this.SendAboutToExpireNotification();
            return expiredItems;
        }

        public Notification SendAboutToExpireNotification()
        {
            Notification notification = new(ProductExpiredTitle, ExpiredItemsMessage);
            return notification;
        }

        public void ValidateItemForAdd(Item item)
        {
            if (item.Name == string.Empty ||
                item.Producer == string.Empty ||
                item.Price < MinimumPositiveValue ||
                item.NumberOfPills < MinimumPositiveValue ||
                item.Quantity < EmptyQuantity ||
                item.DiscountPercentage < EmptyQuantity ||
                item.ActiveSubstances.Count == EmptyQuantity)
            {
                throw new ArgumentException("Invalid item data. Please check the input and try again.");
            }

            bool nameAlreadyExists = this.itemRepository.GetAllItems()
                .Any(existingItem => string.Equals(existingItem.Name, item.Name, StringComparison.OrdinalIgnoreCase));
            if (nameAlreadyExists)
            {
                throw new ArgumentException($"An item named \"{item.Name}\" already exists.");
            }
        }

        public List<Notification> GetNotificationsForUser(User user)
        {
            List<Notification> notifications = new();
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (user.IsAdmin)
            {
                List<Item> items = this.itemRepository.GetAllItems();
                foreach (Item item in items)
                {
                    foreach (KeyValuePair<DateOnly, int> batch in item.Batches)
                    {
                        if (batch.Key <= today)
                        {
                            notifications.Add(new(
                                ProductExpiredTitle,
                                string.Format(ProductExpiredBodyTemplate, item.Id),
                                GoToProductsActionTextCapitalized));
                            break;
                        }
                    }
                }
            }

            foreach (int itemId in user.StockAlerts)
            {
                Item item = this.itemRepository.GetItemById(itemId);
                if (item.Quantity >= MinimumPositiveValue)
                {
                    string concentrations = item.ActiveSubstances != null && item.ActiveSubstances.Any()
                        ? string.Join(", ", item.ActiveSubstances.Select(substance => $"{substance.Key} ({substance.Value})"))
                        : "None";
                    string body = $"{item.Name}, {item.NumberOfPills} pills, {concentrations}, {item.Producer}";
                    notifications.Add(new(StockAlertTitle, body, GoToProductsActionText));
                }
            }

            return notifications;
        }

        public List<Tuple<int, string, int>> GetTop30Items()
        {
            return this.itemRepository.GetTop30Items();
        }

        public Dictionary<string, int> GetTop30Substances()
        {
            return this.substanceRepository.GetTop30Substances();
        }
    }
}
