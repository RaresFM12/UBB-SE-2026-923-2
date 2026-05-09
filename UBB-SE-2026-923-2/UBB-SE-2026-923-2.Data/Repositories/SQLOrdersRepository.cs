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

            var clientUser = databaseContext.Users.Find(clientId);
            if (clientUser == null)
            {
                clientUser = new User { Id = clientId };
                databaseContext.Attach(clientUser);
            }

            var newOrder = new Order(0, clientUser, pickUpDate, isCompleted, isExpired);
            databaseContext.Orders.Add(newOrder);
            databaseContext.SaveChanges();
            return newOrder.Id;
        }

        public void RemoveOrder(int orderIdToBeRemoved)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var orderToRemove = databaseContext.Orders.FirstOrDefault(order => order.Id == orderIdToBeRemoved);
            if (orderToRemove is null)
            {
                return;
            }

            // Cascade is configured Order → OrderItem; just remove the parent.
            databaseContext.Orders.Remove(orderToRemove);
            databaseContext.SaveChanges();
        }

        public void UpdateOrder(Order updatedOrder)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var existingOrder = databaseContext.Orders
                .Include(order => order.OrderItemEntries)
                .FirstOrDefault(order => order.Id == updatedOrder.Id);

            if (existingOrder is null)
            {
                return;
            }

            existingOrder.PickUpDate = updatedOrder.PickUpDate;
            existingOrder.IsCompleted = updatedOrder.IsCompleted;
            existingOrder.IsExpired = updatedOrder.IsExpired;

            // Replace the line items from the legacy dictionary on the
            // incoming Order. Phase 3 will switch callers to mutate
            // OrderItemEntries directly.
            databaseContext.OrderItems.RemoveRange(existingOrder.OrderItemEntries);
            existingOrder.OrderItemEntries.Clear();
            foreach (var itemQuantityEntry in updatedOrder.ItemQuantitiesWithFinalPrice)
            {
                int itemIdentifier = itemQuantityEntry.Key;
                var itemEntity = databaseContext.Items.Find(itemIdentifier)
                    ?? throw new ArgumentException($"Item with identifier {itemIdentifier} not found.");

                existingOrder.OrderItemEntries.Add(new OrderItem
                {
                    Item = itemEntity,
                    OrderQuantity = itemQuantityEntry.Value.Item1,
                    Price = itemQuantityEntry.Value.Item2,
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
                    .ThenInclude(orderItem => orderItem.Item)
                .FirstOrDefault(order => order.Id == orderId);

            return order is null ? null! : ProjectIntoLegacyDictionary(order);
        }

        public List<Order> GetAllOrders()
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var orders = databaseContext.Orders
                .AsNoTracking()
                .Include(order => order.OrderItemEntries)
                    .ThenInclude(orderItem => orderItem.Item)
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
                    .ThenInclude(orderItem => orderItem.Item)
                .Where(order => EF.Property<int>(order, "ClientId") == clientId)
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
                int itemIdentifier = orderItem.Item.Id;
                if (!order.ItemQuantitiesWithFinalPrice.ContainsKey(itemIdentifier))
                {
                    order.AddItemToOrder(itemIdentifier, orderItem.OrderQuantity, orderItem.Price);
                }
            }

            return order;
        }
    }
}
