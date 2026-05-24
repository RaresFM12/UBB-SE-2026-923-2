using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Web.ViewModels
{
    public class PeriodTrackerViewModel
    {
        public bool HasPeriodTracker { get; set; }

        [DataType(DataType.Date)]
        public DateOnly StartPeriodDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Range(20, 45)]
        public int CycleDays { get; set; }

        [Range(1, 9)]
        public int PeriodLasts { get; set; }

        [Range(0, 3)]
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

        public List<WebNoteItemViewModel> Notes { get; set; } = new List<WebNoteItemViewModel>();
        public bool CanAddNote => Notes.Count < 4;

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
    public class PeriodTrackerInputModel
    {
        public DateTime StartPeriodDate { get; set; }
        public int CycleDays { get; set; }
        public int PeriodLasts { get; set; }
        public int PMSOption { get; set; }
    }
}