using UBB_SE_2026_923_2.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using UBB_SE_2026_923_2.ViewModels.Pharmacy;

namespace UBB_SE_2026_923_2.Views.Pharmacy;

public sealed partial class PharmacistVacationPage : Page
{
    public PharmacistVacationViewModel ViewModel { get; }

    public PharmacistVacationPage()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<PharmacistVacationViewModel>();
        DataContext = ViewModel;
        PharmacistComboBox.ItemsSource = ViewModel.Pharmacists;
    }

    private void AddVacationShift_Click(object sender, RoutedEventArgs e)
    {
        var selected = PharmacistComboBox.SelectedItem as PharmacistVacationViewModel.PharmacistChoice;

        var result = ViewModel.TryRegisterVacation(
            selected,
            StartDatePicker.Date,
            EndDatePicker.Date);

        ShowMessage(result.message, MapSeverity(result.status));
    }

    private static InfoBarSeverity MapSeverity(VacationRegistrationStatus status) => status switch
    {
        VacationRegistrationStatus.Success => InfoBarSeverity.Success,
        VacationRegistrationStatus.Warning => InfoBarSeverity.Warning,
        _ => InfoBarSeverity.Error
    };

    private void ShowMessage(string message, InfoBarSeverity severity)
    {
        StatusInfoBar.Message = message;
        StatusInfoBar.Severity = severity;
        StatusInfoBar.IsOpen = true;
    }
}
