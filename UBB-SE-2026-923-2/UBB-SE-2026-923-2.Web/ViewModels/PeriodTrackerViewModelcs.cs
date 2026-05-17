using System;
using System.Collections.Generic;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Web.Models
{
    public class PeriodTrackerViewModel
    {
        // State Configuration Fields
        public bool HasPeriodTracker { get; set; }
        public DateTime StartPeriodDate { get; set; } = DateTime.Today;
        public int CycleDays { get; set; }
        public int PeriodLasts { get; set; }
        public int PMSOption { get; set; }

        // Navigation parameters
        public int MonthOffset { get; set; }
        public string CurrentMonthName { get; set; }

        // Core Dashboard Diagnostics
        public string PeriodIntervalText { get; set; }
        public string LowFertilityIntervalText { get; set; }
        public string OvulationIntervalText { get; set; }
        public string PmsIntervalText { get; set; }

        public string CurrentPhaseString { get; set; }
        public string NextPeriodDateString { get; set; }
        public string NextPeriodDistanceString { get; set; }
        public bool IsInMenstrualPhase { get; set; }
        // Add these to your existing PeriodTrackerViewModel class
        public int CurrentDayOfCycle { get; set; }
        public int DaysUntilOvulation { get; set; }
        public string OvulationDistanceString { get; set; }

        // Notes Feature
        public List<WebNoteItemViewModel> Notes { get; set; } = new List<WebNoteItemViewModel>();
        public bool CanAddNote => Notes.Count < 4;

        // Recommended Items Section
        public List<WebShopItemViewModel> ShopItems { get; set; } = new List<WebShopItemViewModel>();
    }

    public class WebNoteItemViewModel
    {
        public int NoteId { get; set; }
        public string NoteBody { get; set; }
        public bool IsDone { get; set; }
    }

    public class WebShopItemViewModel
    {
        public Item RawItem { get; set; }
        public float DisplayPrice { get; set; }
        public bool HasDiscountApplied { get; set; }
    }
}