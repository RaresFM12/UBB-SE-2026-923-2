namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using UBB_SE_2026_923_2.Command;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public class OrderHistoryViewModel : IOrderHistoryViewModel
    {
        private readonly IOrderService orderService;
        private readonly List<Order> baseOrderList;

        public ICommand CancelCommand { get; private set; }

        public ICommand ResubmitCommand { get; private set; }

        public ICommand GoToDetailPageCommand { get; private set; }

        public ICommand ModifyCommand { get; private set; }

        public ObservableCollection<Order> OrderHistory { get; private set; }

        private bool isExpiredCheckbox;

        public bool IsExpiredCheckbox
        {
            get => this.isExpiredCheckbox;
            set
            {
                this.isExpiredCheckbox = value;
                this.ReapplyFilters();
            }
        }

        public event Action<int> RedirectToDetailRequested;

        public event Action<int> RedirectToModifyRequested;

        public event Action<Order> CancelConfirmationRequested;

        public event Action<int> RedirectToResubmitRequested;

        public OrderHistoryViewModel(IOrderService injectedOrderService)
        {
            this.orderService = injectedOrderService;
            this.CancelCommand = new RelayCommandWithOneParameter<Order>(this.CancelOrderCommand);
            this.ResubmitCommand = new RelayCommandWithOneParameter<Order>(this.ResubmitExpiredOrderCommand);
            this.GoToDetailPageCommand = new RelayCommandWithOneParameter<Order>(this.DisplayOrderDetailCommand);
            this.ModifyCommand = new RelayCommandWithOneParameter<Order>(this.ModifyOrderCommand);
            this.OrderHistory = new ObservableCollection<Order>();
            this.baseOrderList = new List<Order>();

            this.LoadOrders();
        }

        private void LoadOrders()
        {
            if (this.orderService.ActiveUser == null)
            {
                return;
            }

            this.orderService.ExpireOverdueOrders();
            int clientId = this.orderService.ActiveUser.Id;
            List<Order> userOrders = this.orderService.OrdersRepository.GetOrdersOfClient(clientId);
            foreach (Order currentOrder in SortOrders(userOrders))
            {
                this.OrderHistory.Add(currentOrder);
                this.baseOrderList.Add(currentOrder);
            }
        }

        private void CancelOrderCommand(Order orderToCancel)
        {
            this.CancelConfirmationRequested?.Invoke(orderToCancel);
        }

        private void ResubmitExpiredOrderCommand(Order orderToResubmit)
        {
            this.RedirectToResubmitRequested?.Invoke(orderToResubmit.Id);
        }

        private void DisplayOrderDetailCommand(Order orderToModify)
        {
            this.RedirectToDetailRequested?.Invoke(orderToModify.Id);
        }

        private void ModifyOrderCommand(Order orderToModify)
        {
            this.RedirectToModifyRequested?.Invoke(orderToModify.Id);
        }

        private void ReapplyFilters()
        {
            List<Order> intermediateFilteredOrderList = new List<Order>(this.baseOrderList);

            if (this.isExpiredCheckbox)
            {
                intermediateFilteredOrderList = intermediateFilteredOrderList
                    .Where(order => order.IsExpired)
                    .ToList();
            }

            this.OrderHistory.Clear();
            foreach (Order resultOrder in SortOrders(intermediateFilteredOrderList))
            {
                this.OrderHistory.Add(resultOrder);
            }
        }

        public void CancelOrder(Order orderToCancel)
        {
            this.orderService.CancelOrder(orderToCancel.Id);

            orderToCancel.IsExpired = true;
            foreach (Order currOrder in this.baseOrderList)
            {
                if (currOrder.Id == orderToCancel.Id)
                {
                    currOrder.IsExpired = true;
                }
            }

            this.ReapplyFilters();
        }

        private static List<Order> SortOrders(IEnumerable<Order> orders)
        {
            return orders
                .OrderByDescending(order => order.PickUpDate)
                .ThenByDescending(order => order.Id)
                .ToList();
        }
    }
}
