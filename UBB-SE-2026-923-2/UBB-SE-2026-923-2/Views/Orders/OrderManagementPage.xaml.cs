namespace UBB_SE_2026_923_2.Views.Orders;

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.ViewModels.Orders;

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
        if (e.Parameter is not IOrderService service)
        {
            return;
        }

        this.orderService = service;
        this.ViewModel = new(this.orderService);
        this.DataContext = this.ViewModel;

        this.ViewModel.ClickDetailButton += this.RedirectToPage;
    }

    private void RedirectToPage(Tuple<IOrderService, OrderDetail> args)
    {
        bool completeStatus = args.Item2.IsComplete;
        bool expiredStatus = args.Item2.IsExpired;

        if (!completeStatus && !expiredStatus)
        {
            this.Frame.Navigate(
                typeof(UBB_SE_2026_923_2.Views.Orders.EditableOrderDetailPage),
                    new Tuple<IOrderService, int>(args.Item1, args.Item2.OrderID));
        }
        else
        {
            this.Frame.Navigate(
                typeof(UBB_SE_2026_923_2.Views.Orders.NonEditableOrderDetailPage),
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
            return this.CompleteTemplate;
        }

        if (currentOrder.IsExpired)
        {
            return this.ExpiredTemplate;
        }

        return this.IncompleteTemplate;
    }
}