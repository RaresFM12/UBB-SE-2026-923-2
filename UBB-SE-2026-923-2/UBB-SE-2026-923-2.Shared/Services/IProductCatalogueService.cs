namespace UBB_SE_2026_923_2.Services
{
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;

    public interface IProductCatalogueService
    {
        List<Item> GetItems(
            string search,
            List<string> categories = null,
            List<(float minimum, float maximum)> priceRanges = null,
            string stockFilter = null,
            bool? discounted = null,
            List<string> substances = null,
            bool ascending = true,
            int page = 0,
            int pageSize = ProductCatalogueService.DefaultPageSize,
            string sortBy = null);
    }
}
