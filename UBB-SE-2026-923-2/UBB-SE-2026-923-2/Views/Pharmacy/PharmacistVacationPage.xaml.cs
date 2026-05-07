namespace UBB_SE_2026_923_2.Views.Pharmacy;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.ViewModels.Pharmacy;

public sealed partial class PharmacistVacationPage : Page
{
    public PharmacistVacationViewModel ViewModel { get; }

    public PharmacistVacationPage()
    {
        InitializeComponent();

        this.ViewModel = App.Services.GetRequiredService<PharmacistVacationViewModel>();
        this.DataContext = this.ViewModel;
        PharmacistComboBox.ItemsSource = this.ViewModel.Pharmacists;
    }

    private void AddVacationShift_Click(object sender, RoutedEventArgs e)
    {
        var selected = PharmacistComboBox.SelectedItem as PharmacistVacationViewModel.PharmacistChoice;

        var result = this.ViewModel.TryRegisterVacation(
            selected,
            StartDatePicker.Date,
            EndDatePicker.Date);

        this.ShowMessage(result.Message, MapSeverity(result.Status));
    }

    private static InfoBarSeverity MapSeverity(VacationRegistrationStatus status) => status switch
    {
        VacationRegistrationStatus.Success => InfoBarSeverity.Success,
        VacationRegistrationStatus.Warning => InfoBarSeverity.Warning,
        _ => InfoBarSeverity.Error,
    };

    private void ShowMessage(string message, InfoBarSeverity severity)
    {
        StatusInfoBar.Message = message;
        StatusInfoBar.Severity = severity;
        StatusInfoBar.IsOpen = true;
    }
}
