using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UBB_SE_2026_923_2.Data;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="IOrdersRepository"/>. Order line
    /// items are loaded through the <see cref="Order.OrderItemEntries"/>
    /// navigation collection and projected back into the legacy
    /// <see cref="Order.ItemQuantitiesWithFinalPrice"/> dictionary so existing
    /// services and view models keep working unchanged.
    /// </summary>
    public class SQLOrdersRepository : IOrdersRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public SQLOrdersRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public int AddOrder(int clientId, DateOnly pickUpDate, bool isCompleted = false, bool isExpired = false)
        {
            using var db = dbContextFactory.CreateDbContext();

            var order = new Order(0, clientId, pickUpDate, isCompleted, isExpired);
            db.Orders.Add(order);
            db.SaveChanges();
            return order.Id;
        }

        public void RemoveOrder(int orderIdToBeRemoved)
        {
            using var db = dbContextFactory.CreateDbContext();
            var order = db.Orders.FirstOrDefault(o => o.Id == orderIdToBeRemoved);
            if (order is null)
            {
                return;
            }

            // Cascade is configured Order → OrderItem; just remove the parent.
            db.Orders.Remove(order);
            db.SaveChanges();
        }

        public void UpdateOrder(Order newOrder)
        {
            using var db = dbContextFactory.CreateDbContext();
            var existing = db.Orders
                .Include(o => o.OrderItemEntries)
                .FirstOrDefault(o => o.Id == newOrder.Id);

            if (existing is null)
            {
                return;
            }

            existing.ClientId = newOrder.ClientId;
            existing.PickUpDate = newOrder.PickUpDate;
            existing.IsCompleted = newOrder.IsCompleted;
            existing.IsExpired = newOrder.IsExpired;

            // Replace the line items from the legacy dictionary on the
            // incoming Order. Phase 3 will switch callers to mutate
            // OrderItemEntries directly.
            db.OrderItems.RemoveRange(existing.OrderItemEntries);
            existing.OrderItemEntries.Clear();
            foreach (var pair in newOrder.ItemQuantitiesWithFinalPrice)
            {
                existing.OrderItemEntries.Add(new OrderItem
                {
                    OrderId = existing.Id,
                    ItemId = pair.Key,
                    OrderQuantity = pair.Value.Item1,
                    Price = pair.Value.Item2,
                });
            }

            db.SaveChanges();
        }

        public Order GetOrder(int orderId)
        {
            using var db = dbContextFactory.CreateDbContext();
            var order = db.Orders
                .AsNoTracking()
                .Include(o => o.OrderItemEntries)
                .FirstOrDefault(o => o.Id == orderId);

            return order is null ? null! : ProjectIntoLegacyDictionary(order);
        }

        public List<Order> GetAllOrders()
        {
            using var db = dbContextFactory.CreateDbContext();
            var orders = db.Orders
                .AsNoTracking()
                .Include(o => o.OrderItemEntries)
                .ToList();

            foreach (var order in orders)
            {
                ProjectIntoLegacyDictionary(order);
            }

            return orders;
        }

        public List<Order> GetOrdersOfClient(int clientId)
        {
            using var db = dbContextFactory.CreateDbContext();
            var orders = db.Orders
                .AsNoTracking()
                .Include(o => o.OrderItemEntries)
                .Where(o => o.ClientId == clientId)
                .ToList();

            foreach (var order in orders)
            {
                ProjectIntoLegacyDictionary(order);
            }

            return orders;
        }

        public bool OrderExists(int orderId)
        {
            using var db = dbContextFactory.CreateDbContext();
            return db.Orders.AsNoTracking().Any(o => o.Id == orderId);
        }

        private static Order ProjectIntoLegacyDictionary(Order order)
        {
            foreach (var line in order.OrderItemEntries)
            {
                if (!order.ItemQuantitiesWithFinalPrice.ContainsKey(line.ItemId))
                {
                    order.AddItemToOrder(line.ItemId, line.OrderQuantity, line.Price);
                }
            }

            return order;
        }
    }
}
