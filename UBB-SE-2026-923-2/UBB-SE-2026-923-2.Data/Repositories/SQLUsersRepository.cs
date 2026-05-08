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
        private readonly IDbContextFactory<AppDbContext> databaseContextFactory;

        public SQLUsersRepository(IDbContextFactory<AppDbContext> databaseContextFactory)
        {
            this.databaseContextFactory = databaseContextFactory ?? throw new ArgumentNullException(nameof(databaseContextFactory));
        }

        public bool UserExists(string email)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.Users.AsNoTracking().Any(user => user.Email == email);
        }

        public bool UserExists(int userId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.Users.AsNoTracking().Any(user => user.Id == userId);
        }

        public User GetUserById(int userId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var user = databaseContext.Users
                .AsNoTracking()
                .Include(user => user.PeriodNoteEntries)
                .Include(user => user.UserDiscountEntries)
                .Include(user => user.UserNotificationEntries)
                .FirstOrDefault(user => user.Id == userId);

            return user is null ? null! : ProjectIntoLegacyCollections(user);
        }

        public User GetUserByEmail(string email)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var user = databaseContext.Users
                .AsNoTracking()
                .Include(user => user.PeriodNoteEntries)
                .Include(user => user.UserDiscountEntries)
                .Include(user => user.UserNotificationEntries)
                .FirstOrDefault(user => user.Email == email);

            return user is null ? null! : ProjectIntoLegacyCollections(user);
        }

        public void AddUser(string email, string phoneNumber, string passwordHash, string username,
            bool discountNotifications, bool isDisabled = false, bool isAdmin = false, int loyaltyPoints = 0, string role = "Client")
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();

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

            databaseContext.Users.Add(user);
            databaseContext.SaveChanges();
        }

        public void UpdateUser(User newUser)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();

            var existingUser = databaseContext.Users
                .Include(user => user.PeriodNoteEntries)
                .Include(user => user.UserDiscountEntries)
                .Include(user => user.UserNotificationEntries)
                .FirstOrDefault(user => user.Id == newUser.Id);

            if (existingUser is null)
            {
                return;
            }

            // Scalar fields, including period-tracker columns that used to
            // live in a separate table.
            existingUser.Email = newUser.Email;
            existingUser.PhoneNumber = newUser.PhoneNumber;
            existingUser.PasswordHash = newUser.PasswordHash;
            existingUser.Username = newUser.Username;
            existingUser.IsAdmin = newUser.IsAdmin;
            existingUser.IsDisabled = newUser.IsDisabled;
            existingUser.Role = newUser.Role;
            existingUser.DiscountNotifications = newUser.DiscountNotifications;
            existingUser.LoyaltyPoints = newUser.LoyaltyPoints;
            existingUser.StartPeriodDate = newUser.StartPeriodDate;
            existingUser.CycleDays = newUser.CycleDays;
            existingUser.PeriodLasts = newUser.PeriodLasts;
            existingUser.PremenstrualSyndromeOption = newUser.PremenstrualSyndromeOption;

            // Replace the related collections from the legacy in-memory views.
            // Phase 3 will switch callers to mutate the EF nav collections
            // directly, at which point this projection goes away.
            databaseContext.PeriodNotes.RemoveRange(existingUser.PeriodNoteEntries);
            existingUser.PeriodNoteEntries.Clear();
            foreach (var keyValuePair in newUser.PeriodNotes)
            {
                existingUser.PeriodNoteEntries.Add(new PeriodNote
                {
                    UserId = existingUser.Id,
                    NoteId = keyValuePair.Key,
                    NoteBody = keyValuePair.Value.Item1,
                    IsDone = keyValuePair.Value.Item2,
                });
            }

            databaseContext.UserDiscounts.RemoveRange(existingUser.UserDiscountEntries);
            existingUser.UserDiscountEntries.Clear();
            foreach (var keyValuePair in newUser.UserDiscounts)
            {
                existingUser.UserDiscountEntries.Add(new UserDiscount
                {
                    UserId = existingUser.Id,
                    ItemId = keyValuePair.Key,
                    DiscountPercentage = keyValuePair.Value,
                });
            }

            databaseContext.UserNotifications.RemoveRange(existingUser.UserNotificationEntries);
            existingUser.UserNotificationEntries.Clear();
            var notificationItemIds = new HashSet<int>(newUser.FavoriteItems);
            notificationItemIds.UnionWith(newUser.StockAlerts);
            foreach (var itemId in notificationItemIds)
            {
                existingUser.UserNotificationEntries.Add(new UserNotification
                {
                    UserId = existingUser.Id,
                    ItemId = itemId,
                    IsFavorite = newUser.FavoriteItems.Contains(itemId),
                    IsStockAlert = newUser.StockAlerts.Contains(itemId),
                });
            }

            databaseContext.SaveChanges();
        }

        public List<User> GetAllUsers()
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            var users = databaseContext.Users
                .AsNoTracking()
                .Include(user => user.PeriodNoteEntries)
                .Include(user => user.UserDiscountEntries)
                .Include(user => user.UserNotificationEntries)
                .ToList();

            foreach (var user in users)
            {
                ProjectIntoLegacyCollections(user);
            }

            return users;
        }

        public bool UserHasPeriodTracker(int userId)
        {
            using var databaseContext = this.databaseContextFactory.CreateDbContext();
            return databaseContext.Users
                .AsNoTracking()
                .Any(user => user.Id == userId
                           && user.StartPeriodDate != default
                           && user.StartPeriodDate != DateOnly.MinValue
                           && user.StartPeriodDate != DateOnly.MaxValue);
        }

        private static User ProjectIntoLegacyCollections(User user)
        {
            foreach (var periodNote in user.PeriodNoteEntries)
            {
                if (!user.PeriodNotes.ContainsKey(periodNote.NoteId))
                {
                    user.AddPeriodNoteToUser(periodNote.NoteId, periodNote.NoteBody, periodNote.IsDone);
                }
            }

            foreach (var userDiscount in user.UserDiscountEntries)
            {
                if (!user.UserDiscounts.ContainsKey(userDiscount.ItemId))
                {
                    user.AddUserDiscount(userDiscount.ItemId, userDiscount.DiscountPercentage);
                }
            }

            foreach (var userNotification in user.UserNotificationEntries)
            {
                if (userNotification.IsFavorite && !user.FavoriteItems.Contains(userNotification.ItemId))
                {
                    user.AddItemToFavoriteItems(userNotification.ItemId);
                }

                if (userNotification.IsStockAlert && !user.StockAlerts.Contains(userNotification.ItemId))
                {
                    user.AddStockAlertToUser(userNotification.ItemId);
                }
            }

            return user;
        }
    }
}