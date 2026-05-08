namespace UBB_SE_2026_923_2.ViewModels.Doctor
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using UBB_SE_2026_923_2.Command;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Base;

    public class HangoutViewModel : ObservableObject
    {
        private const int MinimumHangoutTitleLength = 5;
        private const int MaximumHangoutTitleLength = 25;
        private const int MaximumHangoutDescriptionLength = 100;
        private const int MinimumDaysAheadForHangoutCreation = 7;

        private readonly IHangoutService hangoutService;
        private readonly IDoctorAppointmentService? doctorService;

        public ObservableCollection<int> MaximumParticipantsOptions { get; } = new ObservableCollection<int> { 2, 3, 4, 5, 10, 15, 20 };

        public ObservableCollection<Hangout> Hangouts { get; } = new ObservableCollection<Hangout>();

        public ObservableCollection<DoctorScheduleViewModel.DoctorOption> Doctors { get; } = new ObservableCollection<DoctorScheduleViewModel.DoctorOption>();

        private DoctorScheduleViewModel.DoctorOption? selectedDoctor;

        public DoctorScheduleViewModel.DoctorOption? SelectedDoctor
        {
            get => this.selectedDoctor;
            set
            {
                this.SetProperty(ref this.selectedDoctor, value);
                this.CreateCommand.RaiseCanExecuteChanged();
            }
        }

        private string title = string.Empty;

        public string Title
        {
            get => this.title;
            set
            {
                this.SetProperty(ref this.title, value);
                this.CreateCommand.RaiseCanExecuteChanged();
            }
        }

        private string description = string.Empty;

        public string Description
        {
            get => this.description;
            set
            {
                this.SetProperty(ref this.description, value);
                this.CreateCommand.RaiseCanExecuteChanged();
            }
        }

        private DateTimeOffset selectedDate = DateTimeOffset.Now.AddDays(MinimumDaysAheadForHangoutCreation);

        public DateTimeOffset SelectedDate
        {
            get => this.selectedDate;
            set
            {
                this.SetProperty(ref this.selectedDate, value);
                this.CreateCommand.RaiseCanExecuteChanged();
            }
        }

        private int maximumParticipants = 5;

        public int MaximumParticipants
        {
            get => this.maximumParticipants;
            set
            {
                this.SetProperty(ref this.maximumParticipants, value);
                this.CreateCommand.RaiseCanExecuteChanged();
            }
        }

        private string errorMessage = string.Empty;

        public string ErrorMessage
        {
            get => this.errorMessage;
            set => this.SetProperty(ref this.errorMessage, value);
        }

        private string successMessage = string.Empty;

        public string SuccessMessage
        {
            get => this.successMessage;
            set => this.SetProperty(ref this.successMessage, value);
        }

        public RelayCommand CreateCommand { get; }

        public HangoutViewModel(IHangoutService hangoutService, IDoctorAppointmentService doctorService)
        {
            this.hangoutService = hangoutService;
            this.doctorService = doctorService;

            this.CreateCommand = new RelayCommand(this.CreateHangout, this.CanCreateHangout);
            this.LoadHangouts();
            _ = this.LoadDoctorsAsync();
        }

        public HangoutViewModel(IHangoutService hangoutService)
        {
            this.hangoutService = hangoutService;
            this.doctorService = null;
            this.CreateCommand = new RelayCommand(this.CreateHangout, this.CanCreateHangout);
            this.LoadHangouts();
        }

        private async Task LoadDoctorsAsync()
        {
            if (this.doctorService == null)
            {
                return;
            }

            this.Doctors.Clear();
            try
            {
                var allDoctors = await this.doctorService.GetAllDoctorsAsync();
                string GetDoctorName((int DoctorId, string DoctorName) doctor) => doctor.DoctorName;
                foreach (var doctor in allDoctors.OrderBy(GetDoctorName))
                {
                    this.Doctors.Add(new DoctorScheduleViewModel.DoctorOption
                    {
                        DoctorId = doctor.DoctorId,
                        DoctorName = doctor.DoctorName,
                        FirstName = DoctorScheduleViewModel.DoctorOption.SplitFirstLast(doctor.DoctorName).FirstName,
                        LastName = DoctorScheduleViewModel.DoctorOption.SplitFirstLast(doctor.DoctorName).LastName,
                    });
                }

                if (this.Doctors.Any())
                {
                    this.SelectedDoctor = this.Doctors.First();
                }
            }
            catch (Exception exception)
            {
                this.ErrorMessage = $"Failed to load doctors: {exception.Message}";
            }
        }

        private void LoadHangouts()
        {
            this.Hangouts.Clear();
            foreach (var hangout in this.hangoutService.GetAllHangouts())
            {
                this.Hangouts.Add(hangout);
            }
        }

        private bool CanCreateHangout() =>
            this.Title.Length >= MinimumHangoutTitleLength &&
            this.Title.Length <= MaximumHangoutTitleLength &&
            this.Description.Length <= MaximumHangoutDescriptionLength &&
            this.SelectedDoctor != null;

        private void CreateHangout()
        {
            this.ErrorMessage = string.Empty;
            this.SuccessMessage = string.Empty;
            try
            {
                var currentDoctor = new Models.Doctor
                {
                    StaffID = this.SelectedDoctor!.DoctorId,
                    FirstName = this.SelectedDoctor.FirstName,
                    LastName = this.SelectedDoctor.LastName,
                };

                this.hangoutService.CreateHangout(this.Title, this.Description, this.SelectedDate.DateTime, this.MaximumParticipants, currentDoctor);
                this.SuccessMessage = "Hangout created successfully!";
                this.LoadHangouts();

                this.Title = string.Empty;
                this.Description = string.Empty;
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
        }

        public void JoinHangoutById(int hangoutId)
        {
            this.ErrorMessage = string.Empty;
            this.SuccessMessage = string.Empty;

            if (this.SelectedDoctor == null)
            {
                this.ErrorMessage = "Please select a doctor to join the hangout.";
                return;
            }

            try
            {
                var currentDoctor = new Models.Doctor
                {
                    StaffID = this.SelectedDoctor.DoctorId,
                    FirstName = this.SelectedDoctor.FirstName,
                    LastName = this.SelectedDoctor.LastName,
                };

                this.hangoutService.JoinHangout(hangoutId, currentDoctor);
                this.SuccessMessage = "Joined hangout successfully!";
                this.LoadHangouts();
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
        }
    }
}
