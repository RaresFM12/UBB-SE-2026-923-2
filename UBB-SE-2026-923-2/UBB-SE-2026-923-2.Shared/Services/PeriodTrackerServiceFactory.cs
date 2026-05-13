namespace UBB_SE_2026_923_2.Services
{
    using UBB_SE_2026_923_2.Repositories;

    public class PeriodTrackerServiceFactory : IPeriodTrackerServiceFactory
    {
        private readonly IUsersRepository usersRepository;
        private readonly IItemsRepository itemsRepository;
        private readonly RaresICurrentUserService currentUserService;
        private readonly IOrderService orderService;

        public PeriodTrackerServiceFactory(
            IUsersRepository usersRepository,
            IItemsRepository itemsRepository,
            RaresICurrentUserService currentUserService,
            IOrderService orderService)
        {
            this.usersRepository = usersRepository;
            this.itemsRepository = itemsRepository;
            this.currentUserService = currentUserService;
            this.orderService = orderService;
        }

        public IPeriodTrackerService CreatePeriodTrackerService()
        {
            return new PeriodTrackerService(this.usersRepository, this.currentUserService);
        }

        public IWellnessItemsService CreateWellnessItemsService()
        {
            return new WellnessItemsService(this.itemsRepository);
        }

        public IBasketService CreateBasketService()
        {
            return new BasketService(this.orderService);
        }
    }
}