namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;

    public interface IAdminService
    {
        List<Item> GetAllItems();

        List<Substance> GetAllSubstances();

        List<Item> SearchItemsByName(string query);

        Item GetItemById(int itemId);

        Substance GetSubstanceByName(string name);

        bool SubstanceExists(string name);

        void AddItem(Item newItem);

        void AddItemWithQuantity(Item newItem);

        void RemoveItemById(int itemId);

        void UpdateItemById(int itemId, Item updatedItem);

        void AddSubstance(Substance newSubstance);

        void RemoveSubstanceByName(Substance substance);

        void UpdateSubstanceByName(string name, Substance substance);

        void ValidateItemForAdd(Item item);

        List<Item> GetExpiredItems();

        Notification SendNewStockNotification(Item item);

        Notification SendAboutToExpireNotification();

        List<Notification> GetNotificationsForUser(User user);

        List<Tuple<int, string, int>> GetTop30Items();

        Dictionary<string, int> GetTop30Substances();
    }
}
