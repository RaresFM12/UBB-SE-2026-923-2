using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace UBB_SE_2026_923_2.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Producer { get; set; }
        public float Price { get; set; }
        public string Category { get; set; }
        public string ImagePath { get; set; }
        public int NumberOfPills { get; set; }
        // Setter opened up so System.Text.Json can rehydrate Quantity over HTTP.
        public int Quantity { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public float DiscountPercentage { get; set; }

        private const string ImagePathDefault = "..\\..\\Assets\\placeholder.png";

        // Legacy in-memory views — not persisted. Phase 2 will migrate callers
        // onto the navigation collections below.
        [NotMapped]
        public Dictionary<string, float> ActiveSubstances { get; set; }
        [NotMapped]
        public Dictionary<DateOnly, int> Batches { get; set; }

        // ---- EF Core navigation collections (persisted) ----
        // [JsonIgnore]: server projects these into the legacy dictionaries
        // (ActiveSubstances/Batches) before returning, and they create cycles
        // back to Item over the wire.
        [JsonIgnore]
        public ICollection<ItemSubstance> ItemSubstanceEntries { get; set; } = new List<ItemSubstance>();
        [JsonIgnore]
        public ICollection<ItemBatch> ItemBatchEntries { get; set; } = new List<ItemBatch>();

        public Item()
        {
            Name = string.Empty;
            Producer = string.Empty;
            Category = string.Empty;
            ImagePath = ImagePathDefault;
            Label = string.Empty;
            Description = string.Empty;
            ActiveSubstances = new Dictionary<string, float>();
            Batches = new Dictionary<DateOnly, int>();
        }

        public Item(int id, string name, string producer, string category,
                    float price, int numberOfPills,
                    string label = "", string description = "", string imagePath = ImagePathDefault,
                    float discount = 0f)
            : this()
        {
            Id = id;
            Name = name;
            Producer = producer;
            Price = price;
            NumberOfPills = numberOfPills;
            Category = category;
            ImagePath = imagePath;
            Quantity = 0;
            Label = label;
            Description = description;
            DiscountPercentage = discount;
        }

        public Item(int id, string name, string producer, string category,
                    float price, int numberOfPills,
                    string label = "", string description = "", string imagePath = ImagePathDefault,
                    float discount = 0f, int quantity = 0)
            : this(id, name, producer, category, price, numberOfPills, label, description, imagePath, discount)
        {
            Quantity = quantity;
        }

        public Item(string name, string producer, string category,
            float price, int numberOfPills,
            int quantity = 0,
            string label = "", string description = "", string imagePath = ImagePathDefault,
            float discount = 0f)
            : this(0, name, producer, category, price, numberOfPills, label, description, imagePath, discount)
        {
            Quantity = quantity;
        }

        public Item(string name, string producer, string category,
                    float price, int numberOfPills,
                    Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches,
                    int quantity = 0,
                    string label = "", string description = "", string imagePath = ImagePathDefault,
                    float discount = 0f)
            : this(name, producer, category, price, numberOfPills, quantity, label, description, imagePath, discount)
        {
            ActiveSubstances = activeSubstances;
            Batches = batches;
        }

        public void AddActiveSubstanceToItem(string newSubstanceName, float concentration)
        {
            if (ActiveSubstances.ContainsKey(newSubstanceName))
            {
                throw new ArgumentException(newSubstanceName + "is already inside the medication");
            }

            ActiveSubstances[newSubstanceName] = concentration;
        }

        public void ChangeActiveSubstanceConcentration(string newSubstanceName, float newConcentration)
        {
            if (!ActiveSubstances.ContainsKey(newSubstanceName))
            {
                throw new ArgumentException(newSubstanceName + "is not inside the medication");
            }

            ActiveSubstances[newSubstanceName] = newConcentration;
        }

        public void RemoveActiveSubstanceFromItem(string substanceName)
        {
            if (!ActiveSubstances.ContainsKey(substanceName))
            {
                throw new ArgumentException(substanceName + "is not inside the medication");
            }

            ActiveSubstances.Remove(substanceName);
        }


        public void AddNewBatchToItem(DateOnly newExpirationDate, int numberOfPacks)
        {
            if (Batches.ContainsKey(newExpirationDate))
            {
                Batches[newExpirationDate] += numberOfPacks;
                Quantity += numberOfPacks;
                return;
            }

            Batches[newExpirationDate] = numberOfPacks;
            this.Quantity += numberOfPacks;
        }

        public void ChangeNumberOfPacksForBatch(DateOnly expirationDate, int newNumberOfPacks)
        {
            int oldNumberOfPacks = Batches[expirationDate];

            if (!Batches.ContainsKey(expirationDate))
            {
                throw new ArgumentException("A batch with expiration date " + expirationDate.ToString() + " doesn't exist");
            }

            Batches[expirationDate] = newNumberOfPacks;
            Quantity += newNumberOfPacks - oldNumberOfPacks;
        }

        public void RemoveBatchFromItem(DateOnly expirationDate)
        {
            if (!Batches.ContainsKey(expirationDate))
            {
                throw new ArgumentException("A batch with expiration date " + expirationDate.ToString() + " doesn't exist");
            }

            Quantity -= Batches[expirationDate];
            Batches.Remove(expirationDate);
        }
        public void RemoveQuantityFromItem(int quantityToRemove, DateOnly dateAfter)
        {
            List<DateOnly> sortedExpirationDates = Batches.Keys.ToList<DateOnly>();
            sortedExpirationDates.Sort();

            int indexForDate = 0;
            int remainingQuantity = quantityToRemove;
            while (remainingQuantity > 0)
            {
                if (sortedExpirationDates[indexForDate] < dateAfter)
                {
                    indexForDate++;
                    continue;
                }

                if (remainingQuantity > Batches[sortedExpirationDates[indexForDate]])
                {
                    remainingQuantity -= Batches[sortedExpirationDates[indexForDate]];
                    RemoveBatchFromItem(sortedExpirationDates[indexForDate]);
                    indexForDate++;
                    continue;
                }

                int newBatchQuantity = Batches[sortedExpirationDates[indexForDate]] - remainingQuantity;
                ChangeNumberOfPacksForBatch(sortedExpirationDates[indexForDate], newBatchQuantity);
                remainingQuantity = 0;
                indexForDate++;
            }
        }

        public int GetQuantityAtSpecifiedDate(DateOnly date)
        {
            int validatedQuantity = 0;

            foreach (KeyValuePair<DateOnly, int> batchEntry in Batches)
            {
                DateOnly currentBatchExpirationDate = batchEntry.Key;
                int currentBatchQuantity = batchEntry.Value;

                if (date < currentBatchExpirationDate)
                {
                    validatedQuantity += currentBatchQuantity;
                }
            }

            return validatedQuantity;
        }
    }
}
