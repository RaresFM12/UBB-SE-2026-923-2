namespace UBB_SE_2026_923_2.Views.Admin
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using UBB_SE_2026_923_2.ViewModels.Admin;

    public sealed partial class FatigueAuditPage : Page
    {
        private readonly FatigueShiftAuditViewModel viewModel;

        public FatigueAuditPage()
        {
            InitializeComponent();

            this.viewModel = App.Services.GetRequiredService<FatigueShiftAuditViewModel>();
            this.DataContext = this.viewModel;

            WeekStartPicker.Date = new DateTimeOffset(DateTime.Today);
        }

        private void WeekStartPicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs eventArgs)
        {
            if (sender.Date.HasValue)
            {
                this.viewModel.SelectedWeekStart = sender.Date.Value;
            }
        }

        private void RunAutoAudit_Click(object sender, RoutedEventArgs eventArgs)
        {
            try
            {
                this.viewModel.RunAutoAudit();
            }
            catch (Exception exception)
            {
                this.viewModel.StatusMessage = $"Error during audit: {exception.Message}";
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs eventArgs)
        {
            this.viewModel.Violations.Clear();
            this.viewModel.Suggestions.Clear();
        }

        private async void ApplyReassignment_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (sender is Button button && button.Tag is int shiftId)
            {
                var result = this.viewModel.ApplyReassignment(shiftId);

                var dialog = new ContentDialog
                {
                    Title = result.Title,
                    Content = result.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot,
                };
                await dialog.ShowAsync();
            }
        }

        private void PublishRoster_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (this.viewModel.CanPublish)
            {
                var dialog = new ContentDialog
                {
                    Title = "Roster Published",
                    Content = $"The roster for the {this.viewModel.WeekLabel} has been published successfully.",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot,
                };
                _ = dialog.ShowAsync();
            }
        }
    }
}
