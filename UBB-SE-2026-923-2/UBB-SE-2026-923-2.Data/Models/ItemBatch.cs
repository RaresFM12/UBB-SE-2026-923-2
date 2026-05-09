namespace UBB_SE_2026_923_2.Models
{
    using System;

    /// <summary>
    /// One batch of an <see cref="Item"/> identified by its expiration date.
    /// Surrogate primary key Id replaces the old composite (ItemId, ExpirationDate) PK.
    /// FK column is maintained as an EF Core shadow property.
    /// </summary>
    public class ItemBatch
    {
        // Surrogate primary key — replaces the old composite (ItemId, ExpirationDate) PK.
        public int Id { get; set; }

        public DateOnly ExpirationDate { get; set; }

        public int NumberOfPacks { get; set; }

        // EF Core navigation property — persisted via shadow FK column "ItemId".
        public Item Item { get; set; } = null!;
    }
}
