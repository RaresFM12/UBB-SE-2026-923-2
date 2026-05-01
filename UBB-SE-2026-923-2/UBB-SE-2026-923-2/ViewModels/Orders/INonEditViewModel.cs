using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    public interface INonEditViewModel
    {
        List<ItemDetail> OrderItems { get; }
        string TotalPriceString { get; }
        string StatusString { get; }
        DateOnly PickUpDate { get; }
        string PickUpDateString { get; }
    }
}
