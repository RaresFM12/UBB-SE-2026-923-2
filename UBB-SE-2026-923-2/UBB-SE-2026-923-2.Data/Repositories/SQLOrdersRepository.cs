namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using UBB_SE_2026_923_2.Data;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// EF Core implementation of <see cref="IOrdersRepository"/>. Order line
    /// items are loaded through the <see cref="Order.OrderItemEntries"/>
    /// navigation collection and projected back into the legacy
    /// <see cref="Order.ItemQuantitiesWithFinalPrice"/> dictionary so existing
    /// services and view models keep working unchanged.
    /// </summary>
    public class SQLOrdersRepository : IOrdersRepository
    {
        private readonly IDbContextFactory<AppDbContext> databaseContextFactory;

        public SQLOrdersRepository(IDbContextFactory<AppDbContext> databaseContextFactory)
        {
            this.databaseContextFactory = databaseContextFactory ?? throw new ArgumentNullException(nameof(databaseContextFactory));
        }

        public int AddOrder(int clientId, DateOnly pickUpDate, bool isCompleted = false, bool isExpired = false)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();

            var order = new Order(0, clientId, pickUpDate, isCompleted, isExpired);
            databaseContext.Orders.Add(order);
            databaseContext.SaveChanges();
            return order.Id;
        }

        public void RemoveOrder(int orderIdToBeRemoved)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var order = databaseContext.Orders.FirstOrDefault(order => order.Id == orderIdToBeRemoved);
            if (order is null)
            {
                return;
            }

            // Cascade is configured Order → OrderItem; just remove the parent.
            databaseContext.Orders.Remove(order);
            databaseContext.SaveChanges();
        }

        public void UpdateOrder(Order newOrder)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var existing = databaseContext.Orders
                .Include(order => order.OrderItemEntries)
                .FirstOrDefault(order => order.Id == newOrder.Id);

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
            databaseContext.OrderItems.RemoveRange(existing.OrderItemEntries);
            existing.OrderItemEntries.Clear();
            foreach (var keyValuePair in newOrder.ItemQuantitiesWithFinalPrice)
            {
                existing.OrderItemEntries.Add(new OrderItem
                {
                    OrderId = existing.Id,
                    ItemId = keyValuePair.Key,
                    OrderQuantity = keyValuePair.Value.Item1,
                    Price = keyValuePair.Value.Item2,
                });
            }

            databaseContext.SaveChanges();
        }

        public Order GetOrder(int orderId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var order = databaseContext.Orders
                .AsNoTracking()
                .Include(order => order.OrderItemEntries)
                .FirstOrDefault(order => order.Id == orderId);

            return order is null ? null! : ProjectIntoLegacyDictionary(order);
        }

        public List<Order> GetAllOrders()
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var orders = databaseContext.Orders
                .AsNoTracking()
                .Include(order => order.OrderItemEntries)
                .ToList();

            foreach (var order in orders)
            {
                ProjectIntoLegacyDictionary(order);
            }

            return orders;
        }

        public List<Order> GetOrdersOfClient(int clientId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var orders = databaseContext.Orders
                .AsNoTracking()
                .Include(order => order.OrderItemEntries)
                .Where(order => order.ClientId == clientId)
                .ToList();

            foreach (var order in orders)
            {
                ProjectIntoLegacyDictionary(order);
            }

            return orders;
        }

        public bool OrderExists(int orderId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.Orders.AsNoTracking().Any(order => order.Id == orderId);
        }

        private static Order ProjectIntoLegacyDictionary(Order order)
        {
            foreach (var orderItem in order.OrderItemEntries)
            {
                if (!order.ItemQuantitiesWithFinalPrice.ContainsKey(orderItem.ItemId))
                {
                    order.AddItemToOrder(orderItem.ItemId, orderItem.OrderQuantity, orderItem.Price);
                }
            }

            return order;
        }
    }
}