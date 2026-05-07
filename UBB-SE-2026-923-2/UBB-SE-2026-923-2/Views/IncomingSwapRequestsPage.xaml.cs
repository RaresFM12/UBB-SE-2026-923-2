namespace UBB_SE_2026_923_2.Views
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml.Controls;
    using UBB_SE_2026_923_2.ViewModels.Doctor;

    public sealed partial class IncomingSwapRequestsPage : Page
    {
        public IncomingSwapRequestsViewModel ViewModel { get; }

        public IncomingSwapRequestsPage()
        {
            this.InitializeComponent();

            this.ViewModel = App.Services.GetRequiredService<IncomingSwapRequestsViewModel>();
            this.DataContext = this.ViewModel;
        }
    }
}
