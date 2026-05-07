namespace UBB_SE_2026_923_2.Views.Pharmacy;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using UBB_SE_2026_923_2.Configuration;
using UBB_SE_2026_923_2.ViewModels.Pharmacy;

public sealed partial class PharmacySchedulePage : Page
{
    public PharmacyScheduleViewModel ViewModel { get; }

    public PharmacySchedulePage()
    {
        InitializeComponent();

        this.ViewModel = App.Services.GetRequiredService<PharmacyScheduleViewModel>();
        this.DataContext = this.ViewModel;

        this.Loaded += this.PharmacySchedulePage_Loaded;
    }

    private async void PharmacySchedulePage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        this.Loaded -= this.PharmacySchedulePage_Loaded;
        await this.ViewModel.InitializeAsync();
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

        this.ViewModel.AnchorDate = picked;
    }
}
