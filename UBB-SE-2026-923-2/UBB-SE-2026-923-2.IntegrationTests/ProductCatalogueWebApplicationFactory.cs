using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.ViewModels.Orders;

namespace UBB_SE_2026_923_2.IntegrationTests;

public sealed class ProductCatalogueWebApplicationFactory : WebApplicationFactory<UBB_SE_2026_923_2.Web.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IProductCatalogueService>();
            services.RemoveAll<IOrderService>();

            services.AddSingleton<IProductCatalogueService, FakeProductCatalogueService>();
            services.AddSingleton<IOrderService, FakeOrderService>();
        });
    }
}

internal sealed class FakeProductCatalogueService : IProductCatalogueService
{
    public List<Item> GetItems(
        string search,
        List<string> categories = null,
        List<(float minimum, float maximum)> priceRanges = null,
        string stockFilter = null,
        bool? discounted = null,
        List<string> substances = null,
        bool ascending = true,
        int page = 0,
        int pageSize = ProductCatalogueService.DefaultPageSize,
        string sortBy = null)
    {
        return new List<Item>();
    }
}

internal sealed class FakeOrderService : IOrderService
{
    public ISubstancesRepository SubstancesRepository => throw new NotImplementedException();

    public IItemsRepository ItemsRepository => throw new NotImplementedException();

    public IUsersRepository UsersRepository => throw new NotImplementedException();

    public IOrdersRepository OrdersRepository => throw new NotImplementedException();

    public User ActiveUser => throw new NotImplementedException();

    public void PlaceOrderFromBasket(DateOnly chosenPickUpDate)
    {
    }

    public void CompleteOrder(int orderId, Dictionary<int, Tuple<int, float>> updatedQuantities)
    {
    }

    public void ModifyIncompleteOrder(int orderIdToModify, Dictionary<int, Tuple<int, float>> updatedQuantities, DateOnly updatedPickUpDate)
    {
    }

    public void ResubmitExpiredOrder(int orderIdToResubmit, DateOnly chosenPickUpDate)
    {
    }

    public void CancelOrder(int orderId)
    {
    }

    public void ExpireOverdueOrders()
    {
    }

    public void AddToBasket(int itemId, int quantityToBuy)
    {
    }

    public void AddItemToBasket(int itemId, int quantityToBuy, float extraDiscountPercentage = 0f)
    {
    }

    public void UpdateBasketItemQuantity(int itemId, int newQuantityToBuy)
    {
    }

    public void RemoveFromBasket(int itemIdToRemove)
    {
    }

    public Dictionary<int, int> FillBasketFromPrescription(string prescriptionId)
    {
        return new Dictionary<int, int>();
    }

    public List<BasketItemViewModel> GetBasketItems()
    {
        return new List<BasketItemViewModel>();
    }

    public void ApplyPrescriptionToBasket(string prescriptionId)
    {
    }

    public void RecalculateBasketItemPrices(BasketItemViewModel basketItem)
    {
    }

    public Tuple<float, float> CalculateBasketTotalSum(IEnumerable<BasketItemViewModel> basketItems)
    {
        return new Tuple<float, float>(0f, 0f);
    }
}