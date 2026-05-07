namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public interface ICheckoutViewModel
    {
        List<BasketItemViewModel> BasketItems { get; }

        string TotalPriceString { get; }

        ICommand PlaceOrderCommand { get; }

        event Action OrderPlacedSuccessfully;

        event Action<string> OrderPlacementFailed;
    }
}
