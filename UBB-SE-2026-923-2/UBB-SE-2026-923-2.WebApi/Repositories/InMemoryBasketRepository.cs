namespace UBB_SE_2026_923_2.Repositories
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using UBB_SE_2026_923_2.Models;

    public class InMemoryBasketRepository : IBasketRepository
    {
        private readonly ConcurrentDictionary<int, Dictionary<int, BasketEntry>> baskets = new();

        public Dictionary<int, BasketEntry> GetBasket(int userId)
        {
            return this.baskets.TryGetValue(userId, out Dictionary<int, BasketEntry>? basket)
                ? CloneBasket(basket)
                : new Dictionary<int, BasketEntry>();
        }

        public void SaveBasket(int userId, Dictionary<int, BasketEntry> basket)
        {
            this.baskets[userId] = CloneBasket(basket);
        }

        public void ClearBasket(int userId)
        {
            this.baskets.TryRemove(userId, out _);
        }

        private static Dictionary<int, BasketEntry> CloneBasket(Dictionary<int, BasketEntry> basket)
        {
            return basket.ToDictionary(
                entry => entry.Key,
                entry => new BasketEntry(entry.Value.Quantity, entry.Value.ExtraDiscountPercentage));
        }
    }
}
