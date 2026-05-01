using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.ViewModels.Orders;

namespace UBB_SE_2026_923_2.Views.Orders;
public sealed partial class OrderManagementPage : Page
{
    private IOrderService orderService;
    public OrderManagementViewModel ViewModel { get; set; }

    public OrderManagementPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        orderService = (OrderService)e.Parameter;
        ViewModel = new (orderService);
        DataContext = ViewModel;

        ViewModel.ClickDetailButton += RedirectToPage;
    }

    private void RedirectToPage(Tuple<IOrderService, OrderDetail> args)
    {
        bool completeStatus = args.Item2.IsComplete;
        bool expiredStatus = args.Item2.IsExpired;

        if (!completeStatus && !expiredStatus)
        {
            Frame.Navigate(typeof(UBB_SE_2026_923_2.Views.Orders.EditableOrderDetailPage),
                    new Tuple<IOrderService, int>(args.Item1, args.Item2.OrderID));
        }
        else
        {
            Frame.Navigate(typeof(UBB_SE_2026_923_2.Views.Orders.NonEditableOrderDetailPage),
                    new Tuple<IOrderService, int>(args.Item1, args.Item2.OrderID));
        }
    }
}

public partial class OrderDetailTemplateSelector : DataTemplateSelector
{
    public DataTemplate IncompleteTemplate { get; set; }
    public DataTemplate ExpiredTemplate { get; set; }
    public DataTemplate CompleteTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        OrderDetail currentOrder = (OrderDetail)item;

        if (currentOrder.IsComplete)
        {
            return CompleteTemplate;
        }
        if (currentOrder.IsExpired)
        {
            return ExpiredTemplate;
        }

        return IncompleteTemplate;
    }
}