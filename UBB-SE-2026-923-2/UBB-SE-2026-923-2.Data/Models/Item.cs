namespace UBB_SE_2026_923_2.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text.Json.Serialization;

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
            this.Name = string.Empty;
            this.Producer = string.Empty;
            this.Category = string.Empty;
            this.ImagePath = ImagePathDefault;
            this.Label = string.Empty;
            this.Description = string.Empty;
            this.ActiveSubstances = new Dictionary<string, float>();
            this.Batches = new Dictionary<DateOnly, int>();
        }

        public Item(int id, string name, string producer, string category,
                    float price, int numberOfPills,
                    string label = "", string description = "", string imagePath = ImagePathDefault,
                    float discount = 0f)
            : this()
        {
            this.Id = id;
            this.Name = name;
            this.Producer = producer;
            this.Price = price;
            this.NumberOfPills = numberOfPills;
            this.Category = category;
            this.ImagePath = imagePath;
            this.Quantity = 0;
            this.Label = label;
            this.Description = description;
            this.DiscountPercentage = discount;
        }

        public Item(int id, string name, string producer, string category,
                    float price, int numberOfPills,
                    string label = "", string description = "", string imagePath = ImagePathDefault,
                    float discount = 0f, int quantity = 0)
            : this(id, name, producer, category, price, numberOfPills, label, description, imagePath, discount)
        {
            this.Quantity = quantity;
        }

        public Item(string name, string producer, string category,
            float price, int numberOfPills,
            int quantity = 0,
            string label = "", string description = "", string imagePath = ImagePathDefault,
            float discount = 0f)
            : this(0, name, producer, category, price, numberOfPills, label, description, imagePath, discount)
        {
            this.Quantity = quantity;
        }

        public Item(string name, string producer, string category,
                    float price, int numberOfPills,
                    Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches,
                    int quantity = 0,
                    string label = "", string description = "", string imagePath = ImagePathDefault,
                    float discount = 0f)
            : this(name, producer, category, price, numberOfPills, quantity, label, description, imagePath, discount)
        {
            this.ActiveSubstances = activeSubstances;
            this.Batches = batches;
        }

        public void AddActiveSubstanceToItem(string newSubstanceName, float concentration)
        {
            if (this.ActiveSubstances.ContainsKey(newSubstanceName))
            {
                throw new ArgumentException(newSubstanceName + "is already inside the medication");
            }

            this.ActiveSubstances[newSubstanceName] = concentration;
        }

        public void ChangeActiveSubstanceConcentration(string newSubstanceName, float newConcentration)
        {
            if (!this.ActiveSubstances.ContainsKey(newSubstanceName))
            {
                throw new ArgumentException(newSubstanceName + "is not inside the medication");
            }

            this.ActiveSubstances[newSubstanceName] = newConcentration;
        }

        public void RemoveActiveSubstanceFromItem(string substanceName)
        {
            if (!this.ActiveSubstances.ContainsKey(substanceName))
            {
                throw new ArgumentException(substanceName + "is not inside the medication");
            }

            this.ActiveSubstances.Remove(substanceName);
        }

        public void AddNewBatchToItem(DateOnly newExpirationDate, int numberOfPacks)
        {
            if (this.Batches.ContainsKey(newExpirationDate))
            {
                this.Batches[newExpirationDate] += numberOfPacks;
                this.Quantity += numberOfPacks;
                return;
            }

            this.Batches[newExpirationDate] = numberOfPacks;
            this.Quantity += numberOfPacks;
        }

        public void ChangeNumberOfPacksForBatch(DateOnly expirationDate, int newNumberOfPacks)
        {
            int oldNumberOfPacks = this.Batches[expirationDate];

            if (!this.Batches.ContainsKey(expirationDate))
            {
                throw new ArgumentException("A batch with expiration date " + expirationDate.ToString() + " doesn't exist");
            }

            this.Batches[expirationDate] = newNumberOfPacks;
            this.Quantity += newNumberOfPacks - oldNumberOfPacks;
        }

        public void RemoveBatchFromItem(DateOnly expirationDate)
        {
            if (!this.Batches.ContainsKey(expirationDate))
            {
                throw new ArgumentException("A batch with expiration date " + expirationDate.ToString() + " doesn't exist");
            }

            this.Quantity -= this.Batches[expirationDate];
            this.Batches.Remove(expirationDate);
        }

        public void RemoveQuantityFromItem(int quantityToRemove, DateOnly dateAfter)
        {
            List<DateOnly> sortedExpirationDates = this.Batches.Keys.ToList<DateOnly>();
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

                if (remainingQuantity > this.Batches[sortedExpirationDates[indexForDate]])
                {
                    remainingQuantity -= this.Batches[sortedExpirationDates[indexForDate]];
                    this.RemoveBatchFromItem(sortedExpirationDates[indexForDate]);
                    indexForDate++;
                    continue;
                }

                int newBatchQuantity = this.Batches[sortedExpirationDates[indexForDate]] - remainingQuantity;
                this.ChangeNumberOfPacksForBatch(sortedExpirationDates[indexForDate], newBatchQuantity);
                remainingQuantity = 0;
                indexForDate++;
            }
        }

        public int GetQuantityAtSpecifiedDate(DateOnly date)
        {
            int validatedQuantity = 0;

            foreach (KeyValuePair<DateOnly, int> batchEntry in this.Batches)
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