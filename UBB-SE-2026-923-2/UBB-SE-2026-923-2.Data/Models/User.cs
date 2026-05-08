namespace UBB_SE_2026_923_2.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    public class BasketEntry
    {
        public int Quantity { get; set; }

        public float ExtraDiscountPercentage { get; set; }

        // Parameterless ctor required by System.Text.Json for round-trip via API.
        public BasketEntry()
        {
        }

        public BasketEntry(int quantity, float extraDiscountPercentage = 0f)
        {
            this.Quantity = quantity;
            this.ExtraDiscountPercentage = extraDiscountPercentage;
        }
    }

    public class User
    {
        // Setters opened up (previously private) so System.Text.Json can
        // round-trip the User payload across the Web API.
        public int Id { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string PasswordHash { get; set; }

        public bool IsAdmin { get; set; }

        public string Username { get; set; }

        public bool IsDisabled { get; set; }

        public string Role { get; set; }

        public DateOnly StartPeriodDate { get; set; }

        public int CycleDays { get; set; }

        public int PeriodLasts { get; set; }

        public int PremenstrualSyndromeOption { get; set; }

        [NotMapped]
        public Dictionary<int, Tuple<string, bool>> PeriodNotes { get; set; }

        [NotMapped]
        public List<int> StockAlerts { get; set; }

        [NotMapped]
        public List<int> FavoriteItems { get; set; }

        [NotMapped]
        public Dictionary<int, float> UserDiscounts { get; set; }

        [NotMapped]
        public Dictionary<int, BasketEntry> Basket { get; set; }

        public bool DiscountNotifications { get; set; }

        public int LoyaltyPoints { get; set; }

        // ---- EF Core navigation collections (persisted) ----
        // The legacy [NotMapped] dictionaries above remain as the in-memory API
        // for existing callers; Phase 2 rewires repositories to populate these
        // collections instead, then the dictionary adapters can be removed.
        // [JsonIgnore]: server projects these into the legacy dictionaries
        // before returning, and they create cycles back to User over the wire.
        [JsonIgnore]
        public ICollection<PeriodNote> PeriodNoteEntries { get; set; } = new List<PeriodNote>();

        [JsonIgnore]
        public ICollection<UserDiscount> UserDiscountEntries { get; set; } = new List<UserDiscount>();

        [JsonIgnore]
        public ICollection<UserNotification> UserNotificationEntries { get; set; } = new List<UserNotification>();

        [JsonIgnore]
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        private const int CycleDaysDefault = 28;
        private const int PeriodLastsDefault = 5;
        private const int PremenstrualSyndromeOptionDefault = 0;

        public User()
        {
            this.Email = string.Empty;
            this.PhoneNumber = string.Empty;
            this.PasswordHash = string.Empty;
            this.Username = string.Empty;
            this.Role = nameof(UserRole.Client);
            this.PeriodNotes = new Dictionary<int, Tuple<string, bool>>();
            this.StockAlerts = new List<int>();
            this.FavoriteItems = new List<int>();
            this.UserDiscounts = new Dictionary<int, float>();
            this.Basket = new Dictionary<int, BasketEntry>();
        }

        public User(int userId, string email, string phoneNumber,
                    string passwordHash, bool isAdmin, bool isDisabled,
                    string userName, bool discountNotifications,
                    int loyaltyPoints, DateOnly startPeriodDate = default(DateOnly),
                    int cycleDays = CycleDaysDefault, int periodLasts = PeriodLastsDefault,
                    int premenstrualSyndromeOption = PremenstrualSyndromeOptionDefault)
            : this()
        {
            this.Id = userId;
            this.Email = email;
            this.PhoneNumber = phoneNumber;
            this.PasswordHash = passwordHash;
            this.IsAdmin = isAdmin;
            this.IsDisabled = isDisabled;
            this.Username = userName;
            this.DiscountNotifications = discountNotifications;
            this.LoyaltyPoints = loyaltyPoints;
            this.StartPeriodDate = startPeriodDate.Year == default(DateOnly).Year
                ? DateOnly.FromDateTime(DateTime.Now)
                : startPeriodDate;
            this.CycleDays = cycleDays;
            this.PeriodLasts = periodLasts;
            this.PremenstrualSyndromeOption = premenstrualSyndromeOption;
        }

        public void AddStockAlertToUser(int newItemId)
        {
            if (this.StockAlerts.Contains(newItemId))
            {
                throw new ArgumentException("Item #" + newItemId + " already in stock alert");
            }

            this.StockAlerts.Add(newItemId);
        }

        public void RemoveStockAlertFromUser(int itemIdToRemove)
        {
            if (!this.StockAlerts.Contains(itemIdToRemove))
            {
                throw new ArgumentException("Item #" + itemIdToRemove + " not in stock alert");
            }

            this.StockAlerts.Remove(itemIdToRemove);
        }

        public void AddItemToFavoriteItems(int newItemId)
        {
            if (this.FavoriteItems.Contains(newItemId))
            {
                throw new ArgumentException("Item #" + newItemId + " already in favorites");
            }

            this.FavoriteItems.Add(newItemId);
        }

        public void RemoveItemFromFavoriteItems(int itemIdToRemove)
        {
            if (!this.FavoriteItems.Contains(itemIdToRemove))
            {
                throw new ArgumentException("Item #" + itemIdToRemove + " not in favorites");
            }

            this.FavoriteItems.Remove(itemIdToRemove);
        }

        public void AddUserDiscount(int newItemId, float discount)
        {
            if (this.UserDiscounts.ContainsKey(newItemId))
            {
                throw new ArgumentException("Item #" + newItemId + " already has a discount on this user");
            }

            this.UserDiscounts[newItemId] = discount;
        }

        public void ChangeUserDiscount(int itemId, float newDiscount)
        {
            if (!this.UserDiscounts.ContainsKey(itemId))
            {
                throw new ArgumentException("Item #" + itemId + " doesn't have a discount on this user");
            }

            this.UserDiscounts[itemId] = newDiscount;
        }

        public void RemoveUserDiscount(int itemIdToRemove)
        {
            if (!this.UserDiscounts.ContainsKey(itemIdToRemove))
            {
                throw new ArgumentException("Item #" + itemIdToRemove + " doesn't have a discount on this user");
            }

            this.UserDiscounts.Remove(itemIdToRemove);
        }

        public void AddItemToBasket(int newItemId, int quantityToGet, float extraDiscountPercentage = 0f)
        {
            if (this.Basket.ContainsKey(newItemId))
            {
                throw new ArgumentException("Item #" + newItemId + " already in user's basket");
            }

            this.Basket[newItemId] = new BasketEntry(quantityToGet, extraDiscountPercentage);
        }

        public void ChangeItemQuantityInBasket(int itemId, int newQuantityToGet)
        {
            if (!this.Basket.ContainsKey(itemId))
            {
                throw new ArgumentException("Item #" + itemId + " is not in user's basket");
            }

            this.Basket[itemId].Quantity = newQuantityToGet;
        }

        public void ChangeItemDiscountInBasket(int itemId, float extraDiscountPercentage)
        {
            if (!this.Basket.ContainsKey(itemId))
            {
                throw new ArgumentException("Item #" + itemId + " is not in user's basket");
            }

            this.Basket[itemId].ExtraDiscountPercentage = extraDiscountPercentage;
        }

        public void RemoveItemFromBasket(int itemIdToRemove)
        {
            if (!this.Basket.ContainsKey(itemIdToRemove))
            {
                throw new ArgumentException("Item #" + itemIdToRemove + " is not in user's basket");
            }

            this.Basket.Remove(itemIdToRemove);
        }

        public void SetPeriodTracker(DateOnly startPeriodDate, int cycleDays,
                                     int periodLasts, int premenstrualSyndromeOption)
        {
            this.StartPeriodDate = startPeriodDate;
            this.CycleDays = cycleDays;
            this.PeriodLasts = periodLasts;
            this.PremenstrualSyndromeOption = premenstrualSyndromeOption;
        }

        public void AddPeriodNoteToUser(int noteId, string noteBody, bool isDone)
        {
            if (this.PeriodNotes.ContainsKey(noteId))
            {
                throw new ArgumentException("Note #" + noteId + " is already exists");
            }

            this.PeriodNotes[noteId] = new Tuple<string, bool>(noteBody, isDone);
        }

        public void RemovePeriodNoteFromUser(int noteIdToRemove)
        {
            if (!this.PeriodNotes.ContainsKey(noteIdToRemove))
            {
                throw new ArgumentException("Note #" + noteIdToRemove + " is not created");
            }

            this.PeriodNotes.Remove(noteIdToRemove);
        }
    }
}