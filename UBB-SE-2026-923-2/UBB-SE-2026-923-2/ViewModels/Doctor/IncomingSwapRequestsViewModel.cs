namespace UBB_SE_2026_923_2.ViewModels.Doctor
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using UBB_SE_2026_923_2.Command;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Base;

    public sealed class IncomingSwapRequestsViewModel : INotifyPropertyChanged
    {
        private readonly IShiftSwapService shiftSwapService;

        public ObservableCollection<IncomingSwapRequestItemViewModel> Requests { get; } = new ObservableCollection<IncomingSwapRequestItemViewModel>();

        public ObservableCollection<DoctorOptionViewModel> Doctors { get; } = new ObservableCollection<DoctorOptionViewModel>();

        private DoctorOptionViewModel? selectedDoctor;

        public DoctorOptionViewModel? SelectedDoctor
        {
            get => this.selectedDoctor;
            set
            {
                if (this.SetProperty(ref this.selectedDoctor, value))
                {
                    this.LoadRequests();
                }
            }
        }

        private string statusMessage = string.Empty;

        public string StatusMessage
        {
            get => this.statusMessage;
            set => this.SetProperty(ref this.statusMessage, value);
        }

        public ICommand RefreshCommand { get; }

        public ICommand AcceptCommand { get; }

        public ICommand RejectCommand { get; }

        public IncomingSwapRequestsViewModel(IShiftSwapService shiftSwapService)
            : this(shiftSwapService, shiftSwapService.GetAllDoctors().Select(DoctorOptionViewModel.From))
        {
        }

        public IncomingSwapRequestsViewModel(IShiftSwapService shiftSwapService, IEnumerable<DoctorOptionViewModel> doctors)
        {
            this.shiftSwapService = shiftSwapService;

            this.Doctors.ReplaceWith(doctors);
            this.SelectedDoctor = this.Doctors.FirstOrDefault();

            bool CanRefresh() => this.SelectedDoctor != null;
            this.RefreshCommand = new RelayCommand(this.LoadRequests, CanRefresh);
            this.AcceptCommand = new RelayCommand(this.AcceptSelected, this.CanProcessSelected);
            this.RejectCommand = new RelayCommand(this.RejectSelected, this.CanProcessSelected);
        }

        private IncomingSwapRequestItemViewModel? selectedRequest;

        public IncomingSwapRequestItemViewModel? SelectedRequest
        {
            get => this.selectedRequest;
            set
            {
                if (this.SetProperty(ref this.selectedRequest, value))
                {
                    (this.AcceptCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (this.RejectCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private bool CanProcessSelected() => this.SelectedDoctor != null && this.SelectedRequest != null;

        private void AcceptSelected()
        {
            if (this.SelectedDoctor == null || this.SelectedRequest == null)
            {
                return;
            }

            var succeeded = this.shiftSwapService.AcceptSwapRequest(this.SelectedRequest.SwapId, this.SelectedDoctor.StaffId, out var resultMessage);
            this.StatusMessage = resultMessage;
            if (succeeded)
            {
                this.LoadRequests();
            }
        }

        private void RejectSelected()
        {
            if (this.SelectedDoctor == null || this.SelectedRequest == null)
            {
                return;
            }

            var succeeded = this.shiftSwapService.RejectSwapRequest(this.SelectedRequest.SwapId, this.SelectedDoctor.StaffId, out var resultMessage);
            this.StatusMessage = resultMessage;
            if (succeeded)
            {
                this.LoadRequests();
            }
        }

        private void LoadRequests()
        {
            if (this.SelectedDoctor == null)
            {
                this.Requests.Clear();
                this.StatusMessage = "Select doctor first.";
                return;
            }

            this.Requests.ReplaceWith(this.shiftSwapService.GetIncomingSwapRequests(this.SelectedDoctor.StaffId)
                .Select(IncomingSwapRequestItemViewModel.From));

            this.StatusMessage = this.Requests.Count == 0 ? "No pending requests." : $"{this.Requests.Count} pending request(s).";
            this.SelectedRequest = this.Requests.FirstOrDefault();
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

    public sealed class IncomingSwapRequestItemViewModel
    {
        public int SwapId { get; set; }

        public int ShiftId { get; set; }

        public int RequesterId { get; set; }

        public DateTime RequestedAt { get; set; }

        public string Status { get; set; } = string.Empty;

        public string DisplayText => $"Request #{this.SwapId} | Shift #{this.ShiftId} | From staff #{this.RequesterId} | {this.RequestedAt:g}";

        public static IncomingSwapRequestItemViewModel From(ShiftSwapRequest request) =>
            new IncomingSwapRequestItemViewModel
            {
                SwapId = request.SwapId,
                ShiftId = request.Shift.Id,
                RequesterId = request.Requester.StaffID,
                RequestedAt = request.RequestedAt,
                Status = request.Status.ToString(),
            };
    }
}
