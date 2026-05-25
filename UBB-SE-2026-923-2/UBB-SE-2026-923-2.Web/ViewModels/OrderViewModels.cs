namespace UBB_SE_2026_923_2.Web.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using UBB_SE_2026_923_2.ViewModels.Orders;

    public class OrdersIndexViewModel
    {
        public List<OrderListItemViewModel> Orders { get; set; } = new();

        public bool ShowExpiredOnly { get; set; }

        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }
    }

    public class OrderManagementViewModel
    {
        public List<OrderListItemViewModel> Orders { get; set; } = new();

        public string UserEmail { get; set; } = string.Empty;

        public int? OrderId { get; set; }

        public bool IncompleteOnly { get; set; }

        public bool ExpiredOnly { get; set; }

        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }
    }

    public class OrderListItemViewModel
    {
        public int Id { get; set; }

        public string UserEmail { get; set; } = string.Empty;

        public DateOnly PickUpDate { get; set; }

        public DateOnly ExpirationDate { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsExpired { get; set; }

        public string PickUpDateString => this.PickUpDate.ToString("yyyy.MM.dd");

        public string ExpirationDateString => this.ExpirationDate.ToString("yyyy.MM.dd");

        public string Status
        {
            get
            {
                if (this.IsExpired)
                {
                    return "Expired";
                }

                return this.IsCompleted ? "Complete" : "Incomplete";
            }
        }
    }

    public class OrderCheckoutViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        public DateOnly PickUpDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        public List<BasketItemViewModel> Items { get; set; } = new();

        public float TotalBeforeDiscount { get; set; }

        public float TotalAfterDiscount { get; set; }
    }

    public class OrderDetailsViewModel
    {
        public int Id { get; set; }

        public string UserEmail { get; set; } = string.Empty;

        public DateOnly PickUpDate { get; set; }

        public DateOnly ExpirationDate { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsExpired { get; set; }

        public bool AdminView { get; set; }

        public List<OrderLineItemViewModel> Items { get; set; } = new();

        public float Total { get; set; }

        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public string PickUpDateString => this.PickUpDate.ToString("yyyy.MM.dd");

        public string ExpirationDateString => this.ExpirationDate.ToString("yyyy.MM.dd");

        public string Status
        {
            get
            {
                if (this.IsExpired)
                {
                    return "Expired";
                }

                return this.IsCompleted ? "Complete" : "Incomplete";
            }
        }
    }

    public class OrderEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly PickUpDate { get; set; }

        public bool AdminView { get; set; }

        public List<OrderLineItemViewModel> Items { get; set; } = new();

        public float Total { get; set; }
    }

    public class OrderResubmitViewModel
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly PickUpDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        public List<OrderLineItemViewModel> Items { get; set; } = new();

        public float Total { get; set; }
    }

    public class OrderLineItemViewModel
    {
        public int ItemId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Producer { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        public float FinalPrice { get; set; }
    }
}
