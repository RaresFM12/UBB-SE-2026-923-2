using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using UBB_SE_2026_923_2.ViewModels.Doctor;

namespace UBB_SE_2026_923_2.Views
{
    public sealed partial class IncomingSwapRequestsPage : Page
    {
        public IncomingSwapRequestsViewModel ViewModel { get; }

        public IncomingSwapRequestsPage()
        {
            this.InitializeComponent();

            ViewModel = App.Services.GetRequiredService<IncomingSwapRequestsViewModel>();
            DataContext = ViewModel;
        }
    }
}
