using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UBB_SE_2026_923_2.Data;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
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

        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public SQLItemsRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public void AddItem(string name, string producer, string category,
            float price, int numberOfPills,
            string label = "", string description = "", string imagePath = ImagePathDefault,
            float discount = 0f)
        {
            using var db = dbContextFactory.CreateDbContext();

            var item = new Item(name, producer, category, price, numberOfPills,
                                quantity: 0, label, description, imagePath, discount);

            db.Items.Add(item);
            db.SaveChanges();
        }

        public void AddItemWithQuantity(string name, string producer, string category,
            float price, int numberOfPills,
            int quantity, Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches,
            string label = "", string description = "", string imagePath = ImagePathDefault,
            float discount = 0f)
        {
            using var db = dbContextFactory.CreateDbContext();

            var item = new Item(name, producer, category, price, numberOfPills,
                                activeSubstances, batches, quantity,
                                label, description, imagePath, discount);

            // Project the legacy dictionaries into the EF-mapped navigation
            // collections — the dictionaries themselves are [NotMapped].
            foreach (var pair in activeSubstances)
            {
                item.ItemSubstanceEntries.Add(new ItemSubstance
                {
                    SubstanceName = pair.Key,
                    Concentration = pair.Value,
                });
            }

            foreach (var pair in batches)
            {
                item.ItemBatchEntries.Add(new ItemBatch
                {
                    ExpirationDate = pair.Key,
                    NumberOfPacks = pair.Value,
                });
            }

            db.Items.Add(item);
            db.SaveChanges();
        }

        public void RemoveItemById(int idToBeRemoved)
        {
            using var db = dbContextFactory.CreateDbContext();

            var item = db.Items.FirstOrDefault(i => i.Id == idToBeRemoved);
            if (item is null)
            {
                return;
            }

            // Cascade is configured for substances/batches/order items already.
            // User-side links (UserNotifications, UserDiscounts) reference Item
            // with cascade as well; just remove the parent.
            db.Items.Remove(item);
            db.SaveChanges();
        }

        public Item GetItemById(int id)
        {
            using var db = dbContextFactory.CreateDbContext();
            var item = db.Items
                .AsNoTracking()
                .Include(i => i.ItemSubstanceEntries)
                .Include(i => i.ItemBatchEntries)
                .FirstOrDefault(i => i.Id == id);

            return item is null ? null! : ProjectIntoLegacyDictionaries(item);
        }

        public List<Item> GetAllItems()
        {
            using var db = dbContextFactory.CreateDbContext();
            var items = db.Items
                .AsNoTracking()
                .Include(i => i.ItemSubstanceEntries)
                .Include(i => i.ItemBatchEntries)
                .ToList();

            foreach (var item in items)
            {
                ProjectIntoLegacyDictionaries(item);
            }

            return items;
        }

        public List<Item> GetItemsByName(string name)
        {
            using var db = dbContextFactory.CreateDbContext();
            var items = db.Items
                .AsNoTracking()
                .Include(i => i.ItemSubstanceEntries)
                .Include(i => i.ItemBatchEntries)
                .Where(i => i.Name == name)
                .ToList();

            foreach (var item in items)
            {
                ProjectIntoLegacyDictionaries(item);
            }

            return items;
        }

        public void UpdateItemById(Item newItem)
        {
            using var db = dbContextFactory.CreateDbContext();

            var existing = db.Items
                .Include(i => i.ItemSubstanceEntries)
                .Include(i => i.ItemBatchEntries)
                .FirstOrDefault(i => i.Id == newItem.Id);

            if (existing is null)
            {
                return;
            }

            existing.Name = newItem.Name;
            existing.Price = newItem.Price;
            existing.Category = newItem.Category;
            existing.NumberOfPills = newItem.NumberOfPills;
            existing.Producer = newItem.Producer;
            existing.ImagePath = newItem.ImagePath;
            existing.Label = newItem.Label;
            existing.Description = newItem.Description;
            existing.DiscountPercentage = newItem.DiscountPercentage;

            // Replace the related collections from the legacy dictionaries.
            // Phase 3 will switch callers to mutate ItemSubstanceEntries /
            // ItemBatchEntries directly, at which point this projection goes.
            db.ItemSubstances.RemoveRange(existing.ItemSubstanceEntries);
            existing.ItemSubstanceEntries.Clear();
            foreach (var pair in newItem.ActiveSubstances)
            {
                existing.ItemSubstanceEntries.Add(new ItemSubstance
                {
                    ItemId = existing.Id,
                    SubstanceName = pair.Key,
                    Concentration = pair.Value,
                });
            }

            db.ItemBatches.RemoveRange(existing.ItemBatchEntries);
            existing.ItemBatchEntries.Clear();
            foreach (var pair in newItem.Batches)
            {
                existing.ItemBatchEntries.Add(new ItemBatch
                {
                    ItemId = existing.Id,
                    ExpirationDate = pair.Key,
                    NumberOfPacks = pair.Value,
                });
            }

            db.SaveChanges();
        }

        public bool ItemExists(int id)
        {
            using var db = dbContextFactory.CreateDbContext();
            return db.Items.AsNoTracking().Any(i => i.Id == id);
        }

        public List<Tuple<int, string, int>> GetTop30Items()
        {
            DateOnly oneMonthAgo = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));

            using var db = dbContextFactory.CreateDbContext();

            return db.OrderItems
                .AsNoTracking()
                .Join(
                    db.Orders.Where(o => o.PickUpDate >= oneMonthAgo),
                    oi => oi.OrderId,
                    o => o.Id,
                    (oi, _) => oi.ItemId)
                .Join(
                    db.Items.AsNoTracking(),
                    itemId => itemId,
                    item => item.Id,
                    (itemId, item) => new { item.Id, item.Name })
                .GroupBy(row => new { row.Id, row.Name })
                .Select(group => new
                {
                    group.Key.Id,
                    group.Key.Name,
                    NumberOfOrders = group.Count(),
                })
                .OrderByDescending(row => row.NumberOfOrders)
                .Take(TopItemsLimit)
                .AsEnumerable()
                .Select(row => new Tuple<int, string, int>(row.Id, row.Name, row.NumberOfOrders))
                .ToList();
        }

        private static Item ProjectIntoLegacyDictionaries(Item item)
        {
            // Move data from the EF nav collections into the legacy dict
            // properties so existing call sites keep working unchanged.
            // Use the domain methods so Item.Quantity is recomputed correctly.
            foreach (var link in item.ItemSubstanceEntries)
            {
                if (!item.ActiveSubstances.ContainsKey(link.SubstanceName))
                {
                    item.AddActiveSubstanceToItem(link.SubstanceName, link.Concentration);
                }
            }

            foreach (var batch in item.ItemBatchEntries)
            {
                if (!item.Batches.ContainsKey(batch.ExpirationDate))
                {
                    item.AddNewBatchToItem(batch.ExpirationDate, batch.NumberOfPacks);
                }
            }

            return item;
        }
    }
}
