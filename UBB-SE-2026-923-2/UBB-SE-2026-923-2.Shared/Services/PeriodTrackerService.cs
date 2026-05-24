namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;

    public class PeriodTrackerService : IPeriodTrackerService
    {
        private const int DefaultCycleLengthInDays = 28;
        private const int DefaultPeriodLengthInDays = 5;
        private const int DefaultPremenstrualSyndromeOption = 0;
        private const int FirstNoteIdentifier = 1;
        private const int MidnightHour = 0;
        private const int MidnightMinute = 0;

        private readonly IUsersRepository usersRepository;
        private readonly RaresICurrentUserService currentUserService;
        private readonly IWellnessItemsService? wellnessItemsService;

        public PeriodTrackerService(
            IUsersRepository usersRepository,
            RaresICurrentUserService currentUserService)
            : this(usersRepository, currentUserService, null)
        {
        }

        public PeriodTrackerService(
            IUsersRepository usersRepository,
            RaresICurrentUserService currentUserService,
            IWellnessItemsService? wellnessItemsService)
        {
            this.usersRepository = usersRepository;
            this.currentUserService = currentUserService;
            this.wellnessItemsService = wellnessItemsService;
        }

        public User GetCurrentUser()
        {
            return this.currentUserService.RaresCurrentUser;
        }

        public PeriodTrackerState GetTrackerState()
        {
            var cachedUser = this.currentUserService.RaresCurrentUser;
            if (cachedUser != null)
            {
                var freshUser = this.usersRepository.GetUserById(cachedUser.Id);
                if (freshUser != null)
                {
                    cachedUser.PeriodNotes = freshUser.PeriodNotes;

                    cachedUser.SetPeriodTracker(
                        freshUser.StartPeriodDate,
                        Convert.ToInt32(freshUser.CycleDays),
                        Convert.ToInt32(freshUser.PeriodLasts),
                        freshUser.PremenstrualSyndromeOption);
                }
            }

            User currentUser = this.GetCurrentUser();

            if (currentUser == null)
            {
                return CreateDefaultTrackerState();
            }

            DateTime trackerStartDate = GetTrackerStartDate(currentUser);

            return new PeriodTrackerState
            {
                StartPeriodDate = new DateTimeOffset(trackerStartDate),
                CycleDays = currentUser.CycleDays,
                PeriodLasts = currentUser.PeriodLasts,
                PremenstrualSyndromeOption = currentUser.PremenstrualSyndromeOption,
                HasPeriodTracker = this.usersRepository.UserHasPeriodTracker(currentUser.Id),
            };
        }

        public PeriodTrackerDashboardSnapshot GetDashboardSnapshot(int monthOffset)
        {
            var cachedUser = this.currentUserService.RaresCurrentUser;
            if (cachedUser != null)
            {
                var freshUser = this.usersRepository.GetUserById(cachedUser.Id);
                if (freshUser != null)
                {
                    cachedUser.PeriodNotes = freshUser.PeriodNotes;
                    cachedUser.SetPeriodTracker(
                        freshUser.StartPeriodDate,
                        freshUser.CycleDays,
                        freshUser.PeriodLasts,
                        freshUser.PremenstrualSyndromeOption);
                }
            }

            PeriodTrackerState trackerState = this.GetTrackerState();
            PeriodTrackerDashboardSnapshot snapshot = new PeriodTrackerDashboardSnapshot
            {
                HasPeriodTracker = trackerState.HasPeriodTracker,
                StartPeriodDate = trackerState.StartPeriodDate.DateTime,
                CycleDays = (int)trackerState.CycleDays,
                PeriodLasts = (int)trackerState.PeriodLasts,
                PMSOption = trackerState.PremenstrualSyndromeOption,
                MonthOffset = monthOffset,
            };

            if (snapshot.HasPeriodTracker && snapshot.CycleDays > 0)
            {
                this.RunCalendarCalculations(snapshot);
                this.PopulateRecommendedProducts(snapshot);
            }

            snapshot.Notes = this.GetNotes()
                .OrderBy(note => note.Key)
                .Select(note => new PeriodTrackerNoteSnapshot
                {
                    NoteId = note.Key,
                    NoteBody = note.Value.Item1,
                    IsDone = note.Value.Item2,
                })
                .ToList();

            return snapshot;
        }

        public Dictionary<int, Tuple<string, bool>> GetNotes()
        {
            User currentUser = this.GetCurrentUser();
            return currentUser?.PeriodNotes ?? new Dictionary<int, Tuple<string, bool>>();
        }

        public int GetMaxNoteId()
        {
            User currentUser = this.GetCurrentUser();

            if (currentUser == null || currentUser.PeriodNotes == null || currentUser.PeriodNotes.Count == 0)
            {
                return 0;
            }

            return currentUser.PeriodNotes.Keys.Max();
        }

        public void UpdatePeriodTracker(DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int premenstrualSyndromeOption)
        {
            User currentUser = this.GetCurrentUser();

            if (currentUser == null)
            {
                return;
            }

            currentUser.SetPeriodTracker(
                DateOnly.FromDateTime(startPeriodDate.DateTime),
                Convert.ToInt32(cycleDays),
                Convert.ToInt32(periodLasts),
                premenstrualSyndromeOption);

            this.SaveCurrentUser();
        }

        public void AddNote(string noteBody)
        {
            User currentUser = this.GetCurrentUser();

            if (currentUser == null)
            {
                return;
            }

            int nextNoteIdentifier = this.GetNextNoteIdentifier();
            string safeNoteBody = noteBody ?? string.Empty;

            currentUser.AddPeriodNoteToUser(nextNoteIdentifier, safeNoteBody, false);
            this.SaveCurrentUser();
        }

        public void UpdateNote(int noteId, string noteBody, bool isDone)
        {
            User currentUser = this.GetCurrentUser();

            if (currentUser == null || currentUser.PeriodNotes == null)
            {
                return;
            }

            string safeNoteBody = noteBody ?? string.Empty;
            currentUser.PeriodNotes[noteId] = new Tuple<string, bool>(safeNoteBody, isDone);

            this.SaveCurrentUser();
        }

        public void DeleteNote(int noteId)
        {
            User currentUser = this.GetCurrentUser();

            if (currentUser == null || currentUser.PeriodNotes == null)
            {
                return;
            }

            if (!currentUser.PeriodNotes.ContainsKey(noteId))
            {
                return;
            }

            currentUser.PeriodNotes.Remove(noteId);
            this.SaveCurrentUser();
        }

        public void SaveCurrentUser()
        {
            User currentUser = this.GetCurrentUser();

            if (currentUser != null)
            {
                this.usersRepository.UpdateUser(currentUser);
            }
        }

        private void RunCalendarCalculations(PeriodTrackerDashboardSnapshot snapshot)
        {
            DateTime today = DateTime.Today;
            DateTime computedStart = snapshot.StartPeriodDate.Date;

            while (computedStart.AddDays(snapshot.CycleDays) <= today)
            {
                computedStart = computedStart.AddDays(snapshot.CycleDays);
            }

            while (computedStart > today)
            {
                computedStart = computedStart.AddDays(-snapshot.CycleDays);
            }

            computedStart = computedStart.AddDays(snapshot.MonthOffset * snapshot.CycleDays);

            DateTime endPeriod = computedStart.AddDays(snapshot.PeriodLasts);
            DateTime startLowFertility = endPeriod.AddDays(1);
            DateTime endLowFertility = computedStart.AddDays(8);
            DateTime startOvulation = computedStart.AddDays(11);
            DateTime endOvulation = computedStart.AddDays(15);
            DateTime nextPeriod = computedStart.AddDays(snapshot.CycleDays);

            snapshot.CurrentMonthName = computedStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
            snapshot.PeriodIntervalText = $"{computedStart.Day} {computedStart:MMMM} - {endPeriod.Day} {endPeriod:MMMM}";
            snapshot.LowFertilityIntervalText = snapshot.PeriodLasts < 8
                ? $"{startLowFertility.Day} {startLowFertility:MMMM} - {endLowFertility.Day} {endLowFertility:MMMM}"
                : "No low fertility days this month";
            snapshot.OvulationIntervalText = $"{startOvulation.Day} {startOvulation:MMMM} - {endOvulation.Day} {endOvulation:MMMM}";

            if (snapshot.PMSOption > 0)
            {
                int pmsOffset = snapshot.PMSOption == 1 ? 3 : (snapshot.PMSOption == 2 ? 6 : 10);
                DateTime startPms = nextPeriod.AddDays(-pmsOffset);
                snapshot.PmsIntervalText = $"{startPms.Day} {startPms:MMMM} - {nextPeriod.Day} {nextPeriod:MMMM}";
            }
            else
            {
                snapshot.PmsIntervalText = "No registered PMS effects";
            }

            snapshot.IsInMenstrualPhase = today >= computedStart && today <= endPeriod;

            if (today >= computedStart && today <= endPeriod)
            {
                snapshot.CurrentPhaseString = "Menstrual Phase";
            }
            else if (today > endPeriod && today < startOvulation)
            {
                snapshot.CurrentPhaseString = "Follicular Phase";
            }
            else if (today >= startOvulation && today <= endOvulation)
            {
                snapshot.CurrentPhaseString = "Ovulation Phase";
            }
            else if (today > endOvulation && today < nextPeriod)
            {
                snapshot.CurrentPhaseString = "Luteal Phase";
            }
            else
            {
                snapshot.CurrentPhaseString = "Out of Scope Cycle";
            }

            snapshot.NextPeriodDateString = nextPeriod.ToString("d");
            snapshot.CurrentDayOfCycle = (int)(today - computedStart).TotalDays + 1;

            double daysLeftPeriod = Math.Max(0, Math.Ceiling((nextPeriod - today).TotalDays));
            snapshot.NextPeriodDistanceString = $"{daysLeftPeriod} days left";

            if (today < startOvulation)
            {
                snapshot.DaysUntilOvulation = (int)Math.Ceiling((startOvulation - today).TotalDays);
                snapshot.OvulationDistanceString = $"In {snapshot.DaysUntilOvulation} days";
            }
            else if (today >= startOvulation && today <= endOvulation)
            {
                snapshot.DaysUntilOvulation = 0;
                snapshot.OvulationDistanceString = "In Progress";
            }
            else
            {
                DateTime nextMonthOvulation = nextPeriod.AddDays(11);
                snapshot.DaysUntilOvulation = (int)Math.Ceiling((nextMonthOvulation - today).TotalDays);
                snapshot.OvulationDistanceString = $"In {snapshot.DaysUntilOvulation} days";
            }
        }

        private void PopulateRecommendedProducts(PeriodTrackerDashboardSnapshot snapshot)
        {
            if (this.wellnessItemsService == null)
            {
                return;
            }

            var wellnessItems = this.wellnessItemsService.GetWellnessItems();
            float discountModifier = snapshot.IsInMenstrualPhase ? 20.0f : 0.0f;

            foreach (var item in wellnessItems)
            {
                float basePrice = (float)item.Price;
                float netPrice = discountModifier > 0 ? basePrice * 0.8f : basePrice;

                snapshot.ShopItems.Add(new PeriodTrackerShopItemSnapshot
                {
                    RawItem = item,
                    DisplayPrice = netPrice,
                    HasDiscountApplied = snapshot.IsInMenstrualPhase,
                });
            }
        }

        private static PeriodTrackerState CreateDefaultTrackerState()
        {
            return new PeriodTrackerState
            {
                StartPeriodDate = new DateTimeOffset(DateTime.Today),
                CycleDays = DefaultCycleLengthInDays,
                PeriodLasts = DefaultPeriodLengthInDays,
                PremenstrualSyndromeOption = DefaultPremenstrualSyndromeOption,
                HasPeriodTracker = false,
            };
        }

        private static DateTime GetTrackerStartDate(User currentUser)
        {
            bool userHasConfiguredStartDate = currentUser.StartPeriodDate.Year != default(DateOnly).Year;

            if (!userHasConfiguredStartDate)
            {
                return DateTime.Today;
            }

            return currentUser.StartPeriodDate.ToDateTime(new TimeOnly(MidnightHour, MidnightMinute));
        }

        private int GetNextNoteIdentifier()
        {
            int maximumExistingNoteIdentifier = this.GetMaxNoteId();

            if (maximumExistingNoteIdentifier == 0)
            {
                return FirstNoteIdentifier;
            }

            return maximumExistingNoteIdentifier + 1;
        }
    }
}