namespace UBB_SE_2026_923_2.Services
{
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;

    public interface IWellnessItemsService
    {
        List<Item> GetWellnessItems();
    }
}