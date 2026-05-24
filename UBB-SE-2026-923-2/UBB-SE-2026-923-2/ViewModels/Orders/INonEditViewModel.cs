namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface INonEditViewModel
    {
        List<ItemDetail> OrderItems { get; }

        string TotalPriceString { get; }

        string UserEmail { get; }

        string StatusString { get; }

        DateOnly PickUpDate { get; }

        string PickUpDateString { get; }

        DateOnly ExpirationDate { get; }

        string ExpirationDateString { get; }
    }
}
