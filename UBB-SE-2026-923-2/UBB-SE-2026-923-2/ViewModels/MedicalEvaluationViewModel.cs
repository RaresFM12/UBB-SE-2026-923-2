namespace UBB_SE_2026_923_2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.UI;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Media;
    using UBB_SE_2026_923_2.Command;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Base;

    public partial class MedicalEvaluationViewModel : ObservableObject
    {
        private readonly IMedicalEvaluationService evaluationService;
        private readonly ICurrentUserService currentUserService;
        private List<MedicalEvaluation> allRecords = new List<MedicalEvaluation>();

        public ObservableCollection<MedicalEvaluation> PastEvaluations { get; } = new ObservableCollection<MedicalEvaluation>();

        public ObservableCollection<Appointment> AvailableAppointments { get; } = new ObservableCollection<Appointment>();

        public ObservableCollection<Models.Doctor> AllDoctors { get; } = new ObservableCollection<Models.Doctor>();

        private Models.Doctor? selectedDoctor;

        public Models.Doctor? SelectedDoctor
        {
            get => this.selectedDoctor;
            set
            {
                if (this.SetProperty(ref this.selectedDoctor, value))
                {
                    if (value != null)
                    {
                        this.CurrentDoctorName = $"Dr. {value.FirstName} {value.LastName}";
                        this.InitializeSession();
                    }
                }
            }
        }

        private string currentDoctorName = "Physician";

        public string CurrentDoctorName
        {
            get => this.currentDoctorName;
            set => this.SetProperty(ref this.currentDoctorName, value);
        }

        private Appointment? selectedAppointment;

        public Appointment? SelectedAppointment
        {
            get => this.selectedAppointment;
            set
            {
                if (this.SetProperty(ref this.selectedAppointment, value))
                {
                    if (value != null)
                    {
                        this.PatientId = value.Notes;
                        this.ResetFormForNewSelection();
                    }
                }
            }
        }

        private string patientId = string.Empty;

        public string PatientId
        {
            get => this.patientId;
            set => this.SetProperty(ref this.patientId, value);
        }

        private MedicalEvaluation? selectedEvaluation;

        public MedicalEvaluation? SelectedEvaluation
        {
            get => this.selectedEvaluation;
            set
            {
                if (this.SetProperty(ref this.selectedEvaluation, value))
                {
                    if (value != null)
                    {
                        this.Symptoms = value.Symptoms;
                        this.MedicationsList = value.MedicationsList;
                        this.DoctorNotes = value.Notes;
                        this.PatientId = value.PatientId;
                    }
                    else
                    {
                        this.ResetForm();
                    }

                    this.RaisePropertyChanged(nameof(this.IsEditing));
                    this.DeleteEvaluationCommand.RaiseCanExecuteChanged();
                    this.SaveDiagnosisCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsEditing => this.SelectedEvaluation != null;

        private string searchText = string.Empty;

        public string SearchText
        {
            get => this.searchText;
            set
            {
                if (this.SetProperty(ref this.searchText, value))
                {
                    this.ApplyFilter();
                }
            }
        }

        private string symptoms = string.Empty;

        public string Symptoms
        {
            get => this.symptoms;
            set
            {
                if (this.SetProperty(ref this.symptoms, value))
                {
                    this.RefreshButtonState();
                }
            }
        }

        private string medsList = string.Empty;

        public string MedicationsList
        {
            get => this.medsList;
            set
            {
                if (this.SetProperty(ref this.medsList, value))
                {
                    this.ValidateMedsConflict(value);
                    this.RefreshButtonState();
                }
            }
        }

        private string doctorNotes = string.Empty;

        public string DoctorNotes
        {
            get => this.doctorNotes;
            set
            {
                if (this.SetProperty(ref this.doctorNotes, value))
                {
                    this.RefreshButtonState();
                }
            }
        }

        private string validationError = string.Empty;

        public string ValidationError
        {
            get => this.validationError;
            set => this.SetProperty(ref this.validationError, value);
        }

        private string conflictWarning = string.Empty;

        public string ConflictWarning
        {
            get => this.conflictWarning;
            set => this.SetProperty(ref this.conflictWarning, value);
        }

        private bool isConflictVisible;

        public bool IsConflictVisible
        {
            get => this.isConflictVisible;
            set
            {
                if (this.SetProperty(ref this.isConflictVisible, value))
                {
                    this.RaisePropertyChanged(nameof(this.NotesBackground));
                    this.RaisePropertyChanged(nameof(this.ConflictVisibility));
                    this.IsRiskAssumed = false;
                    this.RefreshButtonState();
                }
            }
        }

        public Visibility ConflictVisibility => this.IsConflictVisible ? Visibility.Visible : Visibility.Collapsed;

        private bool isRiskAssumed;

        public bool IsRiskAssumed
        {
            get => this.isRiskAssumed;
            set
            {
                if (this.SetProperty(ref this.isRiskAssumed, value))
                {
                    this.RefreshButtonState();
                }
            }
        }

        public Brush NotesBackground => this.IsConflictVisible
            ? new SolidColorBrush(Windows.UI.Color.FromArgb(100, 255, 255, 0))
            : new SolidColorBrush(Colors.Transparent);

        private bool isFatigued;

        public bool IsFatigued
        {
            get => this.isFatigued;
            set
            {
                if (this.SetProperty(ref this.isFatigued, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsFormEnabled));
                    this.RaisePropertyChanged(nameof(this.LockoutVisibility));
                    this.RefreshButtonState();
                }
            }
        }

        public bool IsFormEnabled => !this.IsFatigued;

        public Visibility LockoutVisibility => this.IsFatigued ? Visibility.Visible : Visibility.Collapsed;

        private bool isLoading;

        public bool IsLoading
        {
            get => this.isLoading;
            set
            {
                if (this.SetProperty(ref this.isLoading, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsEmptyStateVisible));
                    this.RaisePropertyChanged(nameof(this.EmptyStateVisibility));
                }
            }
        }

        public bool IsEmptyStateVisible => !this.IsLoading && this.PastEvaluations.Count == 0;

        public Visibility EmptyStateVisibility => this.IsEmptyStateVisible ? Visibility.Visible : Visibility.Collapsed;

        public RelayCommand SaveDiagnosisCommand { get; }

        public RelayCommand DeleteEvaluationCommand { get; }

        public MedicalEvaluationViewModel(IMedicalEvaluationService evaluationService, ICurrentUserService currentUserService)
        {
            this.evaluationService = evaluationService;
            this.currentUserService = currentUserService;

            bool CanDelete() => this.IsEditing;
            this.SaveDiagnosisCommand = new RelayCommand(this.SaveDiagnosis, this.CanSaveDiagnosis);
            this.DeleteEvaluationCommand = new RelayCommand(this.ExecuteDeletion, CanDelete);

            this.LoadDoctorList();
            this.InitializeSession();
        }

        private void InitializeSession()
        {
            this.LoadAppointments();
            this.PopulateHistory();
            this.CheckDoctorFatigue();
        }

        private void LoadDoctorList()
        {
            this.AllDoctors.ReplaceWith(this.evaluationService.GetAllDoctors());

            bool IsCurrentUser(Models.Doctor doctor) => doctor.StaffID == this.currentUserService.UserId;
            this.selectedDoctor = this.AllDoctors.FirstOrDefault(IsCurrentUser);
            if (this.selectedDoctor != null)
            {
                this.CurrentDoctorName = $"Dr. {this.selectedDoctor.FirstName} {this.selectedDoctor.LastName}";
            }
        }

        private void LoadAppointments()
        {
            this.AvailableAppointments.ReplaceWith(this.evaluationService.GetAppointmentsByDoctor(this.currentUserService.UserId));
        }

        private void ValidateMedsConflict(string currentMeds)
        {
            if (string.IsNullOrWhiteSpace(currentMeds) || string.IsNullOrWhiteSpace(this.PatientId))
            {
                this.IsConflictVisible = false;
                return;
            }

            string? warning = this.evaluationService.CheckMedicineConflict(this.PatientId, currentMeds);

            if (!string.IsNullOrEmpty(warning))
            {
                this.ConflictWarning = warning;
                this.IsConflictVisible = true;
            }
            else
            {
                this.IsConflictVisible = false;
            }
        }

        private bool CanSaveDiagnosis()
        {
            if (this.IsFatigued)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.PatientId) || this.PatientId == "N/A" || this.PatientId == string.Empty)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.Symptoms) || string.IsNullOrWhiteSpace(this.DoctorNotes))
            {
                this.ValidationError = "?? Symptoms and Doctor Notes are required.";
                return false;
            }

            if (this.IsConflictVisible && !this.IsRiskAssumed)
            {
                this.ValidationError = "?? You must acknowledge the clinical risk.";
                return false;
            }

            this.ValidationError = string.Empty;
            return true;
        }

        private void SaveDiagnosis()
        {
            if (this.IsEditing && this.SelectedEvaluation != null)
            {
                this.SelectedEvaluation.Symptoms = this.Symptoms;
                this.SelectedEvaluation.MedicationsList = this.MedicationsList;
                this.SelectedEvaluation.Notes = this.DoctorNotes;

                this.evaluationService.UpdateEvaluation(this.SelectedEvaluation);
            }
            else
            {
                var newRecord = new MedicalEvaluation
                {
                    PatientId = this.PatientId,
                    Symptoms = this.Symptoms,
                    MedicationsList = this.MedicationsList,
                    Notes = this.DoctorNotes,
                    EvaluationDate = DateTime.Now,
                    Evaluator = new Models.Doctor(
                        this.currentUserService.UserId,
                        string.Empty, string.Empty, string.Empty,
                        true, string.Empty, "Available", DoctorStatus.AVAILABLE, 0),
                };

                this.evaluationService.SaveEvaluation(newRecord);
            }

            this.ResetForm();
            this.PopulateHistory();
        }

        public void ResetForm()
        {
            this.Symptoms = string.Empty;
            this.MedicationsList = string.Empty;
            this.DoctorNotes = string.Empty;
            this.IsRiskAssumed = false;
            this.IsConflictVisible = false;
            this.selectedEvaluation = null;
            this.SelectedAppointment = null;
            this.PatientId = string.Empty;

            this.NotifyAllProperties();
            this.RefreshButtonState();
        }

        private void ResetFormForNewSelection()
        {
            this.Symptoms = string.Empty;
            this.MedicationsList = string.Empty;
            this.DoctorNotes = string.Empty;
            this.IsRiskAssumed = false;
            this.IsConflictVisible = false;
            this.selectedEvaluation = null;

            this.NotifyAllProperties();
            this.RefreshButtonState();
        }

        private void NotifyAllProperties()
        {
            this.RaisePropertyChanged(nameof(this.Symptoms));
            this.RaisePropertyChanged(nameof(this.MedicationsList));
            this.RaisePropertyChanged(nameof(this.DoctorNotes));
            this.RaisePropertyChanged(nameof(this.IsRiskAssumed));
            this.RaisePropertyChanged(nameof(this.IsConflictVisible));
            this.RaisePropertyChanged(nameof(this.ConflictVisibility));
            this.RaisePropertyChanged(nameof(this.NotesBackground));
            this.RaisePropertyChanged(nameof(this.SelectedEvaluation));
            this.RaisePropertyChanged(nameof(this.IsEditing));
            this.RaisePropertyChanged(nameof(this.PatientId));
            this.RaisePropertyChanged(nameof(this.CurrentDoctorName));
            this.RaisePropertyChanged(nameof(this.SelectedDoctor));
            this.RaisePropertyChanged(nameof(this.SelectedAppointment));

            this.DeleteEvaluationCommand.RaiseCanExecuteChanged();
        }

        private void RefreshButtonState()
        {
            this.SaveDiagnosisCommand.RaiseCanExecuteChanged();
            this.DeleteEvaluationCommand.RaiseCanExecuteChanged();
            this.RaisePropertyChanged(nameof(this.ValidationError));
        }

        public async void PopulateHistory()
        {
            this.IsLoading = true;
            this.PastEvaluations.Clear();
            const int SimulatedLoadingDelayMs = 500;
            await Task.Delay(SimulatedLoadingDelayMs);

            this.allRecords = this.evaluationService.GetEvaluationsByDoctor(this.currentUserService.UserId.ToString());

            this.ApplyFilter();
            this.IsLoading = false;
        }

        private void ApplyFilter()
        {
            bool RecordMatchesSearch(MedicalEvaluation record) =>
                record.PatientId.Contains(this.SearchText, StringComparison.OrdinalIgnoreCase);

            var filteredRecords = string.IsNullOrWhiteSpace(this.SearchText)
                ? this.allRecords
                : this.allRecords.Where(RecordMatchesSearch);

            this.PastEvaluations.ReplaceWith(filteredRecords);

            this.RaisePropertyChanged(nameof(this.IsEmptyStateVisible));
            this.RaisePropertyChanged(nameof(this.EmptyStateVisibility));
        }

        private void CheckDoctorFatigue()
        {
            bool wasFatigued = this.IsFatigued;
            this.IsFatigued = this.evaluationService.IsDoctorFatigued(this.currentUserService.UserId.ToString());

            if (this.IsFatigued && !wasFatigued)
            {
                this.evaluationService.RaiseFatigueIntervention(this.currentUserService.UserId, this.CurrentDoctorName);
            }
        }

        public void ExecuteDeletion()
        {
            if (this.SelectedEvaluation == null)
            {
                return;
            }

            this.evaluationService.DeleteEvaluation(this.SelectedEvaluation.EvaluationID);
            this.ResetForm();
            this.PopulateHistory();
        }
    }
}
