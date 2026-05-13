namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;

    public class WellnessItemsService : IWellnessItemsService
    {
        private const string WellnessCategoryName = "wellness";

        private readonly IItemsRepository itemsRepository;

        public WellnessItemsService(IItemsRepository itemsRepository)
        {
            this.itemsRepository = itemsRepository;
        }

        public List<Item> GetWellnessItems()
        {
            return this.itemsRepository
                .GetAllItems()
                .Where(IsWellnessItem)
                .OrderBy(item => item.Id)
                .ToList();
        }

        private static bool IsWellnessItem(Item item)
        {
            return item.Category != null &&
                   item.Category.Equals(WellnessCategoryName, StringComparison.OrdinalIgnoreCase);
        }
    }
}