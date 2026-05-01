using UBB_SE_2026_923_2.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using UBB_SE_2026_923_2.ViewModels.Pharmacy;

namespace UBB_SE_2026_923_2.Views.Pharmacy;

public sealed partial class PharmacySchedulePage : Page
{
    public PharmacyScheduleViewModel ViewModel { get; }

    public PharmacySchedulePage()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<PharmacyScheduleViewModel>();
        DataContext = ViewModel;

        Loaded += PharmacySchedulePage_Loaded;
    }

    private async void PharmacySchedulePage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Loaded -= PharmacySchedulePage_Loaded;
        await ViewModel.InitializeAsync();
    }

    private void DateCalendar_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs eventArgs)
    {
        if (sender.SelectedDates == null || sender.SelectedDates.Count == 0)
        {
            return;
        }

        var picked = sender.SelectedDates[0].Date;

        if (picked < AppSettings.SqlMinimumDate)
        {
            return;
        }

        ViewModel.AnchorDate = picked;
    }
}
