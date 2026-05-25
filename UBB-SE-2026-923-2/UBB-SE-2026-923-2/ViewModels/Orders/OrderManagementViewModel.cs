namespace UBB_SE_2026_923_2.ViewModels.Orders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using UBB_SE_2026_923_2.Command;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public class OrderDetail
    {
        public int OrderID { get; set; }

        public string UserEmail { get; set; }

        public bool IsComplete { get; set; }

        public bool IsExpired { get; set; }

        public DateOnly PickUpDate { get; set; }

        public DateOnly ExpirationDate
        {
            get => this.PickUpDate.AddDays(Order.OrderExpirationDays);
        }

        public string OrderString
        {
            get => $"Order#{this.OrderID}";
        }

        public string PickUpDateString
        {
            get => this.PickUpDate.ToString("yyyy.MM.dd");
        }

        public string ExpirationDateString
        {
            get => this.ExpirationDate.ToString("yyyy.MM.dd");
        }

        public OrderDetail(Order orderDetails, string userEmail)
        {
            this.OrderID = orderDetails.Id;
            this.UserEmail = userEmail;
            this.IsComplete = orderDetails.IsCompleted;
            this.IsExpired = orderDetails.IsExpired;
            this.PickUpDate = orderDetails.PickUpDate;
        }
    }

    // Inherit from the new interface!
    public class OrderManagementViewModel : IOrderManagementViewModel
    {
        private const int EmptyLength = 0;

        // Changed to IOrderService
        private readonly IOrderService orderService;

        private readonly List<OrderDetail> baseOrderList;

        public ObservableCollection<OrderDetail> FilteredOrderList { get; set; }

        public ICommand RedirectToDetailPageCommand { get; set; }

        private string orderIDInput;
        private string userEmailInput;
        private bool isIncompleteCheckbox;
        private bool isExpiredCheckbox;

        public string OrderIDInput
        {
            get => this.orderIDInput;
            set
            {
                this.orderIDInput = value;
                this.OnPropertyChanged();
                this.ReapplyFilters();
            }
        }

        public string UserEmailInput
        {
            get => this.userEmailInput;
            set
            {
                this.userEmailInput = value;
                this.OnPropertyChanged();
                this.ReapplyFilters();
            }
        }

        public bool IsIncompleteCheckbox
        {
            get => this.isIncompleteCheckbox;
            set
            {
                this.isIncompleteCheckbox = value;
                this.OnPropertyChanged();
                this.ReapplyFilters();
            }
        }

        public bool IsExpiredCheckbox
        {
            get => this.isExpiredCheckbox;
            set
            {
                this.isExpiredCheckbox = value;
                this.OnPropertyChanged();
                this.ReapplyFilters();
            }
        }

        public OrderManagementViewModel(IOrderService newOrderServ)
        {
            this.orderService = newOrderServ;
            this.baseOrderList = new();
            this.FilteredOrderList = new();
            this.RedirectToDetailPageCommand = new RelayCommandWithOneParameter<OrderDetail>(this.OnClickDetailButton);

            this.orderService.ExpireOverdueOrders();
            foreach (Order currOrder in this.orderService.OrdersRepository.GetAllOrders())
            {
                string currUserEmail = this.ResolveClientEmail(currOrder);

                OrderDetail currOrderDetail = new(currOrder, currUserEmail);

                this.baseOrderList.Add(currOrderDetail);
                this.FilteredOrderList.Add(currOrderDetail);
            }
        }

        private string ResolveClientEmail(Order order)
        {
            int? clientId = order.Client?.Id;
            if (!clientId.HasValue && order.ClientId > 0)
            {
                clientId = order.ClientId;
            }

            if (!clientId.HasValue)
            {
                Order hydratedOrder = this.orderService.OrdersRepository.GetOrder(order.Id);
                clientId = hydratedOrder?.Client?.Id;
                if (!clientId.HasValue && hydratedOrder?.ClientId > 0)
                {
                    clientId = hydratedOrder.ClientId;
                }
            }

            if (!clientId.HasValue)
            {
                return "Unknown";
            }

            return this.orderService.UsersRepository.GetUserById(clientId.Value)?.Email ?? "Unknown";
        }

        public event Action<Tuple<IOrderService, OrderDetail>> ClickDetailButton;

        public virtual void OnClickDetailButton(OrderDetail chosenOrder)
        {
            this.ClickDetailButton?.Invoke(new Tuple<IOrderService, OrderDetail>(this.orderService, chosenOrder));
        }

        private void ReapplyFilters()
        {
            List<OrderDetail> intermediateFilteredOrderList = new();

            foreach (OrderDetail iterOrderDetail in this.baseOrderList)
            {
                intermediateFilteredOrderList.Add(iterOrderDetail);
            }

            try
            {
                int inputtedOrderID = int.Parse(this.orderIDInput);
                List<OrderDetail> result = intermediateFilteredOrderList
                    .Where<OrderDetail>(order => order.OrderID == inputtedOrderID)
                    .ToList<OrderDetail>();

                intermediateFilteredOrderList.Clear();
                foreach (OrderDetail resultOrder in result)
                {
                    intermediateFilteredOrderList.Add(resultOrder);
                }
            }
            catch (Exception exception)
            {
            }

            if (this.userEmailInput is not null)
            {
                if (this.userEmailInput.Length != EmptyLength)
                {
                    List<OrderDetail> result = intermediateFilteredOrderList
                        .Where<OrderDetail>(order => order.UserEmail == this.userEmailInput)
                        .ToList<OrderDetail>();

                    intermediateFilteredOrderList.Clear();
                    foreach (OrderDetail resultOrder in result)
                    {
                        intermediateFilteredOrderList.Add(resultOrder);
                    }
                }
            }

            if (this.isIncompleteCheckbox)
            {
                List<OrderDetail> result = intermediateFilteredOrderList
                    .Where<OrderDetail>(order => !order.IsComplete && !order.IsExpired)
                    .ToList<OrderDetail>();

                intermediateFilteredOrderList.Clear();
                foreach (OrderDetail resultOrder in result)
                {
                    intermediateFilteredOrderList.Add(resultOrder);
                }
            }

            if (this.isExpiredCheckbox)
            {
                List<OrderDetail> result = intermediateFilteredOrderList
                    .Where<OrderDetail>(order => order.IsExpired)
                    .ToList<OrderDetail>();

                intermediateFilteredOrderList.Clear();
                foreach (OrderDetail resultOrder in result)
                {
                    intermediateFilteredOrderList.Add(resultOrder);
                }
            }

            this.FilteredOrderList.Clear();
            foreach (OrderDetail resultOrder in intermediateFilteredOrderList)
            {
                this.FilteredOrderList.Add(resultOrder);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
