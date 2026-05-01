using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using UBB_SE_2026_923_2.ViewModels.Doctor;

namespace UBB_SE_2026_923_2.Views
{
    public sealed partial class HangOutPlaceholderPage : Page
    {
        public HangoutViewModel ViewModel { get; }

        public HangOutPlaceholderPage()
        {
            this.InitializeComponent();

            ViewModel = App.Services.GetRequiredService<HangoutViewModel>();
            this.DataContext = ViewModel;
        }

        private void Join_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int hangoutId)
            {
                ViewModel.JoinHangoutById(hangoutId);
            }
        }
    }
}
