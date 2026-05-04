namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Line item on an <see cref="Order"/>. Composite key (OrderId, ItemId).
    /// Replaces the legacy <see cref="Order.ItemQuantitiesWithFinalPrice"/> dictionary.
    /// </summary>
    public class OrderItem
    {
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public int OrderQuantity { get; set; }
        public float Price { get; set; }

        public Order? Order { get; set; }
        public Item? Item { get; set; }
    }
}
