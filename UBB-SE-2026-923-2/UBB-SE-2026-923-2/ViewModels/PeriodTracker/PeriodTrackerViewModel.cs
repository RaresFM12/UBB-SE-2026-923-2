namespace UBB_SE_2026_923_2.ViewModels.PeriodTracker
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using Microsoft.UI.Xaml;
    using Syncfusion.UI.Xaml.Core;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public class PeriodTrackerViewModel : INotifyPropertyChanged
    {
        private const int MaximumNotesCount = 4;
        private const float MenstrualPhaseExtraDiscountPercentage = 20.0f;
        private const float NoExtraDiscountPercentage = 0.0f;
        private const int ItemsPerRow = 4;

        private readonly IPeriodTrackerService periodTrackerService;
        private readonly IWellnessItemsService wellnessItemsService;
        private readonly IBasketService basketService;

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public CalendarsViewModel Calendars { get; }

        public ObservableCollection<NoteViewModel> Notes { get; }

        public ObservableCollection<ItemListViewModel> ItemsLists { get; }

        public ICommand CalculateCommand { get; }

        public ICommand NextCycleCommand { get; }

        public ICommand PreviousCycleCommand { get; }

        public ICommand AddNoteCommand { get; }

        public bool CanAddNote => this.Notes.Count < MaximumNotesCount;

        public Visibility AddNoteVisibility =>
            this.CanAddNote ? Visibility.Visible : Visibility.Collapsed;

        private Visibility calendarsVisibility = Visibility.Collapsed;

        public Visibility CalendarsVisibility
        {
            get => this.calendarsVisibility;
            set
            {
                if (this.calendarsVisibility == value)
                {
                    return;
                }

                this.calendarsVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility shopVisibility = Visibility.Collapsed;

        public Visibility ShopVisibility
        {
            get => this.shopVisibility;
            set
            {
                if (this.shopVisibility == value)
                {
                    return;
                }

                this.shopVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private DateTimeOffset startPeriodDate;

        public DateTimeOffset StartPeriodDate
        {
            get => this.startPeriodDate;
            set
            {
                if (this.startPeriodDate == value)
                {
                    return;
                }

                this.startPeriodDate = value;
                this.OnPropertyChanged();
            }
        }

        private string cycleDaysInputText = string.Empty;

        public string CycleDaysInputText
        {
            get => this.cycleDaysInputText;
            set
            {
                if (this.cycleDaysInputText == value)
                {
                    return;
                }

                this.cycleDaysInputText = value;
                this.OnPropertyChanged();
            }
        }

        private string periodLastsInputText = string.Empty;

        public string PeriodLastsInputText
        {
            get => this.periodLastsInputText;
            set
            {
                if (this.periodLastsInputText == value)
                {
                    return;
                }

                this.periodLastsInputText = value;
                this.OnPropertyChanged();
            }
        }

        private string validationErrorMessage = string.Empty;

        public string ValidationErrorMessage
        {
            get => this.validationErrorMessage;
            set
            {
                if (this.validationErrorMessage == value)
                {
                    return;
                }

                this.validationErrorMessage = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.HasValidationError));
            }
        }

        public bool HasValidationError => !string.IsNullOrEmpty(this.validationErrorMessage);

        private int premenstrualSyndromeOptionInput;

        public int PremenstrualSyndromeOptionInput
        {
            get => this.premenstrualSyndromeOptionInput;
            set
            {
                if (this.premenstrualSyndromeOptionInput == value)
                {
                    return;
                }

                this.premenstrualSyndromeOptionInput = value;
                this.OnPropertyChanged();
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

            this.Calendars = new CalendarsViewModel();
            this.Notes = new ObservableCollection<NoteViewModel>();
            this.ItemsLists = new ObservableCollection<ItemListViewModel>();

            this.CalculateCommand = new DelegateCommand(ignoredParameter => this.CalculatePeriodTracker());
            this.NextCycleCommand = new DelegateCommand(ignoredParameter => this.UpdatePeriodTracker(true));
            this.PreviousCycleCommand = new DelegateCommand(ignoredParameter => this.UpdatePeriodTracker(false));
            this.AddNoteCommand = new DelegateCommand(ignoredParameter => this.AddNewNote());

            this.LoadInitialState();
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void LoadInitialState()
        {
            PeriodTrackerState trackerState = this.periodTrackerService.GetTrackerState();

            this.StartPeriodDate = trackerState.StartPeriodDate;
            this.CycleDaysInputText = trackerState.CycleDays > 0 ? trackerState.CycleDays.ToString() : string.Empty;
            this.PeriodLastsInputText = trackerState.PeriodLasts > 0 ? trackerState.PeriodLasts.ToString() : string.Empty;
            this.PremenstrualSyndromeOptionInput = trackerState.PremenstrualSyndromeOption;

            this.LoadNotes();

            if (trackerState.HasPeriodTracker)
            {
                this.Calendars.CalculatePeriodTracker(
                    this.StartPeriodDate.Date,
                    trackerState.CycleDays,
                    trackerState.PeriodLasts,
                    this.PremenstrualSyndromeOptionInput);

                this.CalendarsVisibility = Visibility.Visible;
                this.BuildItems();
            }
            else
            {
                this.CalendarsVisibility = Visibility.Collapsed;
                this.ShopVisibility = Visibility.Collapsed;
            }
        }

        private void LoadNotes()
        {
            this.Notes.Clear();

            foreach (KeyValuePair<int, Tuple<string, bool>> noteEntry in this.periodTrackerService
                         .GetNotes()
                         .OrderBy(note => note.Key)
                         .Take(MaximumNotesCount))
            {
                this.Notes.Add(new NoteViewModel(
                    noteEntry.Key,
                    noteEntry.Value.Item1,
                    noteEntry.Value.Item2,
                    this.DeleteNote,
                    this.UpdateNote));
            }

            this.OnPropertyChanged(nameof(this.CanAddNote));
            this.OnPropertyChanged(nameof(this.AddNoteVisibility));
        }

        private void CalculatePeriodTracker()
        {
            if (!int.TryParse(this.PeriodLastsInputText, out int periodLasts) || periodLasts < 1 || periodLasts > 9)
            {
                this.ValidationErrorMessage = "Period length must be a whole number between 1 and 9.";
                return;
            }

            if (!int.TryParse(this.CycleDaysInputText, out int cycleDays) || cycleDays < 20 || cycleDays > 45)
            {
                this.ValidationErrorMessage = "Cycle length must be a whole number between 20 and 45.";
                return;
            }

            this.ValidationErrorMessage = string.Empty;

            this.periodTrackerService.UpdatePeriodTracker(
                this.StartPeriodDate,
                cycleDays,
                periodLasts,
                this.PremenstrualSyndromeOptionInput);

            this.Calendars.CalculatePeriodTracker(
                this.StartPeriodDate.Date,
                cycleDays,
                periodLasts,
                this.PremenstrualSyndromeOptionInput);

            this.CalendarsVisibility = Visibility.Visible;
            this.BuildItems();
        }

        private void UpdatePeriodTracker(bool shouldMoveToNextCycle)
        {
            if (this.CalendarsVisibility != Visibility.Visible)
            {
                return;
            }

            this.Calendars.UpdatePeriodTracker(shouldMoveToNextCycle);
            this.BuildItems();
        }

        private void BuildItems()
        {
            this.ItemsLists.Clear();

            List<Item> wellnessItems = this.wellnessItemsService.GetWellnessItems();

            if (wellnessItems.Count == 0)
            {
                this.ShopVisibility = Visibility.Collapsed;
                this.OnPropertyChanged(nameof(this.ItemsLists));
                return;
            }

            this.ShopVisibility = Visibility.Visible;

            float extraDiscountPercentage = this.Calendars.IsInMenstrualPhase
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
                        this.basketService));
                }

                this.ItemsLists.Add(itemRow);
            }

            this.OnPropertyChanged(nameof(this.ItemsLists));
        }

        private void AddNewNote()
        {
            if (this.Notes.Count >= MaximumNotesCount)
            {
                return;
            }

            this.periodTrackerService.AddNote(string.Empty);
            this.LoadNotes();
        }

        private void UpdateNote(NoteViewModel note)
        {
            if (note == null)
            {
                return;
            }

            this.periodTrackerService.UpdateNote(note.NoteId, note.NoteBody, note.NoteIsDone);
        }

        private void DeleteNote(NoteViewModel note)
        {
            if (note == null)
            {
                return;
            }

            this.periodTrackerService.DeleteNote(note.NoteId);
            this.LoadNotes();
        }
    }
}