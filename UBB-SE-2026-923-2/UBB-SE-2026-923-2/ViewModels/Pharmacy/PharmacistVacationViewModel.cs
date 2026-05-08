namespace UBB_SE_2026_923_2.ViewModels.Pharmacy
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.Base;

    public sealed class PharmacistVacationViewModel : ObservableObject
    {
        private readonly IPharmacyVacationService pharmacyVacationService;

        public ObservableCollection<PharmacistChoice> Pharmacists { get; } = new ObservableCollection<PharmacistChoice>();

        public PharmacistVacationViewModel(IPharmacyVacationService pharmacyVacationService)
        {
            this.pharmacyVacationService = pharmacyVacationService ?? throw new ArgumentNullException(nameof(pharmacyVacationService));
            this.LoadPharmacists();
        }

        public void LoadPharmacists()
        {
            this.Pharmacists.Clear();
            foreach (var pharmacist in this.pharmacyVacationService.GetPharmacists())
            {
                bool IsNonEmpty(string? namePart) => !string.IsNullOrWhiteSpace(namePart);
                var displayName = string.Join(
                    " ",
                    new[] { pharmacist.FirstName?.Trim(), pharmacist.LastName?.Trim() }
                        .Where(IsNonEmpty));
                this.Pharmacists.Add(new PharmacistChoice(pharmacist, displayName));
            }
        }

        public VacationRegistrationResult TryRegisterVacation(
            PharmacistChoice? pharmacist,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
        {
            if (pharmacist is null)
            {
                return VacationRegistrationResult.Warning("Select a pharmacist first.");
            }

            if (startDate is null || endDate is null)
            {
                return VacationRegistrationResult.Warning("Select both start and end dates.");
            }

            try
            {
                this.pharmacyVacationService.RegisterVacation(
                    pharmacist.Staff.StaffID,
                    startDate.Value.Date,
                    endDate.Value.Date);
                return VacationRegistrationResult.Success("Vacation shift added to repository.");
            }
            catch (ArgumentException exception)
            {
                return VacationRegistrationResult.Error(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return VacationRegistrationResult.Error(exception.Message);
            }
        }

        public sealed record PharmacistChoice(Pharmacyst Staff, string DisplayName);
    }

    public sealed record VacationRegistrationResult(
        VacationRegistrationStatus Status,
        string Message)
    {
        public static VacationRegistrationResult Success(string message) =>
            new VacationRegistrationResult(VacationRegistrationStatus.Success, message);

        public static VacationRegistrationResult Warning(string message) =>
            new VacationRegistrationResult(VacationRegistrationStatus.Warning, message);

        public static VacationRegistrationResult Error(string message) =>
            new VacationRegistrationResult(VacationRegistrationStatus.Error, message);
    }

    public enum VacationRegistrationStatus
    {
        Success,
        Warning,
        Error,
    }
}
