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

namespace UBB_SE_2026_923_2.ViewModels.Orders
{
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
            OrderID = orderDetails.Id;
            UserEmail = userEmail;
            IsComplete = orderDetails.IsCompleted;
            IsExpired = orderDetails.IsExpired;
            PickUpDate = orderDetails.PickUpDate;
        }
    }

    // Inherit from the new interface!
    public class OrderManagementViewModel : IOrderManagementViewModel
    {
        private const int EmptyLength = 0;

        // Changed to IOrderService
        private readonly IOrderService orderService;

        private List<OrderDetail> baseOrderList;
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
                orderIDInput = value;
                OnPropertyChanged();
                ReapplyFilters();
            }
        }
        public string UserEmailInput
        {
            get => this.userEmailInput;
            set
            {
                userEmailInput = value;
                OnPropertyChanged();
                ReapplyFilters();
            }
        }
        public bool IsIncompleteCheckbox
        {
            get => this.isIncompleteCheckbox;
            set
            {
                isIncompleteCheckbox = value;
                OnPropertyChanged();
                ReapplyFilters();
            }
        }
        public bool IsExpiredCheckbox
        {
            get => this.isExpiredCheckbox;
            set
            {
                isExpiredCheckbox = value;
                OnPropertyChanged();
                ReapplyFilters();
            }
        }

        public OrderManagementViewModel(IOrderService newOrderServ)
        {
            orderService = newOrderServ;
            baseOrderList = new ();
            FilteredOrderList = new ();
            RedirectToDetailPageCommand = new RelayCommandWithOneParameter<OrderDetail>(OnClickDetailButton);

            foreach (Order currOrder in orderService.OrdersRepository.GetAllOrders())
            {
                int userID = orderService.OrdersRepository.GetOrder(currOrder.Id).ClientId;
                string currUserEmail = orderService.UsersRepository.GetUserById(userID).Email;

                OrderDetail currOrderDetail = new (currOrder, currUserEmail);

                baseOrderList.Add(currOrderDetail);
                FilteredOrderList.Add(currOrderDetail);
            }
        }

        public event Action<Tuple<IOrderService, OrderDetail>> ClickDetailButton;

        public virtual void OnClickDetailButton(OrderDetail chosenOrder)
        {
            ClickDetailButton?.Invoke(new Tuple<IOrderService, OrderDetail>(orderService, chosenOrder));
        }

        private void ReapplyFilters()
        {
            List<OrderDetail> intermediateFilteredOrderList = new ();

            foreach (OrderDetail iterOrderDetail in baseOrderList)
            {
                intermediateFilteredOrderList.Add(iterOrderDetail);
            }

            try
            {
                int inputtedOrderID = int.Parse(orderIDInput);
                List<OrderDetail> result = intermediateFilteredOrderList
                    .Where<OrderDetail>(order => order.OrderID == inputtedOrderID)
                    .ToList<OrderDetail>();

                intermediateFilteredOrderList.Clear();
                foreach (OrderDetail resultOrder in result)
                {
                    intermediateFilteredOrderList.Add(resultOrder);
                }
            }
            catch (Exception e)
            {
            }

            if (userEmailInput is not null)
            {
                if (userEmailInput.Length != EmptyLength)
                {
                    List<OrderDetail> result = intermediateFilteredOrderList
                        .Where<OrderDetail>(order => order.UserEmail == userEmailInput)
                        .ToList<OrderDetail>();

                    intermediateFilteredOrderList.Clear();
                    foreach (OrderDetail resultOrder in result)
                    {
                        intermediateFilteredOrderList.Add(resultOrder);
                    }
                }
            }

            if (isIncompleteCheckbox)
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

            if (isExpiredCheckbox)
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

            FilteredOrderList.Clear();
            foreach (OrderDetail resultOrder in intermediateFilteredOrderList)
            {
                FilteredOrderList.Add(resultOrder);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}