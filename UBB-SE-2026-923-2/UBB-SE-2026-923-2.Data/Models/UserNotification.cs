namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Tracks per-user, per-item notification flags (favorite, stock alert).
    /// Surrogate primary key Id replaces the old composite (UserId, ItemId) PK.
    /// FK columns are maintained as EF Core shadow properties.
    /// </summary>
    public class UserNotification
    {
        // Surrogate primary key — replaces the old composite (UserId, ItemId) PK.
        public int Id { get; set; }

        public bool IsFavorite { get; set; }

        public bool IsStockAlert { get; set; }

        // EF Core navigation properties — persisted via shadow FK columns
        // "UserId" and "ItemId".
        public User User { get; set; } = null!;

        public Item Item { get; set; } = null!;
    }
}
