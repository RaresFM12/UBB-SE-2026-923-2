namespace UBB_SE_2026_923_2.Repositories
{
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;

    public interface IBasketRepository
    {
        Dictionary<int, BasketEntry> GetBasket(int userId);

        void SaveBasket(int userId, Dictionary<int, BasketEntry> basket);

        void ClearBasket(int userId);
    }
}
