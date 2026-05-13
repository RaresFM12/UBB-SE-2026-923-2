namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;

    public class PrescriptionService : IPrescriptionService
    {
        private const float MinimumDiscount = 0f;
        private const float MaximumDiscount = 1f;
        private const float PercentageDivisor = 100f;
        private const int SingleBoxQuantity = 1;
        private const int NoCandidateItemId = -1;
        private const int NoCandidateQuantity = -1;
        private const int EmptyQuantity = 0;
        private const int FallbackRequiredPills = 1;

        private static readonly char[] MedicineSeparators = new[] { ',', ';', '\n', '\r' };

        private readonly IItemsRepository itemsRepository;
        private readonly IEvaluationsRepository evaluationsRepository;

        public PrescriptionService(IItemsRepository itemsRepository, IEvaluationsRepository evaluationsRepository)
        {
            this.itemsRepository = itemsRepository;
            this.evaluationsRepository = evaluationsRepository;
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
            if (string.IsNullOrWhiteSpace(prescriptionId) || !int.TryParse(prescriptionId.Trim(), out int evaluationId))
            {
                throw new ArgumentException("Invalid prescription ID");
            }

            MedicalEvaluation? evaluation = this.evaluationsRepository
                .GetAllEvaluations()
                .FirstOrDefault(record => record.EvaluationID == evaluationId);

            if (evaluation == null || string.IsNullOrWhiteSpace(evaluation.MedicationsList))
            {
                throw new ArgumentException("Invalid prescription ID");
            }

            userDiscounts ??= new Dictionary<int, float>();
            List<string> medicineNames = ParseMedicineNames(evaluation.MedicationsList);
            List<Item> allItems = this.itemsRepository.GetAllItems();
            Dictionary<int, int> mergedItems = new Dictionary<int, int>();

            foreach (string medicineName in medicineNames)
            {
                Dictionary<int, int> filledItems = this.FillSingleMedicine(medicineName, allItems, userDiscounts);
                foreach (KeyValuePair<int, int> entry in filledItems)
                {
                    if (mergedItems.ContainsKey(entry.Key))
                    {
                        mergedItems[entry.Key] += entry.Value;
                    }
                    else
                    {
                        mergedItems[entry.Key] = entry.Value;
                    }
                }
            }

            if (mergedItems.Count == 0)
            {
                throw new ArgumentException("Medicine couldn't be retrieved");
            }

            return mergedItems;
        }

        private static List<string> ParseMedicineNames(string medicationsList)
        {
            return medicationsList
                .Split(MedicineSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim())
                .Where(part => part.Length > 0)
                .ToList();
        }

        private Dictionary<int, int> FillSingleMedicine(
            string medicineName,
            List<Item> allItems,
            Dictionary<int, float> userDiscounts)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();

            List<Item> preferredItems = this.itemsRepository.GetItemsByName(medicineName);
            if (preferredItems.Count == 0)
            {
                return result;
            }

            Item preferredItem = preferredItems[0];
            int requiredPills = preferredItem.NumberOfPills > 0 ? preferredItem.NumberOfPills : FallbackRequiredPills;

            var exactMatches = allItems
                .Where(item => item.Name == medicineName && item.NumberOfPills == requiredPills)
                .OrderBy(item => item.Price)
                .ToList();

            foreach (var exactMatch in exactMatches)
            {
                if (exactMatch.Quantity != EmptyQuantity)
                {
                    result.Add(exactMatch.Id, SingleBoxQuantity);
                    return result;
                }
            }

            var exactSubstitutes = allItems
                .Where(item => item.NumberOfPills == requiredPills && this.SubstancesMatch(preferredItem, item))
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
                    result.Add(cheapestItemID, SingleBoxQuantity);
                    return result;
                }
            }

            var multipliedSubstitutes = allItems
                .Where(item => item.NumberOfPills < requiredPills && this.SubstancesMatch(preferredItem, item))
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
                        int multiplier = (int)Math.Ceiling((double)requiredPills / currItem.NumberOfPills);

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
                    result.Add(cheapestItemId, cheapestItemQuantity);
                    return result;
                }
            }

            return result;
        }

        public Dictionary<int, int> GetCheapestPrescriptionItems(string prescriptionName, int requiredPills)
        {
            Dictionary<int, int> items = new Dictionary<int, int>();
            List<Item> allItems = this.itemsRepository.GetAllItems();

            var exactMatches = allItems
                .Where(item => item.Name == prescriptionName && item.NumberOfPills == requiredPills)
                .OrderBy(item => item.Price)
                .ToList();

            if (exactMatches.Count != 0)
            {
                var entry = exactMatches.FirstOrDefault(item => item.Quantity != EmptyQuantity);
                if (entry != null)
                {
                    items.Add(entry.Id, SingleBoxQuantity);
                    return items;
                }
            }

            List<Item> preferredItems = this.itemsRepository.GetItemsByName(prescriptionName);
            if (preferredItems.Count == 0)
            {
                return items;
            }

            Item preferredItem = preferredItems[0];

            var exactSubstitutes = allItems
                .Where(item => item.NumberOfPills == requiredPills && this.SubstancesMatch(preferredItem, item))
                .OrderBy(item => item.Price)
                .ToList();

            if (exactSubstitutes.Count != 0)
            {
                items.Add(exactSubstitutes[0].Id, SingleBoxQuantity);
                return items;
            }

            var multipliedSubstitutes = allItems
                .Where(item => item.NumberOfPills < requiredPills && this.SubstancesMatch(preferredItem, item))
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
