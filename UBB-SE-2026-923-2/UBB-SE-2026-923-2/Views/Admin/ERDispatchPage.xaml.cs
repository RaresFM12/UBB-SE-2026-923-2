namespace UBB_SE_2026_923_2.Views.Admin
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using UBB_SE_2026_923_2.ViewModels.Admin;

    public sealed partial class ERDispatchPage : Page
    {
        private const int SimulatedIncomingRequestCount = 3;

        public ERDispatchViewModel ViewModel { get; }

        public ERDispatchPage()
        {
            InitializeComponent();

            this.ViewModel = App.Services.GetRequiredService<ERDispatchViewModel>();
            this.DataContext = this.ViewModel;
        }

        private async void RunDispatch_Click(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.RunDispatchAsync();

            if (this.ViewModel.UnmatchedRequests.Count > 0)
            {
                UnmatchedRequestCombo.SelectedIndex = 0;
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Refresh();
            UnmatchedRequestCombo.SelectedIndex = -1;
            OverrideDoctorCombo.SelectedIndex = -1;
        }

        private async void SimulateIncoming_Click(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.SimulateIncomingAsync(SimulatedIncomingRequestCount);
        }

        private async void UnmatchedRequestCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UnmatchedRequestCombo.SelectedItem is ERDispatchViewModel.UnmatchedRequestRow row)
            {
                await this.ViewModel.LoadOverrideCandidatesAsync(row.RequestId);
            }
        }

        private async void ApplyOverride_Click(object sender, RoutedEventArgs e)
        {
            if (UnmatchedRequestCombo.SelectedItem is not ERDispatchViewModel.UnmatchedRequestRow selectedRequest)
            {
                return;
            }

            if (OverrideDoctorCombo.SelectedItem is not ERDispatchViewModel.OverrideCandidateRow candidate)
            {
                return;
            }

            var success = await this.ViewModel.ApplyOverrideAsync(selectedRequest.RequestId, candidate.DoctorId);
            if (!success)
            {
                return;
            }

            OverrideDoctorCombo.SelectedIndex = -1;
            if (this.ViewModel.UnmatchedRequests.Count > 0)
            {
                UnmatchedRequestCombo.SelectedIndex = 0;
            }
            else
            {
                UnmatchedRequestCombo.SelectedIndex = -1;
            }
        }
    }
}
