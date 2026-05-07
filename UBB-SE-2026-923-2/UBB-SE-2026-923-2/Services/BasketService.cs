namespace UBB_SE_2026_923_2.Services
{
    public class BasketService : IBasketService
    {
        private readonly IOrderService orderService;

        public BasketService(IOrderService orderService)
        {
            this.orderService = orderService;
        }

        public void AddToBasket(int itemId, int quantity, float extraDiscountPercentage = 0f)
        {
            this.orderService.AddItemToBasket(itemId, quantity, extraDiscountPercentage);
        }
    }
}