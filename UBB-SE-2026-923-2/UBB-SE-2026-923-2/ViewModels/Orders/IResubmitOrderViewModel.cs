namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IResubmitOrderViewModel
    {
        int ShownOrderID { get; set; }

        List<ItemDetail> OrderItems { get; }

        string TotalPriceString { get; }
    }
}