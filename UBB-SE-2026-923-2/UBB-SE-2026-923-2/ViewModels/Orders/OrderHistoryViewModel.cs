using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using UBB_SE_2026_923_2.Command;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    public class OrderHistoryViewModel : IOrderHistoryViewModel
    {
        private readonly IOrderService orderService;
        private List<Order> baseOrderList;

        public ICommand CancelCommand { get; private set; }
        public ICommand ResubmitCommand { get; private set; }
        public ICommand GoToDetailPageCommand { get; private set; }
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
        public event Action<Order> CancelConfirmationRequested;
        public event Action<int> RedirectToResubmitRequested;

        public OrderHistoryViewModel(IOrderService injectedOrderService)
        {
            orderService = injectedOrderService;
            CancelCommand = new RelayCommandWithOneParameter<Order>(CancelOrderCommand);
            ResubmitCommand = new RelayCommandWithOneParameter<Order>(ResubmitExpiredOrderCommand);
            GoToDetailPageCommand = new RelayCommandWithOneParameter<Order>(DisplayOrderDetailCommand);
            OrderHistory = new ObservableCollection<Order>();
            baseOrderList = new List<Order>();

            LoadOrders();
        }

        private void LoadOrders()
        {
            if (orderService.ActiveUser == null)
            {
                return;
            }
            orderService.ExpireOverdueOrders();
            int clientId = orderService.ActiveUser.Id;
            List<Order> userOrders = orderService.OrdersRepository.GetOrdersOfClient(clientId);
            foreach (Order currentOrder in userOrders)
            {
                OrderHistory.Add(currentOrder);
                baseOrderList.Add(currentOrder);
            }
        }

        private void CancelOrderCommand(Order orderToCancel)
        {
            CancelConfirmationRequested?.Invoke(orderToCancel);
        }

        private void ResubmitExpiredOrderCommand(Order orderToResubmit)
        {
            RedirectToResubmitRequested?.Invoke(orderToResubmit.Id);
        }

        private void DisplayOrderDetailCommand(Order orderToModify)
        {
            RedirectToDetailRequested?.Invoke(orderToModify.Id);
        }

        private void ReapplyFilters()
        {
            List<Order> intermediateFilteredOrderList = new List<Order>(baseOrderList);

            if (isExpiredCheckbox)
            {
                intermediateFilteredOrderList = intermediateFilteredOrderList
                    .Where(order => order.IsExpired)
                    .ToList();
            }

            OrderHistory.Clear();
            foreach (Order resultOrder in intermediateFilteredOrderList)
            {
                OrderHistory.Add(resultOrder);
            }
        }

        public void CancelOrder(Order orderToCancel)
        {
            orderService.CancelOrder(orderToCancel.Id);

            orderToCancel.IsExpired = true;
            foreach (Order currOrder in baseOrderList)
            {
                if (currOrder.Id == orderToCancel.Id)
                {
                    currOrder.IsExpired = true;
                }
            }

            ReapplyFilters();
        }
    }
}