namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Per-user, per-item discount percentage. Composite key (UserId, ItemId).
    /// </summary>
    public class UserDiscount
    {
        public int UserId { get; set; }

        public int ItemId { get; set; }

        public float DiscountPercentage { get; set; }

        public User? User { get; set; }

        public Item? Item { get; set; }
    }
}
