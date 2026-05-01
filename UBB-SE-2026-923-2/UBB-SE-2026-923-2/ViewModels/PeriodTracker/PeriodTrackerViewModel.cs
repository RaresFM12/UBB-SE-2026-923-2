using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;
using Syncfusion.UI.Xaml.Core;

namespace UBB_SE_2026_923_2.ViewModels.PeriodTracker
{
    public class PeriodTrackerViewModel : INotifyPropertyChanged
    {
        private const int MaximumNotesCount = 4;
        private const float MenstrualPhaseExtraDiscountPercentage = 20.0f;
        private const float NoExtraDiscountPercentage = 0.0f;
        private const int ItemsPerRow = 4;

        private readonly IPeriodTrackerService periodTrackerService;
        private readonly IWellnessItemsService wellnessItemsService;
        private readonly IBasketService basketService;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public CalendarsViewModel Calendars { get; }

        public ObservableCollection<NoteViewModel> Notes { get; }

        public ObservableCollection<ItemListViewModel> ItemsLists { get; }

        public ICommand CalculateCommand { get; }
        public ICommand NextCycleCommand { get; }
        public ICommand PreviousCycleCommand { get; }
        public ICommand AddNoteCommand { get; }

        public bool CanAddNote => Notes.Count < MaximumNotesCount;

        public Visibility AddNoteVisibility =>
            CanAddNote ? Visibility.Visible : Visibility.Collapsed;

        private Visibility calendarsVisibility = Visibility.Collapsed;
        public Visibility CalendarsVisibility
        {
            get => calendarsVisibility;
            set
            {
                if (calendarsVisibility == value)
                {
                    return;
                }

                calendarsVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility shopVisibility = Visibility.Collapsed;
        public Visibility ShopVisibility
        {
            get => shopVisibility;
            set
            {
                if (shopVisibility == value)
                {
                    return;
                }

                shopVisibility = value;
                OnPropertyChanged();
            }
        }

        private DateTimeOffset startPeriodDate;
        public DateTimeOffset StartPeriodDate
        {
            get => startPeriodDate;
            set
            {
                if (startPeriodDate == value)
                {
                    return;
                }

                startPeriodDate = value;
                OnPropertyChanged();
            }
        }

        private double cycleDaysInput;
        public double CycleDaysInput
        {
            get => cycleDaysInput;
            set
            {
                if (cycleDaysInput == value)
                {
                    return;
                }

                cycleDaysInput = value;
                OnPropertyChanged();
            }
        }

        private double periodLastsInput;
        public double PeriodLastsInput
        {
            get => periodLastsInput;
            set
            {
                if (periodLastsInput == value)
                {
                    return;
                }

                periodLastsInput = value;
                OnPropertyChanged();
            }
        }

        private int premenstrualSyndromeOptionInput;
        public int PremenstrualSyndromeOptionInput
        {
            get => premenstrualSyndromeOptionInput;
            set
            {
                if (premenstrualSyndromeOptionInput == value)
                {
                    return;
                }

                premenstrualSyndromeOptionInput = value;
                OnPropertyChanged();
            }
        }

        public PeriodTrackerViewModel(
            IPeriodTrackerService periodTrackerService,
            IWellnessItemsService wellnessItemsService,
            IBasketService basketService)
        {
            this.periodTrackerService = periodTrackerService;
            this.wellnessItemsService = wellnessItemsService;
            this.basketService = basketService;

            Calendars = new CalendarsViewModel();
            Notes = new ObservableCollection<NoteViewModel>();
            ItemsLists = new ObservableCollection<ItemListViewModel>();

            CalculateCommand = new DelegateCommand(ignoredParameter => CalculatePeriodTracker());
            NextCycleCommand = new DelegateCommand(ignoredParameter => UpdatePeriodTracker(true));
            PreviousCycleCommand = new DelegateCommand(ignoredParameter => UpdatePeriodTracker(false));
            AddNoteCommand = new DelegateCommand(ignoredParameter => AddNewNote());

            LoadInitialState();
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LoadInitialState()
        {
            PeriodTrackerState trackerState = periodTrackerService.GetTrackerState();

            StartPeriodDate = trackerState.StartPeriodDate;
            CycleDaysInput = trackerState.CycleDays;
            PeriodLastsInput = trackerState.PeriodLasts;
            PremenstrualSyndromeOptionInput = trackerState.PremenstrualSyndromeOption;

            LoadNotes();

            if (trackerState.HasPeriodTracker)
            {
                Calendars.CalculatePeriodTracker(
                    StartPeriodDate.Date,
                    (int)CycleDaysInput,
                    (int)PeriodLastsInput,
                    PremenstrualSyndromeOptionInput);

                CalendarsVisibility = Visibility.Visible;
                BuildItems();
            }
            else
            {
                CalendarsVisibility = Visibility.Collapsed;
                ShopVisibility = Visibility.Collapsed;
            }
        }

        private void LoadNotes()
        {
            Notes.Clear();

            foreach (KeyValuePair<int, Tuple<string, bool>> noteEntry in periodTrackerService
                         .GetNotes()
                         .OrderBy(note => note.Key)
                         .Take(MaximumNotesCount))
            {
                Notes.Add(new NoteViewModel(
                    noteEntry.Key,
                    noteEntry.Value.Item1,
                    noteEntry.Value.Item2,
                    DeleteNote,
                    UpdateNote));
            }

            OnPropertyChanged(nameof(CanAddNote));
            OnPropertyChanged(nameof(AddNoteVisibility));
        }

        private void CalculatePeriodTracker()
        {
            periodTrackerService.UpdatePeriodTracker(
                StartPeriodDate,
                CycleDaysInput,
                PeriodLastsInput,
                PremenstrualSyndromeOptionInput);

            Calendars.CalculatePeriodTracker(
                StartPeriodDate.Date,
                (int)CycleDaysInput,
                (int)PeriodLastsInput,
                PremenstrualSyndromeOptionInput);

            CalendarsVisibility = Visibility.Visible;
            BuildItems();
        }

        private void UpdatePeriodTracker(bool shouldMoveToNextCycle)
        {
            if (CalendarsVisibility != Visibility.Visible)
            {
                return;
            }

            Calendars.UpdatePeriodTracker(shouldMoveToNextCycle);
            BuildItems();
        }

        private void BuildItems()
        {
            ItemsLists.Clear();

            List<Item> wellnessItems = wellnessItemsService.GetWellnessItems();

            if (wellnessItems.Count == 0)
            {
                ShopVisibility = Visibility.Collapsed;
                OnPropertyChanged(nameof(ItemsLists));
                return;
            }

            ShopVisibility = Visibility.Visible;

            float extraDiscountPercentage = Calendars.IsInMenstrualPhase
                ? MenstrualPhaseExtraDiscountPercentage
                : NoExtraDiscountPercentage;

            for (int startIndex = 0; startIndex < wellnessItems.Count; startIndex += ItemsPerRow)
            {
                ItemListViewModel itemRow = new ItemListViewModel();

                foreach (Item currentItem in wellnessItems.Skip(startIndex).Take(ItemsPerRow))
                {
                    itemRow.Items.Add(new ItemViewModel(
                        currentItem,
                        extraDiscountPercentage,
                        basketService));
                }

                ItemsLists.Add(itemRow);
            }

            OnPropertyChanged(nameof(ItemsLists));
        }

        private void AddNewNote()
        {
            if (Notes.Count >= MaximumNotesCount)
            {
                return;
            }

            periodTrackerService.AddNote(string.Empty);
            LoadNotes();
        }

        private void UpdateNote(NoteViewModel note)
        {
            if (note == null)
            {
                return;
            }

            periodTrackerService.UpdateNote(note.NoteId, note.NoteBody, note.NoteIsDone);
        }

        private void DeleteNote(NoteViewModel note)
        {
            if (note == null)
            {
                return;
            }

            periodTrackerService.DeleteNote(note.NoteId);
            LoadNotes();
        }
    }
}