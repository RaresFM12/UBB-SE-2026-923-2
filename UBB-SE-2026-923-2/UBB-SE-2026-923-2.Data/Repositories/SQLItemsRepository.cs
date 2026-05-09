namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using UBB_SE_2026_923_2.Data;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// EF Core implementation of <see cref="IItemsRepository"/>.
    /// <para>
    /// Items are loaded together with their related <see cref="ItemSubstance"/>
    /// and <see cref="ItemBatch"/> rows through navigation collections, then
    /// projected into the legacy <see cref="Item.ActiveSubstances"/> and
    /// <see cref="Item.Batches"/> dictionaries so existing services and view
    /// models continue to work unchanged.
    /// </para>
    /// </summary>
    public class SQLItemsRepository : IItemsRepository
    {
        private const int TopItemsLimit = 30;
        private const string ImagePathDefault = "..\\..\\Assets\\placeholder.png";

        private readonly IDbContextFactory<AppDbContext> databaseContextFactory;

        public SQLItemsRepository(IDbContextFactory<AppDbContext> databaseContextFactory)
        {
            this.databaseContextFactory = databaseContextFactory ?? throw new ArgumentNullException(nameof(databaseContextFactory));
        }

        public void AddItem(string name, string producer, string category,
            float price, int numberOfPills,
            string label = "", string description = "", string imagePath = ImagePathDefault,
            float discount = 0f)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();

            var newItem = new Item(name, producer, category, price, numberOfPills,
                                quantity: 0, label, description, imagePath, discount);

            databaseContext.Items.Add(newItem);
            databaseContext.SaveChanges();
        }

        public void AddItemWithQuantity(string name, string producer, string category,
            float price, int numberOfPills,
            int quantity, Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches,
            string label = "", string description = "", string imagePath = ImagePathDefault,
            float discount = 0f)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();

            var newItem = new Item(name, producer, category, price, numberOfPills,
                                activeSubstances, batches, quantity,
                                label, description, imagePath, discount);

            // Project the legacy dictionaries into the EF-mapped navigation
            // collections — the dictionaries themselves are [NotMapped].
            foreach (var substanceConcentrationEntry in activeSubstances)
            {
                string substanceName = substanceConcentrationEntry.Key;
                var substanceEntity = databaseContext.Substances
                    .FirstOrDefault(substance => substance.Name == substanceName);

                if (substanceEntity is null)
                {
                    continue;
                }

                newItem.ItemSubstanceEntries.Add(new ItemSubstance
                {
                    Substance = substanceEntity,
                    Concentration = substanceConcentrationEntry.Value,
                });
            }

            foreach (var batchEntry in batches)
            {
                newItem.ItemBatchEntries.Add(new ItemBatch
                {
                    ExpirationDate = batchEntry.Key,
                    NumberOfPacks = batchEntry.Value,
                });
            }

            databaseContext.Items.Add(newItem);
            databaseContext.SaveChanges();
        }

        public void RemoveItemById(int itemIdToRemove)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();

            var itemToRemove = databaseContext.Items.FirstOrDefault(item => item.Id == itemIdToRemove);
            if (itemToRemove is null)
            {
                return;
            }

            // Cascade is configured for substances/batches/order items already.
            // User-side links (UserNotifications, UserDiscounts) reference Item
            // with cascade as well; just remove the parent.
            databaseContext.Items.Remove(itemToRemove);
            databaseContext.SaveChanges();
        }

        public Item GetItemById(int itemId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var item = databaseContext.Items
                .AsNoTracking()
                .Include(item => item.ItemSubstanceEntries)
                    .ThenInclude(itemSubstanceLink => itemSubstanceLink.Substance)
                .Include(item => item.ItemBatchEntries)
                .FirstOrDefault(item => item.Id == itemId);

            return item is null ? null! : ProjectIntoLegacyDictionaries(item);
        }

        public List<Item> GetAllItems()
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var items = databaseContext.Items
                .AsNoTracking()
                .Include(item => item.ItemSubstanceEntries)
                    .ThenInclude(itemSubstanceLink => itemSubstanceLink.Substance)
                .Include(item => item.ItemBatchEntries)
                .ToList();
            return items.Select(item => ProjectIntoLegacyDictionaries(item)).ToList();
        }

        public List<Item> GetItemsByName(string name)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var items = databaseContext.Items
                .AsNoTracking()
                .Include(item => item.ItemSubstanceEntries)
                    .ThenInclude(itemSubstanceLink => itemSubstanceLink.Substance)
                .Include(item => item.ItemBatchEntries)
                .Where(item => item.Name == name)
                .ToList();
            return items.Select(item => ProjectIntoLegacyDictionaries(item)).ToList();
        }

        public void UpdateItemById(Item updatedItem)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();

            var existingItem = databaseContext.Items
                .Include(item => item.ItemSubstanceEntries)
                .Include(item => item.ItemBatchEntries)
                .FirstOrDefault(item => item.Id == updatedItem.Id);

            if (existingItem is null)
            {
                return;
            }

            existingItem.Name = updatedItem.Name;
            existingItem.Price = updatedItem.Price;
            existingItem.Category = updatedItem.Category;
            existingItem.NumberOfPills = updatedItem.NumberOfPills;
            existingItem.Producer = updatedItem.Producer;
            existingItem.ImagePath = updatedItem.ImagePath;
            existingItem.Label = updatedItem.Label;
            existingItem.Description = updatedItem.Description;
            existingItem.DiscountPercentage = updatedItem.DiscountPercentage;

            // Replace the related collections from the legacy dictionaries.
            // Phase 3 will switch callers to mutate ItemSubstanceEntries /
            // ItemBatchEntries directly, at which point this projection goes.
            databaseContext.ItemSubstances.RemoveRange(existingItem.ItemSubstanceEntries);
            existingItem.ItemSubstanceEntries.Clear();
            foreach (var substanceConcentrationEntry in updatedItem.ActiveSubstances)
            {
                string substanceName = substanceConcentrationEntry.Key;
                var substanceEntity = databaseContext.Substances
                    .FirstOrDefault(substance => substance.Name == substanceName);

                if (substanceEntity is null)
                {
                    continue;
                }

                existingItem.ItemSubstanceEntries.Add(new ItemSubstance
                {
                    Substance = substanceEntity,
                    Concentration = substanceConcentrationEntry.Value,
                });
            }

            databaseContext.ItemBatches.RemoveRange(existingItem.ItemBatchEntries);
            existingItem.ItemBatchEntries.Clear();
            foreach (var batchEntry in updatedItem.Batches)
            {
                existingItem.ItemBatchEntries.Add(new ItemBatch
                {
                    ExpirationDate = batchEntry.Key,
                    NumberOfPacks = batchEntry.Value,
                });
            }

            databaseContext.SaveChanges();
        }

        public bool ItemExists(int itemId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.Items.AsNoTracking().Any(item => item.Id == itemId);
        }

        public List<Tuple<int, string, int>> GetTop30Items()
        {
            DateOnly oneMonthAgo = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));

            using var databaseContext = this.databaseContextFactory.CreateDbContext();

            return databaseContext.OrderItems
                .AsNoTracking()
                .Where(orderItem => orderItem.Order.PickUpDate >= oneMonthAgo)
                .GroupBy(orderItem => new { orderItem.Item.Id, orderItem.Item.Name })
                .Select(itemGroup => new
                {
                    itemGroup.Key.Id,
                    itemGroup.Key.Name,
                    NumberOfOrders = itemGroup.Count(),
                })
                .OrderByDescending(itemRow => itemRow.NumberOfOrders)
                .Take(TopItemsLimit)
                .AsEnumerable()
                .Select(itemRow => new Tuple<int, string, int>(itemRow.Id, itemRow.Name, itemRow.NumberOfOrders))
                .ToList();
        }

        private static Item ProjectIntoLegacyDictionaries(Item item)
        {
            // Move data from the EF nav collections into the legacy dict
            // properties so existing call sites keep working unchanged.
            // Use the domain methods so Item.Quantity is recomputed correctly.
            foreach (var itemSubstanceLink in item.ItemSubstanceEntries)
            {
                string substanceName = itemSubstanceLink.Substance.Name;
                if (!item.ActiveSubstances.ContainsKey(substanceName))
                {
                    item.AddActiveSubstanceToItem(substanceName, itemSubstanceLink.Concentration);
                }
            }

            foreach (var itemBatch in item.ItemBatchEntries)
            {
                if (!item.Batches.ContainsKey(itemBatch.ExpirationDate))
                {
                    item.AddNewBatchToItem(itemBatch.ExpirationDate, itemBatch.NumberOfPacks);
                }
            }

            return item;
        }
    }
}
