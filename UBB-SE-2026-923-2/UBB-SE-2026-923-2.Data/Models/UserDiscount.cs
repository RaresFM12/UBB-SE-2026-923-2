namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Per-user, per-item discount percentage.
    /// Surrogate primary key Id replaces the old composite (UserId, ItemId) PK.
    /// FK columns are maintained as EF Core shadow properties.
    /// </summary>
    public class UserDiscount
    {
        // Surrogate primary key — replaces the old composite (UserId, ItemId) PK.
        public int Id { get; set; }

        public float DiscountPercentage { get; set; }

        // EF Core navigation properties — persisted via shadow FK columns
        // "UserId" and "ItemId".
        public User User { get; set; } = null!;

        public Item Item { get; set; } = null!;
    }
}
