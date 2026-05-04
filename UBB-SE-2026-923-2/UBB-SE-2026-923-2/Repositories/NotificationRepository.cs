using System;
using Microsoft.EntityFrameworkCore;
using UBB_SE_2026_923_2.Data;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="INotificationRepository"/>.
    /// </summary>
    public class NotificationRepository : INotificationRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public NotificationRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public void AddNotification(int recipientStaffId, string title, string message)
        {
            using var db = dbContextFactory.CreateDbContext();

            db.Notifications.Add(new Notification
            {
                RecipientStaffId = recipientStaffId,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
            });

            db.SaveChanges();
        }
    }
}
