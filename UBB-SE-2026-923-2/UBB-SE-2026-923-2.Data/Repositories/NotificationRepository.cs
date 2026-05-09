namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using UBB_SE_2026_923_2.Data;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// EF Core implementation of <see cref="INotificationRepository"/>.
    /// </summary>
    public class NotificationRepository : INotificationRepository
    {
        private readonly IDbContextFactory<AppDbContext> databaseContextFactory;

        public NotificationRepository(IDbContextFactory<AppDbContext> databaseContextFactory)
        {
            this.databaseContextFactory = databaseContextFactory ?? throw new ArgumentNullException(nameof(databaseContextFactory));
        }

        public void AddNotification(int recipientStaffId, string title, string message)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();

            var recipientStaffMember = databaseContext.StaffMembers.Find(recipientStaffId);
            if (recipientStaffMember == null)
            {
                recipientStaffMember = new Staff { StaffID = recipientStaffId };
                databaseContext.Attach(recipientStaffMember);
            }

            databaseContext.Notifications.Add(new Notification
            {
                Recipient = recipientStaffMember,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
            });

            databaseContext.SaveChanges();
        }
    }
}
