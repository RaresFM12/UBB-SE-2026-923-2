namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;

    public interface IItemsRepository
    {
        public const string ImagePathDefault = "..\\..\\Assets\\placeholder.png";

        void AddItem(string name, string producer, string category,
                    float price, int numberOfPills,
                    string label = "", string description = "", string imagePath = ImagePathDefault,
                    float discount = 0f);

        void AddItemWithQuantity(string name, string producer, string category,
                    float price, int numberOfPills,
                    int quantity, Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches,
                    string label = "", string description = "", string imagePath = ImagePathDefault,
                    float discount = 0f);

        void RemoveItemById(int itemIdToRemove);

        Item GetItemById(int itemId);

        List<Item> GetAllItems();

        List<Item> GetItemsByName(string name);

        void UpdateItemById(Item newItem);

        bool ItemExists(int itemId);

        List<Tuple<int, string, int>> GetTop30Items();
    }
}