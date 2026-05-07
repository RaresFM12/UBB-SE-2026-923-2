namespace UBB_SE_2026_923_2.Services
{
    public interface IPeriodTrackerServiceFactory
    {
        IPeriodTrackerService CreatePeriodTrackerService();

        IWellnessItemsService CreateWellnessItemsService();

        IBasketService CreateBasketService();
    }
}
