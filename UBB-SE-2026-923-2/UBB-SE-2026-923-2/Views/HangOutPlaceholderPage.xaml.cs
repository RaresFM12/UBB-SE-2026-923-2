namespace UBB_SE_2026_923_2.Views
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using UBB_SE_2026_923_2.ViewModels.Doctor;

    public sealed partial class HangOutPlaceholderPage : Page
    {
        public HangoutViewModel ViewModel { get; }

        public HangOutPlaceholderPage()
        {
            this.InitializeComponent();

            this.ViewModel = App.Services.GetRequiredService<HangoutViewModel>();
            this.DataContext = this.ViewModel;
        }

        private void Join_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int hangoutId)
            {
                this.ViewModel.JoinHangoutById(hangoutId);
            }
        }
    }
}
