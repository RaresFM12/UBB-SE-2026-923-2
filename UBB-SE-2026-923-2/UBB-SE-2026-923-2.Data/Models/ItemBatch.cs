using System;

namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// One batch of an <see cref="Item"/> identified by its expiration date.
    /// Composite key (ItemId, ExpirationDate). Replaces the legacy
    /// <see cref="Item.Batches"/> dictionary.
    /// </summary>
    public class ItemBatch
    {
        public int ItemId { get; set; }
        public DateOnly ExpirationDate { get; set; }
        public int NumberOfPacks { get; set; }

        public Item? Item { get; set; }
    }
}
