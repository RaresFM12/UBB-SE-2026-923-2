namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Line item on an <see cref="Order"/>.
    /// Surrogate primary key Id replaces the old composite (OrderId, ItemId) PK.
    /// FK columns are maintained as EF Core shadow properties.
    /// </summary>
    public class OrderItem
    {
        // Surrogate primary key — replaces the old composite (OrderId, ItemId) PK.
        public int Id { get; set; }

        public int OrderQuantity { get; set; }

        public float Price { get; set; }

        // EF Core navigation properties — persisted via shadow FK columns
        // "OrderId" and "ItemId".
        public Order Order { get; set; } = null!;

        public Item Item { get; set; } = null!;
    }
}
