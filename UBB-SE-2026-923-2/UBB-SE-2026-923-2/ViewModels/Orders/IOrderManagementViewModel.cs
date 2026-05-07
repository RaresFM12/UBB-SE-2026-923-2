namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using UBB_SE_2026_923_2.Services;

    public interface IOrderManagementViewModel : INotifyPropertyChanged
    {
        ObservableCollection<OrderDetail> FilteredOrderList { get; set; }

        ICommand RedirectToDetailPageCommand { get; set; }

        string OrderIDInput { get; set; }

        string UserEmailInput { get; set; }

        bool IsIncompleteCheckbox { get; set; }

        bool IsExpiredCheckbox { get; set; }

        event Action<Tuple<IOrderService, OrderDetail>> ClickDetailButton;

        void OnClickDetailButton(OrderDetail chosenOrder);
    }
}
