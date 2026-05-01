using System;
using System.Collections.Generic;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    public interface IItemsRepository
    {
        public const string ImagePathDefault = "..\\..\\Assets\\placeholder.png";
        void AddItem(string name, string producer, string category,
                    float price, int nrOfPills,
                    string label = "", string description = "", string imagePath = ImagePathDefault,
                    float discount = 0f);

        void AddItemWithQuantity(string name, string producer, string category,
                    float price, int nrOfPills,
                    int quantity, Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches,
                    string label = "", string description = "", string imagePath = ImagePathDefault,
                    float discount = 0f);

        void RemoveItemById(int idToBeRemoved);
        Item GetItemById(int id);
        List<Item> GetAllItems();
        List<Item> GetItemsByName(string name);
        void UpdateItemById(Item newItem);
        bool ItemExists(int id);
        List<Tuple<int, string, int>> GetTop30Items();
    }
}

