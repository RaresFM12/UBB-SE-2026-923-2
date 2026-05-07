namespace UBB_SE_2026_923_2.ViewModels.Pharmacy;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UBB_SE_2026_923_2.Command;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.ViewModels.Base;

public class PharmacyScheduleViewModel : ObservableObject
{
    private const string PharmacistRoleLabel = "Pharmacist";
    private const string AdminRoleLabel = "Admin";
    private const string DailyDateFormat = "dddd, dd MMM yyyy";
    private const string WeeklyDateFormat = "dd MMM yyyy";
    private const int DaysInWeek = 7;
    private const int OneDay = 1;

    private readonly ICurrentUserService currentUser;
    private readonly IPharmacyScheduleService scheduleService;
    private bool isInitializing;

    public ObservableCollection<PharmacyShiftItemViewModel> Shifts { get; } = new ObservableCollection<PharmacyShiftItemViewModel>();

    public ObservableCollection<PharmacistOption> Pharmacists { get; } = new ObservableCollection<PharmacistOption>();

    private PharmacistOption? selectedPharmacist;

    public PharmacistOption? SelectedPharmacist
    {
        get => this.selectedPharmacist;
        set
        {
            if (this.SetProperty(ref this.selectedPharmacist, value) && !this.isInitializing)
            {
                _ = this.LoadAsync();
            }
        }
    }

    private bool isLoading;

    public bool IsLoading { get => this.isLoading; set => this.SetProperty(ref this.isLoading, value); }

    private string errorMessage = string.Empty;

    public string ErrorMessage { get => this.errorMessage; set => this.SetProperty(ref this.errorMessage, value); }

    private DateTime anchorDate = DateTime.Today;

    public DateTime AnchorDate
    {
        get => this.anchorDate;
        set
        {
            if (this.SetProperty(ref this.anchorDate, value))
            {
                this.RaisePropertyChanged(nameof(this.HeaderSubtitle));
                this.RaisePropertyChanged(nameof(this.SelectedDateText));
                _ = this.LoadAsync();
            }
        }
    }

    private bool isWeeklyView = true;

    public bool IsWeeklyView
    {
        get => this.isWeeklyView;
        set
        {
            if (this.SetProperty(ref this.isWeeklyView, value))
            {
                this.RaisePropertyChanged(nameof(this.IsDailyView));
                this.RaisePropertyChanged(nameof(this.HeaderSubtitle));
                this.RaisePropertyChanged(nameof(this.SelectedDateText));
                _ = this.LoadAsync();
            }
        }
    }

    public bool IsDailyView
    {
        get => !this.isWeeklyView;
        set => this.IsWeeklyView = !value;
    }

    public string HeaderSubtitle
    {
        get
        {
            const int LastDayOfWeekOffset = DaysInWeek - 1;
            return this.IsWeeklyView
                ? $"Week of {StartOfWeek(this.AnchorDate).ToString(WeeklyDateFormat)} – {StartOfWeek(this.AnchorDate).AddDays(LastDayOfWeekOffset).ToString(WeeklyDateFormat)}"
                : this.AnchorDate.ToString(DailyDateFormat);
        }
    }

    public string SelectedDateText =>
        this.IsWeeklyView
            ? $"Week of {StartOfWeek(this.AnchorDate).ToString(WeeklyDateFormat)}"
            : this.AnchorDate.ToString(DailyDateFormat);

    public bool IsPharmacist => string.Equals(this.currentUser.Role, PharmacistRoleLabel, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(this.currentUser.Role, AdminRoleLabel, StringComparison.OrdinalIgnoreCase);

    public bool IsAccessDenied => !this.IsPharmacist;

    public bool IsEmpty => !this.IsLoading && string.IsNullOrWhiteSpace(this.ErrorMessage) && this.Shifts.Count == 0;

    public AsyncRelayCommand RefreshCommand { get; }

    public RelayCommand TodayCommand { get; }

    public RelayCommand NextPeriodCommand { get; }

    public RelayCommand PreviousPeriodCommand { get; }

    public RelayCommand ShowDailyCommand { get; }

    public RelayCommand ShowWeeklyCommand { get; }

    public PharmacyScheduleViewModel(
        ICurrentUserService currentUser,
        IPharmacyScheduleService scheduleService)
    {
        this.currentUser = currentUser;
        this.scheduleService = scheduleService;

        bool CanExecuteAsPharmacist() => this.IsPharmacist;
        this.RefreshCommand = new AsyncRelayCommand(this.LoadAsync, CanExecuteAsPharmacist);

        void SetToday() => this.AnchorDate = DateTime.Today;
        this.TodayCommand = new RelayCommand(SetToday, CanExecuteAsPharmacist);

        void GoToNextPeriod() => this.AnchorDate = this.IsWeeklyView ? this.AnchorDate.AddDays(DaysInWeek) : this.AnchorDate.AddDays(OneDay);
        this.NextPeriodCommand = new RelayCommand(GoToNextPeriod, CanExecuteAsPharmacist);

        void GoToPreviousPeriod() => this.AnchorDate = this.IsWeeklyView ? this.AnchorDate.AddDays(-DaysInWeek) : this.AnchorDate.AddDays(-OneDay);
        this.PreviousPeriodCommand = new RelayCommand(GoToPreviousPeriod, CanExecuteAsPharmacist);

        void ShowDaily() => this.IsWeeklyView = false;
        this.ShowDailyCommand = new RelayCommand(ShowDaily, CanExecuteAsPharmacist);

        void ShowWeekly() => this.IsWeeklyView = true;
        this.ShowWeeklyCommand = new RelayCommand(ShowWeekly, CanExecuteAsPharmacist);
    }

    public async Task InitializeAsync()
    {
        this.isInitializing = true;
        try
        {
            await this.LoadPharmacistsAsync();
        }
        finally
        {
            this.isInitializing = false;
        }

        await this.LoadAsync();
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var normalizedDate = date.Date;
        var daysFromMonday = (DaysInWeek + (int)normalizedDate.DayOfWeek - (int)DayOfWeek.Monday) % DaysInWeek;
        return normalizedDate.AddDays(-daysFromMonday);
    }

    public async Task LoadAsync()
    {
        if (!this.IsPharmacist)
        {
            this.ErrorMessage = string.Empty;
            this.Shifts.Clear();
            this.RaisePropertyChanged(nameof(this.IsAccessDenied));
            this.RaisePropertyChanged(nameof(this.IsEmpty));
            return;
        }

        try
        {
            this.IsLoading = true;
            this.ErrorMessage = string.Empty;
            this.Shifts.Clear();

            if (this.SelectedPharmacist is null)
            {
                this.IsLoading = false;
                this.RaisePropertyChanged(nameof(this.IsEmpty));
                return;
            }

            var rangeStart = this.IsWeeklyView ? StartOfWeek(this.AnchorDate) : this.AnchorDate.Date;
            var rangeEnd = this.IsWeeklyView ? rangeStart.AddDays(DaysInWeek) : rangeStart.AddDays(OneDay);

            var staffId = this.SelectedPharmacist.StaffId;
            var rawShifts = await this.scheduleService.GetShiftsAsync(staffId, rangeStart, rangeEnd);

            PharmacyShiftItemViewModel ToShiftViewModel(Shift rawShift) => new PharmacyShiftItemViewModel(rawShift);
            foreach (var shiftViewModel in rawShifts.Select(ToShiftViewModel))
            {
                this.Shifts.Add(shiftViewModel);
            }
        }
        catch (Exception exception)
        {
            this.ErrorMessage = $"Failed to load pharmacy schedule: {exception.Message}";
        }
        finally
        {
            this.IsLoading = false;
            this.RaisePropertyChanged(nameof(this.IsAccessDenied));
            this.RaisePropertyChanged(nameof(this.IsEmpty));
        }
    }

    private async Task LoadPharmacistsAsync()
    {
        this.Pharmacists.Clear();
        var allPharmacists = await Task.Run(() => this.scheduleService.GetPharmacists());

        string GetPharmacistFirstName(Pharmacyst pharmacist) => pharmacist.FirstName;
        string GetPharmacistLastName(Pharmacyst pharmacist) => pharmacist.LastName;
        bool IsNonEmpty(string? namePart) => !string.IsNullOrWhiteSpace(namePart);

        foreach (var pharmacist in allPharmacists
            .OrderBy(GetPharmacistFirstName)
            .ThenBy(GetPharmacistLastName))
        {
            this.Pharmacists.Add(new PharmacistOption
            {
                StaffId = pharmacist.StaffID,
                PharmacistName = string.Join(" ", new[] { pharmacist.FirstName?.Trim(), pharmacist.LastName?.Trim() }
                    .Where(IsNonEmpty)),
            });
        }

        if (this.Pharmacists.Count == 0)
        {
            this.ErrorMessage = "No pharmacists available.";
            this.SelectedPharmacist = null;
            return;
        }

        bool IsCurrentUser(PharmacistOption pharmacist) => pharmacist.StaffId == this.currentUser.UserId;
        this.SelectedPharmacist = this.Pharmacists.FirstOrDefault(IsCurrentUser)
            ?? this.Pharmacists.First();
    }

    public sealed class PharmacistOption
    {
        public int StaffId { get; set; }

        public string PharmacistName { get; set; } = string.Empty;
    }
}
