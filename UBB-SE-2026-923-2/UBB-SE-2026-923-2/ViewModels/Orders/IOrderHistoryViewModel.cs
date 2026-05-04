using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    public interface IOrderHistoryViewModel
    {
        ICommand CancelCommand { get; }
        ICommand ResubmitCommand { get; }
        ICommand GoToDetailPageCommand { get; }

        ObservableCollection<Order> OrderHistory { get; }

        bool IsExpiredCheckbox { get; set; }

        event Action<int> RedirectToDetailRequested;
        event Action<Order> CancelConfirmationRequested;
        event Action<int> RedirectToResubmitRequested;

        void CancelOrder(Order orderToCancel);
    }
}