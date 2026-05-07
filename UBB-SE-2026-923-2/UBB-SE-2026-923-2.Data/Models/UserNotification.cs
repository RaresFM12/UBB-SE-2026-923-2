namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Tracks per-user, per-item notification flags (favorite, stock alert).
    /// Composite key (UserId, ItemId). Replaces the legacy <c>StockAlerts</c>
    /// and <c>FavoriteItems</c> ID lists on <see cref="User"/>.
    /// </summary>
    public class UserNotification
    {
        public int UserId { get; set; }

        public int ItemId { get; set; }

        public bool IsFavorite { get; set; }

        public bool IsStockAlert { get; set; }

        public User? User { get; set; }

        public Item? Item { get; set; }
    }
}
