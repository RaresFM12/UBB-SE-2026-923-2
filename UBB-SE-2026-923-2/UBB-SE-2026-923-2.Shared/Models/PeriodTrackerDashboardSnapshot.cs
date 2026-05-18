using System;
using System.Collections.Generic;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Models
{
    public class PeriodTrackerDashboardSnapshot
    {
        public bool HasPeriodTracker { get; set; }

        public DateTime StartPeriodDate { get; set; }

        public int CycleDays { get; set; }

        public int PeriodLasts { get; set; }

        public int PMSOption { get; set; }

        public int MonthOffset { get; set; }

        public string CurrentMonthName { get; set; }

        public string PeriodIntervalText { get; set; }

        public string LowFertilityIntervalText { get; set; }

        public string OvulationIntervalText { get; set; }

        public string PmsIntervalText { get; set; }

        public string CurrentPhaseString { get; set; }

        public string NextPeriodDateString { get; set; }

        public string NextPeriodDistanceString { get; set; }

        public bool IsInMenstrualPhase { get; set; }

        public int CurrentDayOfCycle { get; set; }

        public int DaysUntilOvulation { get; set; }

        public string OvulationDistanceString { get; set; }

        public List<PeriodTrackerNoteSnapshot> Notes { get; set; } = new List<PeriodTrackerNoteSnapshot>();

        public List<PeriodTrackerShopItemSnapshot> ShopItems { get; set; } = new List<PeriodTrackerShopItemSnapshot>();
    }

    public class PeriodTrackerNoteSnapshot
    {
        public int NoteId { get; set; }

        public string NoteBody { get; set; }

        public bool IsDone { get; set; }
    }

    public class PeriodTrackerShopItemSnapshot
    {
        public Item RawItem { get; set; }

        public float DisplayPrice { get; set; }

        public bool HasDiscountApplied { get; set; }
    }
}