namespace UBB_SE_2026_923_2.Web.ViewModels
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using UBB_SE_2026_923_2.Models;

    public static class BasketStore
    {
        private static readonly ConcurrentDictionary<int, Dictionary<int, BasketEntry>> UserBaskets = new();

        public static void Restore(User? user)
        {
            if (user == null)
            {
                return;
            }

            if (UserBaskets.TryGetValue(user.Id, out Dictionary<int, BasketEntry>? basket))
            {
                user.Basket = CloneBasket(basket);
            }
        }

        public static void Save(User? user)
        {
            if (user == null)
            {
                return;
            }

            UserBaskets[user.Id] = CloneBasket(user.Basket);
        }

        public static void Clear(User? user)
        {
            if (user == null)
            {
                return;
            }

            UserBaskets.TryRemove(user.Id, out _);
        }

        private static Dictionary<int, BasketEntry> CloneBasket(Dictionary<int, BasketEntry> basket)
        {
            return basket.ToDictionary(
                entry => entry.Key,
                entry => new BasketEntry(entry.Value.Quantity, entry.Value.ExtraDiscountPercentage));
        }
    }
}
