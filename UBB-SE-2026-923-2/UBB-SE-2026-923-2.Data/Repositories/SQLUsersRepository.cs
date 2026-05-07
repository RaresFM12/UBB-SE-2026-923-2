namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using UBB_SE_2026_923_2.Data;
    using UBB_SE_2026_923_2.Models;

    /// <summary>
    /// EF Core implementation of <see cref="IUsersRepository"/>. Period notes,
    /// user discounts and per-item notification flags are loaded through
    /// navigation collections on <see cref="User"/> and projected back into
    /// the legacy in-memory dictionaries / lists so existing services and
    /// view models keep working unchanged.
    /// <para>
    /// In the EF-Core schema the period-tracker fields live directly on the
    /// <see cref="User"/> row instead of in a separate <c>PeriodTrackers</c>
    /// table, so <see cref="UserHasPeriodTracker"/> now answers based on
    /// whether <see cref="User.StartPeriodDate"/> has been set.
    /// </para>
    /// </summary>
    public class SQLUsersRepository : IUsersRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public SQLUsersRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public bool UserExists(string email)
        {
            using var db = this.dbContextFactory.CreateDbContext();
            return db.Users.AsNoTracking().Any(u => u.Email == email);
        }

        public bool UserExists(int id)
        {
            using var db = this.dbContextFactory.CreateDbContext();
            return db.Users.AsNoTracking().Any(u => u.Id == id);
        }

        public User GetUserById(int id)
        {
            using var db = this.dbContextFactory.CreateDbContext();
            var user = db.Users
                .AsNoTracking()
                .Include(u => u.PeriodNoteEntries)
                .Include(u => u.UserDiscountEntries)
                .Include(u => u.UserNotificationEntries)
                .FirstOrDefault(u => u.Id == id);

            return user is null ? null! : ProjectIntoLegacyCollections(user);
        }

        public User GetUserByEmail(string email)
        {
            using var db = this.dbContextFactory.CreateDbContext();
            var user = db.Users
                .AsNoTracking()
                .Include(u => u.PeriodNoteEntries)
                .Include(u => u.UserDiscountEntries)
                .Include(u => u.UserNotificationEntries)
                .FirstOrDefault(u => u.Email == email);

            return user is null ? null! : ProjectIntoLegacyCollections(user);
        }

        public void AddUser(string email, string phoneNumber, string passwordHash, string username,
            bool discountNotifications, bool isDisabled = false, bool isAdmin = false, int loyaltyPoints = 0, string role = "Client")
        {
            using var db = this.dbContextFactory.CreateDbContext();

            var user = new User
            {
                Email = email,
                PhoneNumber = phoneNumber,
                PasswordHash = passwordHash,
                Username = username,
                IsAdmin = isAdmin,
                IsDisabled = isDisabled,
                Role = role,
                DiscountNotifications = discountNotifications,
                LoyaltyPoints = loyaltyPoints,
            };

            db.Users.Add(user);
            db.SaveChanges();
        }

        public void UpdateUser(User newUser)
        {
            using var db = this.dbContextFactory.CreateDbContext();

            var existing = db.Users
                .Include(u => u.PeriodNoteEntries)
                .Include(u => u.UserDiscountEntries)
                .Include(u => u.UserNotificationEntries)
                .FirstOrDefault(u => u.Id == newUser.Id);

            if (existing is null)
            {
                return;
            }

            // Scalar fields, including period-tracker columns that used to
            // live in a separate table.
            existing.Email = newUser.Email;
            existing.PhoneNumber = newUser.PhoneNumber;
            existing.PasswordHash = newUser.PasswordHash;
            existing.Username = newUser.Username;
            existing.IsAdmin = newUser.IsAdmin;
            existing.IsDisabled = newUser.IsDisabled;
            existing.Role = newUser.Role;
            existing.DiscountNotifications = newUser.DiscountNotifications;
            existing.LoyaltyPoints = newUser.LoyaltyPoints;
            existing.StartPeriodDate = newUser.StartPeriodDate;
            existing.CycleDays = newUser.CycleDays;
            existing.PeriodLasts = newUser.PeriodLasts;
            existing.PremenstrualSyndromeOption = newUser.PremenstrualSyndromeOption;

            // Replace the related collections from the legacy in-memory views.
            // Phase 3 will switch callers to mutate the EF nav collections
            // directly, at which point this projection goes away.
            db.PeriodNotes.RemoveRange(existing.PeriodNoteEntries);
            existing.PeriodNoteEntries.Clear();
            foreach (var pair in newUser.PeriodNotes)
            {
                existing.PeriodNoteEntries.Add(new PeriodNote
                {
                    UserId = existing.Id,
                    NoteId = pair.Key,
                    NoteBody = pair.Value.Item1,
                    IsDone = pair.Value.Item2,
                });
            }

            db.UserDiscounts.RemoveRange(existing.UserDiscountEntries);
            existing.UserDiscountEntries.Clear();
            foreach (var pair in newUser.UserDiscounts)
            {
                existing.UserDiscountEntries.Add(new UserDiscount
                {
                    UserId = existing.Id,
                    ItemId = pair.Key,
                    DiscountPercentage = pair.Value,
                });
            }

            db.UserNotifications.RemoveRange(existing.UserNotificationEntries);
            existing.UserNotificationEntries.Clear();
            var notificationItemIds = new HashSet<int>(newUser.FavoriteItems);
            notificationItemIds.UnionWith(newUser.StockAlerts);
            foreach (var itemId in notificationItemIds)
            {
                existing.UserNotificationEntries.Add(new UserNotification
                {
                    UserId = existing.Id,
                    ItemId = itemId,
                    IsFavorite = newUser.FavoriteItems.Contains(itemId),
                    IsStockAlert = newUser.StockAlerts.Contains(itemId),
                });
            }

            db.SaveChanges();
        }

        public List<User> GetAllUsers()
        {
            using var db = this.dbContextFactory.CreateDbContext();
            var users = db.Users
                .AsNoTracking()
                .Include(u => u.PeriodNoteEntries)
                .Include(u => u.UserDiscountEntries)
                .Include(u => u.UserNotificationEntries)
                .ToList();

            foreach (var user in users)
            {
                ProjectIntoLegacyCollections(user);
            }

            return users;
        }

        public bool UserHasPeriodTracker(int id)
        {
            using var db = this.dbContextFactory.CreateDbContext();
            return db.Users
                .AsNoTracking()
                .Any(u => u.Id == id
                       && u.StartPeriodDate != default
                       && u.StartPeriodDate != DateOnly.MinValue
                       && u.StartPeriodDate != DateOnly.MaxValue);
        }

        private static User ProjectIntoLegacyCollections(User user)
        {
            foreach (var note in user.PeriodNoteEntries)
            {
                if (!user.PeriodNotes.ContainsKey(note.NoteId))
                {
                    user.AddPeriodNoteToUser(note.NoteId, note.NoteBody, note.IsDone);
                }
            }

            foreach (var discount in user.UserDiscountEntries)
            {
                if (!user.UserDiscounts.ContainsKey(discount.ItemId))
                {
                    user.AddUserDiscount(discount.ItemId, discount.DiscountPercentage);
                }
            }

            foreach (var notification in user.UserNotificationEntries)
            {
                if (notification.IsFavorite && !user.FavoriteItems.Contains(notification.ItemId))
                {
                    user.AddItemToFavoriteItems(notification.ItemId);
                }

                if (notification.IsStockAlert && !user.StockAlerts.Contains(notification.ItemId))
                {
                    user.AddStockAlertToUser(notification.ItemId);
                }
            }

            return user;
        }
    }
}
