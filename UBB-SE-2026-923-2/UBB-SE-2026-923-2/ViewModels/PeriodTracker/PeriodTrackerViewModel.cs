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

        private string cycleDaysInputText = string.Empty;
        public string CycleDaysInputText
        {
            get => cycleDaysInputText;
            set
            {
                if (cycleDaysInputText == value)
                {
                    return;
                }

                cycleDaysInputText = value;
                OnPropertyChanged();
            }
        }

        private string periodLastsInputText = string.Empty;
        public string PeriodLastsInputText
        {
            get => periodLastsInputText;
            set
            {
                if (periodLastsInputText == value)
                {
                    return;
                }

                periodLastsInputText = value;
                OnPropertyChanged();
            }
        }

        private string validationErrorMessage = string.Empty;
        public string ValidationErrorMessage
        {
            get => validationErrorMessage;
            set
            {
                if (validationErrorMessage == value)
                {
                    return;
                }

                validationErrorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasValidationError));
            }
        }

        public bool HasValidationError => !string.IsNullOrEmpty(validationErrorMessage);

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
            CycleDaysInputText = trackerState.CycleDays > 0 ? trackerState.CycleDays.ToString() : string.Empty;
            PeriodLastsInputText = trackerState.PeriodLasts > 0 ? trackerState.PeriodLasts.ToString() : string.Empty;
            PremenstrualSyndromeOptionInput = trackerState.PremenstrualSyndromeOption;

            LoadNotes();

            if (trackerState.HasPeriodTracker)
            {
                Calendars.CalculatePeriodTracker(
                    StartPeriodDate.Date,
                    trackerState.CycleDays,
                    trackerState.PeriodLasts,
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
            if (!int.TryParse(PeriodLastsInputText, out int periodLasts) || periodLasts < 1 || periodLasts > 9)
            {
                ValidationErrorMessage = "Period length must be a whole number between 1 and 9.";
                return;
            }

            if (!int.TryParse(CycleDaysInputText, out int cycleDays) || cycleDays < 20 || cycleDays > 45)
            {
                ValidationErrorMessage = "Cycle length must be a whole number between 20 and 45.";
                return;
            }

            ValidationErrorMessage = string.Empty;

            periodTrackerService.UpdatePeriodTracker(
                StartPeriodDate,
                cycleDays,
                periodLasts,
                PremenstrualSyndromeOptionInput);

            Calendars.CalculatePeriodTracker(
                StartPeriodDate.Date,
                cycleDays,
                periodLasts,
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