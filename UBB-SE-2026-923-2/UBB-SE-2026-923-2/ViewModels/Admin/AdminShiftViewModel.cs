namespace UBB_SE_2026_923_2.ViewModels.Admin
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Base;

    public class AdminShiftViewModel : INotifyPropertyChanged
    {
        private readonly IShiftManagementService staffAndShiftService;
        private const int DAYSINWEEK = 7;

        public ObservableCollection<Shift> Shifts { get; set; } = new ObservableCollection<Shift>();

        public ObservableCollection<IStaff> AvailableStaff { get; set; } = new ObservableCollection<IStaff>();

        public ObservableCollection<string> SpecializationsAndCertifications { get; set; } = new ObservableCollection<string>();

        private DateTime selectedDate = DateTime.Today;

        public DateTime SelectedDate
        {
            get => this.selectedDate;
            set
            {
                if (this.selectedDate != value)
                {
                    this.selectedDate = value;
                    this.OnPropertyChanged(nameof(this.SelectedDate));
                    this.LoadAndFilterShifts();
                }
            }
        }

        private string selectedDepartment = "All Departments";

        public string SelectedDepartment
        {
            get => this.selectedDepartment;
            set
            {
                if (this.selectedDepartment != value)
                {
                    this.selectedDepartment = value;
                    this.OnPropertyChanged(nameof(this.SelectedDepartment));
                    this.LoadAndFilterShifts();
                }
            }
        }

        private bool isWeeklyView;

        public bool IsWeeklyView
        {
            get => this.isWeeklyView;
            set
            {
                if (this.isWeeklyView != value)
                {
                    this.isWeeklyView = value;
                    this.OnPropertyChanged(nameof(this.IsWeeklyView));
                    this.LoadAndFilterShifts();
                }
            }
        }

        private string scheduleTitle = string.Empty;

        public string ScheduleTitle
        {
            get => this.scheduleTitle;
            set
            {
                if (this.scheduleTitle != value)
                {
                    this.scheduleTitle = value;
                    this.OnPropertyChanged(nameof(this.ScheduleTitle));
                }
            }
        }

        public AdminShiftViewModel(IShiftManagementService service)
        {
            this.staffAndShiftService = service;
            this.LoadAndFilterShifts();
        }

        public void LoadAndFilterShifts()
        {
            var rawShifts = this.staffAndShiftService.GetWeeklyShifts(this.SelectedDate);
            IEnumerable<Shift> filtered = rawShifts;
            var englishCulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

            if (this.IsWeeklyView)
            {
                int diff = (DAYSINWEEK + (this.SelectedDate.DayOfWeek - DayOfWeek.Monday)) % DAYSINWEEK;
                DateTime startOfWeek = this.SelectedDate.Date.AddDays(-diff);
                this.ScheduleTitle = $"Weekly Roster (Week of {startOfWeek.ToString("dd MMM yyyy", englishCulture)})";
            }
            else
            {
                bool IsShiftOnSelectedDate(Shift shift) => shift.StartTime.Date == this.SelectedDate.Date;
                filtered = filtered.Where(IsShiftOnSelectedDate);
                this.ScheduleTitle = $"Daily Roster ({this.SelectedDate.ToString("dddd, dd MMM yyyy", englishCulture)})";
            }

            if (!string.IsNullOrEmpty(this.SelectedDepartment) && this.SelectedDepartment != "All Departments")
            {
                bool IsShiftInSelectedDepartment(Shift shift) => shift.Location == this.SelectedDepartment;
                filtered = filtered.Where(IsShiftInSelectedDepartment);
            }

            DateTime SortByStartTime(Shift shift) => shift.StartTime;
            this.Shifts.ReplaceWith(filtered.OrderBy(SortByStartTime));
        }

        public void FilterSpecializationsAndCertificationsForLocation(string location)
        {
            this.SpecializationsAndCertifications.ReplaceWith(
                this.staffAndShiftService.GetSpecializationsAndCertificationsForLocation(location));
        }

        public void FilterStaffForShift(string location, string requiredSpecializationOrCertification)
        {
            this.AvailableStaff.ReplaceWith(
                this.staffAndShiftService.GetFilteredStaff(location, requiredSpecializationOrCertification));
        }

        public void CreateNewShift(IStaff staff, DateTime start, DateTime end, string location)
        {
            bool isAdded = this.staffAndShiftService.TryAddShift(staff, start, end, location);
            if (isAdded)
            {
                this.LoadAndFilterShifts();
            }
        }

        public void SetShiftActive(int shiftID)
        {
            this.staffAndShiftService.SetShiftActive(shiftID);
            this.LoadAndFilterShifts();
        }

        public void ReassignShift(Shift shift, IStaff newStaff)
        {
            bool isSuccessful = this.staffAndShiftService.ReassignShift(shift, newStaff);
            if (isSuccessful)
            {
                this.LoadAndFilterShifts();
            }
        }

        public void CancelShift(int shiftID)
        {
            this.staffAndShiftService.CancelShift(shiftID);
            this.LoadAndFilterShifts();
        }

        public void AutoFindReplacement(Shift shift)
        {
            var replacementsList = this.staffAndShiftService.FindStaffReplacements(shift);
            if (replacementsList != null && replacementsList.Count > 0)
            {
                this.ReassignShift(shift, replacementsList.First());
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
