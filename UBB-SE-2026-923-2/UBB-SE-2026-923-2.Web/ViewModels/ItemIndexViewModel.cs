namespace UBB_SE_2026_923_2.Web.ViewModels
{
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;

    public class ItemIndexViewModel
    {
        public List<Item> Items { get; set; } = new List<Item>();

        public string SearchQuery { get; set; } = string.Empty;

        public bool ShowExpiredOnly { get; set; }
    }
}
