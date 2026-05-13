namespace UBB_SE_2026_923_2.Services
{
    public interface IBasketService
    {
        void AddToBasket(int itemId, int quantity, float extraDiscountPercentage = 0f);
    }
}