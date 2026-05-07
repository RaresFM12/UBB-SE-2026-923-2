namespace UBB_SE_2026_923_2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using UBB_SE_2026_923_2.Command;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Base;

    public class SalaryComputationViewModel : ObservableObject
    {
        private readonly ISalaryComputationService salaryService;

        public ObservableCollection<IStaff> StaffList { get; } = new ObservableCollection<IStaff>();

        public ObservableCollection<Shift> ShiftList { get; } = new ObservableCollection<Shift>();

        private IStaff selectedStaff = default!;

        public IStaff SelectedStaff
        {
            get => this.selectedStaff;
            set
            {
                this.SetProperty(ref this.selectedStaff, value);
                this.ComputeSalaryCommand.RaiseCanExecuteChanged();
            }
        }

        private int selectedMonth = DateTime.Now.Month;

        public int SelectedMonth { get => this.selectedMonth; set => this.SetProperty(ref this.selectedMonth, value); }

        private int selectedYear = DateTime.Now.Year;

        public int SelectedYear { get => this.selectedYear; set => this.SetProperty(ref this.selectedYear, value); }

        private bool isLoading;

        public bool IsLoading { get => this.isLoading; set => this.SetProperty(ref this.isLoading, value); }

        private string errorMessage = string.Empty;

        public string ErrorMessage { get => this.errorMessage; set => this.SetProperty(ref this.errorMessage, value); }

        private string salaryResult = string.Empty;

        public string SalaryResult { get => this.salaryResult; set => this.SetProperty(ref this.salaryResult, value); }

        public AsyncRelayCommand ComputeSalaryCommand { get; }

        public SalaryComputationViewModel(ISalaryComputationService salaryService)
        {
            this.salaryService = salaryService;

            this.ComputeSalaryCommand = new AsyncRelayCommand(this.ComputeSalaryAsync, this.CanComputeSalary);

            this.LoadStaffList();
            this.LoadShiftList();
        }

        public SalaryComputationViewModel(ISalaryComputationService salaryService, IEnumerable<IStaff> staffList, IEnumerable<Shift> shiftList)
        {
            this.salaryService = salaryService;

            this.ComputeSalaryCommand = new AsyncRelayCommand(this.ComputeSalaryAsync, this.CanComputeSalary);

            this.StaffList.ReplaceWith(staffList);
            this.ShiftList.ReplaceWith(shiftList);
        }

        private void LoadStaffList() => this.StaffList.ReplaceWith(this.salaryService.GetAllStaff());

        private void LoadShiftList() => this.ShiftList.ReplaceWith(this.salaryService.GetAllShifts());

        private bool CanComputeSalary()
        {
            return this.SelectedStaff != null && this.SelectedStaff.StaffID > 0;
        }

        private async Task ComputeSalaryAsync()
        {
            this.ErrorMessage = string.Empty;
            this.SalaryResult = string.Empty;
            this.IsLoading = true;

            try
            {
                bool IsStaffShiftForPeriod(Shift shift) =>
                    shift.AppointedStaff?.StaffID == this.SelectedStaff.StaffID
                    && shift.StartTime.Month == this.SelectedMonth
                    && shift.StartTime.Year == this.SelectedYear;

                var staffShiftsForPeriod = this.ShiftList
                    .Where(IsStaffShiftForPeriod)
                    .ToList();

                double computedSalary = 0;

                if (this.SelectedStaff is Models.Doctor doctor)
                {
                    computedSalary = await this.salaryService.ComputeSalaryDoctorAsync(doctor, staffShiftsForPeriod, this.SelectedMonth, this.SelectedYear);
                }
                else if (this.SelectedStaff is Models.Pharmacyst pharmacist)
                {
                    computedSalary = await this.salaryService.ComputeSalaryPharmacistAsync(pharmacist, staffShiftsForPeriod, this.SelectedMonth, this.SelectedYear);
                }
                else
                {
                    throw new InvalidOperationException("Unsupported staff type for salary computation.");
                }

                this.SalaryResult = $"Computed Salary: ${computedSalary:F2}";
            }
            catch (Exception exception)
            {
                this.ErrorMessage = $"Computation failed: {exception.Message}";
            }
            finally
            {
                this.IsLoading = false;
            }
        }
    }
}
