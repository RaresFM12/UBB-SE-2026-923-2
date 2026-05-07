namespace UBB_SE_2026_923_2.ViewModels.PeriodTracker
{
    using System;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public static class PeriodTrackerUser
    {
        private static IPeriodTrackerService periodTrackerService;

        public static void Initialize(IPeriodTrackerService injectedPeriodTrackerService)
        {
            periodTrackerService = injectedPeriodTrackerService;
        }

        public static User CurrentUser => periodTrackerService.GetCurrentUser();

        public static DateTimeOffset StartPeriodDate => periodTrackerService.GetTrackerState().StartPeriodDate;

        public static int CycleDays => periodTrackerService.GetTrackerState().CycleDays;

        public static int PeriodLasts => periodTrackerService.GetTrackerState().PeriodLasts;

        public static int PremenstrualSyndromeOption => periodTrackerService.GetTrackerState().PremenstrualSyndromeOption;

        public static bool HasPeriodTracker => periodTrackerService.GetTrackerState().HasPeriodTracker;

        public static int MaxNoteId => periodTrackerService.GetMaxNoteId();

        public static void UpdateUser()
        {
            periodTrackerService.SaveCurrentUser();
        }

        public static void UpdatePeriodTracker(DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int premenstrualSyndromeOption)
        {
            periodTrackerService.UpdatePeriodTracker(startPeriodDate, cycleDays, periodLasts, premenstrualSyndromeOption);
        }
    }
}