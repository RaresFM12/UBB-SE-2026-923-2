namespace UBB_SE_2026_923_2.ViewModels.Doctor
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using UBB_SE_2026_923_2.Command;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Base;

    public sealed class MyScheduleViewModel : INotifyPropertyChanged
    {
        private readonly IShiftSwapService staffAndShiftService;

        public ObservableCollection<DoctorOptionViewModel> Doctors { get; } = new ObservableCollection<DoctorOptionViewModel>();

        public ObservableCollection<DoctorShiftItemViewModel> FutureShifts { get; } = new ObservableCollection<DoctorShiftItemViewModel>();

        public ObservableCollection<StaffOptionViewModel> EligibleColleagues { get; } = new ObservableCollection<StaffOptionViewModel>();

        private DoctorOptionViewModel? selectedDoctor;

        public DoctorOptionViewModel? SelectedDoctor
        {
            get => this.selectedDoctor;
            set
            {
                if (this.SetProperty(ref this.selectedDoctor, value))
                {
                    this.SelectedShift = null;
                    this.SelectedColleague = null;
                    this.LoadFutureShifts();
                    ((RelayCommand)this.RequestSwapCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private DoctorShiftItemViewModel? selectedShift;

        public DoctorShiftItemViewModel? SelectedShift
        {
            get => this.selectedShift;
            set
            {
                if (this.SetProperty(ref this.selectedShift, value))
                {
                    this.LoadEligibleColleagues();
                    ((RelayCommand)this.RequestSwapCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private StaffOptionViewModel? selectedColleague;

        public StaffOptionViewModel? SelectedColleague
        {
            get => this.selectedColleague;
            set
            {
                if (this.SetProperty(ref this.selectedColleague, value))
                {
                    ((RelayCommand)this.RequestSwapCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private string statusMessage = string.Empty;

        public string StatusMessage
        {
            get => this.statusMessage;
            set => this.SetProperty(ref this.statusMessage, value);
        }

        public ICommand RequestSwapCommand { get; }

        public MyScheduleViewModel(IShiftSwapService staffAndShiftService)
        {
            this.staffAndShiftService = staffAndShiftService;

            this.RequestSwapCommand = new RelayCommand(this.RequestSwap, this.CanRequestSwap);

            this.LoadDoctors();
        }

        private void LoadDoctors()
        {
            this.Doctors.ReplaceWith(this.staffAndShiftService.GetAllDoctors().Select(DoctorOptionViewModel.From));

            if (this.Doctors.Count == 0)
            {
                this.StatusMessage = "No doctors found in database.";
                return;
            }

            this.SelectedDoctor = this.Doctors.FirstOrDefault();
        }

        private void LoadFutureShifts()
        {
            this.FutureShifts.Clear();
            this.EligibleColleagues.Clear();

            if (this.SelectedDoctor == null)
            {
                this.StatusMessage = "Select a doctor first.";
                return;
            }

            DoctorShiftItemViewModel ToShiftItemViewModel(Shift shift) => new DoctorShiftItemViewModel(shift);
            this.FutureShifts.ReplaceWith(this.staffAndShiftService
                .GetFutureShiftsForStaff(this.SelectedDoctor.StaffId)
                .Select(ToShiftItemViewModel));

            this.StatusMessage = this.FutureShifts.Count == 0
                ? "Selected doctor has no future shifts available for swap requests."
                : string.Empty;
        }

        private void LoadEligibleColleagues()
        {
            this.EligibleColleagues.Clear();

            if (this.SelectedDoctor == null)
            {
                this.StatusMessage = "Select a doctor first.";
                return;
            }

            if (this.SelectedShift == null)
            {
                this.StatusMessage = "Select a future shift first.";
                return;
            }

            var colleagues = this.staffAndShiftService.GetEligibleSwapColleaguesForShift(
                this.SelectedDoctor.StaffId,
                this.SelectedShift.ShiftId,
                out var error);

            if (!string.IsNullOrWhiteSpace(error))
            {
                this.StatusMessage = error;
                return;
            }

            this.EligibleColleagues.ReplaceWith(colleagues.Select(StaffOptionViewModel.From));

            this.StatusMessage = this.EligibleColleagues.Count == 0
                ? "No colleagues available in the same role/department profile."
                : string.Empty;
        }

        private bool CanRequestSwap()
        {
            return this.SelectedDoctor != null && this.SelectedShift != null && this.SelectedColleague != null;
        }

        private void RequestSwap()
        {
            if (this.SelectedDoctor == null || this.SelectedShift == null || this.SelectedColleague == null)
            {
                this.StatusMessage = "Please select doctor, shift and colleague.";
                return;
            }

            var success = this.staffAndShiftService.RequestShiftSwap(
                this.SelectedDoctor.StaffId,
                this.SelectedShift.ShiftId,
                this.SelectedColleague.StaffId,
                out var message);

            this.StatusMessage = message;

            if (success)
            {
                this.SelectedColleague = null;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }

    public sealed class DoctorOptionViewModel
    {
        public int StaffId { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public static DoctorOptionViewModel From(Models.Doctor doctor) =>
            new DoctorOptionViewModel
            {
                StaffId = doctor.StaffID,
                DisplayName = $"{doctor.FirstName} {doctor.LastName}".Trim(),
            };
    }

    public sealed class StaffOptionViewModel
    {
        public int StaffId { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public static StaffOptionViewModel From(IStaff staffMember) =>
            new StaffOptionViewModel
            {
                StaffId = staffMember.StaffID,
                DisplayName = $"{staffMember.FirstName} {staffMember.LastName}".Trim(),
            };
    }
}
