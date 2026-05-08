namespace UBB_SE_2026_923_2.ViewModels.Doctor
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using UBB_SE_2026_923_2.Command;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Base;
    using UBB_SE_2026_923_2.Views.Shell;

    public class DoctorScheduleViewModel : ObservableObject
    {
        private const string EnglishCultureCode = "en-US";
        private const string DailyDateFormat = "dddd, dd MMM yyyy";
        private const string WeeklyDateFormat = "dd MMM yyyy";
        private const string AppointmentDateFormat = "yyyy-MM-dd";
        private const string AppointmentTimeFormat = @"hh\:mm";
        private const string DoctorRoleLabel = "Doctor";
        private const string AdminRoleLabel = "Admin";
        private const int DaysInWeek = 7;
        private const int OneDay = 1;

        private static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo(EnglishCultureCode);

        private readonly ICurrentUserService currentUser;
        private readonly IDoctorAppointmentService appointmentService;
        private readonly DialogPresenter dialogPresenter;

        private int loadVersion;
        private bool isInitializing;

        public ObservableCollection<AppointmentItemViewModel> Appointments { get; } = new ObservableCollection<AppointmentItemViewModel>();

        public ObservableCollection<DoctorShiftItemViewModel> Shifts { get; } = new ObservableCollection<DoctorShiftItemViewModel>();

        public ObservableCollection<DoctorOption> Doctors { get; } = new ObservableCollection<DoctorOption>();

        public enum ScheduleViewMode
        {
            Daily,
            Weekly,
        }

        private ScheduleViewMode viewMode = ScheduleViewMode.Daily;

        public ScheduleViewMode ViewMode
        {
            get => this.viewMode;
            set
            {
                if (this.SetProperty(ref this.viewMode, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsDaily));
                    this.RaisePropertyChanged(nameof(this.IsWeekly));
                    this.RaisePropertyChanged(nameof(this.SelectedDateText));
                    this.RaisePropertyChanged(nameof(this.PreviousButtonText));
                    this.RaisePropertyChanged(nameof(this.NextButtonText));
                    _ = this.LoadAsync();
                }
            }
        }

        public bool IsDaily => this.ViewMode == ScheduleViewMode.Daily;

        public bool IsWeekly => this.ViewMode == ScheduleViewMode.Weekly;

        public string PreviousButtonText => this.IsWeekly ? "Previous Week" : "Previous";

        public string NextButtonText => this.IsWeekly ? "Next Week" : "Next";

        private DoctorOption? selectedDoctor;

        public DoctorOption? SelectedDoctor
        {
            get => this.selectedDoctor;
            set
            {
                if (this.SetProperty(ref this.selectedDoctor, value) && !this.isInitializing)
                {
                    _ = this.LoadAsync();
                }
            }
        }

        private bool isLoading;

        public bool IsLoading
        {
            get => this.isLoading;
            set
            {
                if (this.SetProperty(ref this.isLoading, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsEmpty));
                }
            }
        }

        private string errorMessage = string.Empty;

        public string ErrorMessage
        {
            get => this.errorMessage;
            set
            {
                if (this.SetProperty(ref this.errorMessage, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsEmpty));
                }
            }
        }

        private DateTime selectedDate = DateTime.Today;

        public DateTime SelectedDate
        {
            get => this.selectedDate;
            set
            {
                if (this.SetProperty(ref this.selectedDate, value))
                {
                    this.RaisePropertyChanged(nameof(this.SelectedDateText));
                    _ = this.LoadAsync();
                }
            }
        }

        public string SelectedDateText => this.IsDaily
            ? this.SelectedDate.ToString(DailyDateFormat, EnglishCulture)
            : $"Week of {StartOfWeek(this.SelectedDate).ToString(WeeklyDateFormat, EnglishCulture)}";

        public bool IsDoctor => string.Equals(this.currentUser.Role, DoctorRoleLabel, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(this.currentUser.Role, AdminRoleLabel, StringComparison.OrdinalIgnoreCase);

        public bool IsAccessDenied => !this.IsDoctor;

        public bool IsEmpty => !this.IsLoading && string.IsNullOrWhiteSpace(this.ErrorMessage) && this.Appointments.Count == 0 && this.Shifts.Count == 0;

        public AsyncRelayCommand RefreshCommand { get; }

        public RelayCommand TodayCommand { get; }

        public RelayCommand NextDayCommand { get; }

        public RelayCommand PreviousDayCommand { get; }

        public RelayCommand DailyModeCommand { get; }

        public RelayCommand WeeklyModeCommand { get; }

        public DoctorScheduleViewModel(
            ICurrentUserService currentUser,
            IDoctorAppointmentService appointmentService,
            DialogPresenter dialogPresenter)
        {
            this.currentUser = currentUser;
            this.appointmentService = appointmentService;
            this.dialogPresenter = dialogPresenter;

            bool CanExecuteAsDoctor() => this.IsDoctor;
            this.RefreshCommand = new AsyncRelayCommand(this.LoadAsync, CanExecuteAsDoctor);

            void SetToday() => this.SelectedDate = DateTime.Today;
            this.TodayCommand = new RelayCommand(SetToday, CanExecuteAsDoctor);

            void GoToNextDay() => this.SelectedDate = this.IsWeekly ? this.SelectedDate.AddDays(DaysInWeek) : this.SelectedDate.AddDays(OneDay);
            this.NextDayCommand = new RelayCommand(GoToNextDay, CanExecuteAsDoctor);

            void GoToPreviousDay() => this.SelectedDate = this.IsWeekly ? this.SelectedDate.AddDays(-DaysInWeek) : this.SelectedDate.AddDays(-OneDay);
            this.PreviousDayCommand = new RelayCommand(GoToPreviousDay, CanExecuteAsDoctor);

            void SetDailyMode() => this.ViewMode = ScheduleViewMode.Daily;
            this.DailyModeCommand = new RelayCommand(SetDailyMode, CanExecuteAsDoctor);

            void SetWeeklyMode() => this.ViewMode = ScheduleViewMode.Weekly;
            this.WeeklyModeCommand = new RelayCommand(SetWeeklyMode, CanExecuteAsDoctor);
        }

        public async Task InitializeAsync()
        {
            this.isInitializing = true;
            this.IsLoading = true;
            this.ErrorMessage = string.Empty;
            this.Appointments.Clear();
            this.Shifts.Clear();

            try
            {
                await this.LoadDoctorsAsync();
            }
            catch (Exception exception)
            {
                this.ErrorMessage = $"Failed to initialize: {exception.Message}";
            }
            finally
            {
                this.isInitializing = false;
            }

            await this.LoadAsync();
        }

        private async Task LoadDoctorsAsync()
        {
            try
            {
                var allDoctors = await this.appointmentService.GetAllDoctorsAsync();
                this.Doctors.ReplaceWith(allDoctors.Select(DoctorOption.From));

                if (this.Doctors.Count == 0)
                {
                    this.ErrorMessage = "No doctors available.";
                    this.SelectedDoctor = null;
                    return;
                }

                bool IsCurrentUserDoctor(DoctorOption doctor) => doctor.DoctorId == this.currentUser.UserId;
                this.SelectedDoctor = this.Doctors.FirstOrDefault(IsCurrentUserDoctor) ?? this.Doctors.FirstOrDefault();
            }
            catch (Exception exception)
            {
                this.ErrorMessage = $"Failed to load doctors: {exception.Message}";
                this.SelectedDoctor = null;
            }
        }

        public async Task LoadAsync()
        {
            int capturedLoadVersion = ++this.loadVersion;

            if (!this.IsDoctor)
            {
                this.ErrorMessage = "Access denied. Only doctors can view schedule.";
                this.Appointments.Clear();
                this.Shifts.Clear();
                this.IsLoading = false;
                this.RaisePropertyChanged(nameof(this.IsAccessDenied));
                this.RaisePropertyChanged(nameof(this.IsEmpty));
                return;
            }

            try
            {
                this.IsLoading = true;

                if (this.SelectedDoctor is null)
                {
                    this.Appointments.Clear();
                    this.Shifts.Clear();
                    this.IsLoading = false;
                    this.RaisePropertyChanged(nameof(this.IsEmpty));
                    return;
                }

                this.ErrorMessage = string.Empty;

                var doctorId = this.SelectedDoctor.DoctorId;
                DateTime from = this.IsDaily ? this.SelectedDate.Date : StartOfWeek(this.SelectedDate);
                DateTime to = this.IsDaily ? from.AddDays(OneDay) : from.AddDays(DaysInWeek);

                var filteredAppointments = await this.appointmentService.GetAppointmentsInRangeAsync(doctorId, from, to);
                var filteredShifts = await this.appointmentService.GetShiftsForStaffInRangeAsync(doctorId, from, to);

                if (capturedLoadVersion != this.loadVersion)
                {
                    return;
                }

                AppointmentItemViewModel ToAppointmentItem(Appointment appointment) => new AppointmentItemViewModel(appointment);
                DoctorShiftItemViewModel ToShiftItem(Shift shift) => new DoctorShiftItemViewModel(shift);

                this.Appointments.ReplaceWith(filteredAppointments.Select(ToAppointmentItem));
                this.Shifts.ReplaceWith(filteredShifts.Select(ToShiftItem));
            }
            catch (Exception exception)
            {
                if (capturedLoadVersion == this.loadVersion)
                {
                    this.ErrorMessage = $"Failed to load schedule: {exception.Message}";
                }
            }
            finally
            {
                if (capturedLoadVersion == this.loadVersion)
                {
                    this.IsLoading = false;
                    this.RaisePropertyChanged(nameof(this.IsAccessDenied));
                    this.RaisePropertyChanged(nameof(this.IsEmpty));
                }
            }
        }

        public async void OpenDetails(AppointmentItemViewModel? item)
        {
            if (item is null)
            {
                return;
            }

            try
            {
                var appointmentDetails = await this.appointmentService.GetAppointmentDetailsAsync(item.AppointmentItemId);
                if (appointmentDetails is null)
                {
                    await this.dialogPresenter.ShowMessageAsync("Details", "Appointment not found.");
                    return;
                }

                var patientLine = string.IsNullOrWhiteSpace(item.PatientName) ? "Patient hidden/unknown" : item.PatientName;
                var typeLine = string.IsNullOrWhiteSpace(appointmentDetails.Type) ? "N/A" : appointmentDetails.Type;
                var locationLine = string.IsNullOrWhiteSpace(appointmentDetails.Location) ? "Location TBD" : appointmentDetails.Location;
                var statusLine = string.IsNullOrWhiteSpace(appointmentDetails.Status) ? "Unknown" : appointmentDetails.Status;
                var formattedDate = appointmentDetails.Date.ToString(AppointmentDateFormat);
                var formattedStartTime = appointmentDetails.StartTime.ToString(AppointmentTimeFormat);
                var formattedEndTime = appointmentDetails.EndTime.ToString(AppointmentTimeFormat);

                var text =
                    $"Patient: {patientLine}\n" +
                    $"Type: {typeLine}\n" +
                    $"Location: {locationLine}\n" +
                    $"Time: {formattedDate} {formattedStartTime}-{formattedEndTime}\n" +
                    $"Status: {statusLine}";

                await this.dialogPresenter.ShowMessageAsync("Appointment Details", text);
            }
            catch (Exception exception)
            {
                await this.dialogPresenter.ShowMessageAsync("Details", $"Failed to load details: {exception.Message}");
            }
        }

        private static DateTime StartOfWeek(DateTime date)
        {
            var daysFromMonday = (DaysInWeek + (date.DayOfWeek - DayOfWeek.Monday)) % DaysInWeek;
            return date.Date.AddDays(-daysFromMonday);
        }

        public sealed class DoctorOption
        {
            private const char NameSeparator = ' ';

            public int DoctorId { get; set; }

            public string DoctorName { get; set; } = string.Empty;

            public string FirstName { get; set; } = string.Empty;

            public string LastName { get; set; } = string.Empty;

            public static DoctorOption From((int DoctorId, string DoctorName) doctor) =>
                new DoctorOption
                {
                    DoctorId = doctor.DoctorId,
                    DoctorName = doctor.DoctorName,
                };

            public string DisplayName
            {
                get
                {
                    bool IsNonEmpty(string? namePart) => !string.IsNullOrWhiteSpace(namePart);
                    return string.Join(NameSeparator, new[] { this.FirstName?.Trim(), this.LastName?.Trim() }.Where(IsNonEmpty));
                }
            }

            public static (string FirstName, string LastName) SplitFirstLast(string? fullName)
            {
                const int SingleNamePartCount = 1;
                const int FirstNamePartIndex = 0;

                if (string.IsNullOrWhiteSpace(fullName))
                {
                    return (string.Empty, string.Empty);
                }

                var parts = fullName
                    .Split(NameSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (parts.Length == SingleNamePartCount)
                {
                    return (parts[FirstNamePartIndex], string.Empty);
                }

                return (parts[FirstNamePartIndex], parts[^SingleNamePartCount]);
            }
        }
    }
}
