using System;
using System.Collections.Generic;
using System.Linq;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private const float MinimumDiscount = 0f;
        private const float MaximumDiscount = 1f;
        private const float PercentageDivisor = 100f;
        private const string TestPrescriptionId = "testPrescription";
        private const string DefaultPrescriptionItemName = "Nurofen Express";
        private const int DefaultPrescriptionPills = 40;
        private const int SingleBoxQuantity = 1;
        private const int NoCandidateItemId = -1;
        private const int NoCandidateQuantity = -1;
        private const int EmptyQuantity = 0;

        private readonly IItemsRepository itemsRepository;

        public PrescriptionService(IItemsRepository itemsRepository)
        {
            this.itemsRepository = itemsRepository;
        }

        private static float NormalizeDiscount(float discount)
        {
            if (discount > MaximumDiscount)
            {
                discount /= PercentageDivisor;
            }

            if (discount < MinimumDiscount)
            {
                return MinimumDiscount;
            }

            if (discount > MaximumDiscount)
            {
                return MaximumDiscount;
            }

            return discount;
        }

        private bool SubstancesMatch(Item preferredItem, Item candidate)
        {
            if (candidate.ActiveSubstances.Count != preferredItem.ActiveSubstances.Count)
            {
                return false;
            }

            foreach (var substance in preferredItem.ActiveSubstances)
            {
                if (!candidate.ActiveSubstances.ContainsKey(substance.Key))
                {
                    return false;
                }
                if (candidate.ActiveSubstances[substance.Key] != substance.Value)
                {
                    return false;
                }
            }

            return true;
        }

        public Dictionary<int, int> GetItemsFromPrescription(string prescriptionId, Dictionary<int, float> userDiscounts)
        {
            Dictionary<int, int> items = new Dictionary<int, int>();

            if (string.IsNullOrWhiteSpace(prescriptionId) || !prescriptionId.Equals(TestPrescriptionId))
            {
                throw new ArgumentException("Invalid prescription ID");
            }

            string itemName = DefaultPrescriptionItemName;
            int nrOfRequiredPills = DefaultPrescriptionPills;
            userDiscounts ??= new Dictionary<int, float>();

            List<Item> preferredItems = itemsRepository.GetItemsByName(itemName);
            if (preferredItems.Count == 0)
            {
                throw new ArgumentException("Medicine couldn't be retrieved");
            }
            Item preferredItem = preferredItems[0];
            int numberOfRequiredSubstances = preferredItem.ActiveSubstances.Count;

            List<Item> allItems = itemsRepository.GetAllItems();

            var exactMatches = allItems
                .Where(item => item.Name == itemName && item.NumberOfPills == nrOfRequiredPills)
                .OrderBy(item => item.Price)
                .ToList();

            foreach (var exactMatch in exactMatches)
            {
                if (exactMatch.Quantity != EmptyQuantity)
                {
                    items.Add(exactMatch.Id, SingleBoxQuantity);
                    return items;
                }
            }

            var exactSubstitutes = allItems
                .Where(item => item.NumberOfPills == nrOfRequiredPills && SubstancesMatch(preferredItem, item))
                .OrderBy(item => item.Price)
                .ToList();

            if (exactSubstitutes.Count != 0)
            {
                int cheapestItemID = NoCandidateItemId;
                float cheapestPrice = float.MaxValue;

                foreach (var currItem in exactSubstitutes)
                {
                    if (currItem.Quantity != EmptyQuantity)
                    {
                        float itemDiscount = NormalizeDiscount(currItem.DiscountPercentage);
                        float userDiscount = userDiscounts.ContainsKey(currItem.Id) ? NormalizeDiscount(userDiscounts[currItem.Id]) : MinimumDiscount;
                        float finalPrice = currItem.Price * (1 - itemDiscount) * (1 - userDiscount);

                        if (finalPrice < cheapestPrice)
                        {
                            cheapestPrice = finalPrice;
                            cheapestItemID = currItem.Id;
                        }
                    }
                }

                if (cheapestItemID != NoCandidateItemId)
                {
                    items.Add(cheapestItemID, SingleBoxQuantity);
                    return items;
                }
            }

            var multipliedSubstitutes = allItems
                .Where(item => item.NumberOfPills < nrOfRequiredPills && SubstancesMatch(preferredItem, item))
                .OrderBy(item => item.Price)
                .ToList();

            if (multipliedSubstitutes.Count != 0)
            {
                int cheapestItemId = NoCandidateItemId;
                int cheapestItemQuantity = NoCandidateQuantity;
                float cheapestPrice = float.MaxValue;

                foreach (var currItem in multipliedSubstitutes)
                {
                    if (currItem.Quantity != EmptyQuantity)
                    {
                        int multiplier = (int)Math.Ceiling((double)nrOfRequiredPills / currItem.NumberOfPills);

                        if (currItem.Quantity >= multiplier)
                        {
                            float itemDiscount = NormalizeDiscount(currItem.DiscountPercentage);
                            float userDiscount = userDiscounts.ContainsKey(currItem.Id) ? NormalizeDiscount(userDiscounts[currItem.Id]) : MinimumDiscount;
                            float finalPrice = currItem.Price * multiplier * (1 - itemDiscount) * (1 - userDiscount);

                            if (finalPrice < cheapestPrice)
                            {
                                cheapestPrice = finalPrice;
                                cheapestItemId = currItem.Id;
                                cheapestItemQuantity = multiplier;
                            }
                        }
                    }
                }

                if (cheapestItemId != NoCandidateItemId && cheapestItemQuantity != NoCandidateQuantity)
                {
                    items.Add(cheapestItemId, cheapestItemQuantity);
                    return items;
                }
            }

            throw new ArgumentException("Medicine couldn't be retrieved");
        }

        public Dictionary<int, int> GetCheapestPrescriptionItems(string prescriptionName, int requiredPills)
        {
            Dictionary<int, int> items = new Dictionary<int, int>();
            List<Item> allItems = itemsRepository.GetAllItems();

            var exactMatches = allItems
                .Where(item => item.Name == prescriptionName && item.NumberOfPills == requiredPills)
                .OrderBy(item => item.Price)
                .ToList();

            if (exactMatches.Count != 0)
            {
                var entry = exactMatches.FirstOrDefault(i => i.Quantity != EmptyQuantity);
                if (entry != null)
                {
                    items.Add(entry.Id, SingleBoxQuantity);
                    return items;
                }
            }

            List<Item> preferredItems = itemsRepository.GetItemsByName(prescriptionName);
            if (preferredItems.Count == 0)
            {
                return items;
            }
            Item preferredItem = preferredItems[0];

            // 2. Exact Substitutes
            var exactSubstitutes = allItems
                .Where(i => i.NumberOfPills == requiredPills && SubstancesMatch(preferredItem, i))
                .OrderBy(item => item.Price)
                .ToList();

            if (exactSubstitutes.Count != 0)
            {
                items.Add(exactSubstitutes[0].Id, SingleBoxQuantity);
                return items;
            }

            var multipliedSubstitutes = allItems
                .Where(i => i.NumberOfPills < requiredPills && SubstancesMatch(preferredItem, i))
                .OrderBy(item => item.Price)
                .ToList();

            if (multipliedSubstitutes.Count != 0)
            {
                var currentItem = multipliedSubstitutes[0];
                int multiplier = (int)Math.Ceiling((double)requiredPills / currentItem.NumberOfPills);
                items.Add(currentItem.Id, multiplier);
                return items;
            }

            return items;
        }
    }
}