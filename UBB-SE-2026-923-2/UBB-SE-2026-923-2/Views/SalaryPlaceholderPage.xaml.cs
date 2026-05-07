namespace UBB_SE_2026_923_2.Views
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml.Controls;
    using UBB_SE_2026_923_2.ViewModels;

    public sealed partial class SalaryPlaceholderPage : Page
    {
        public SalaryComputationViewModel ViewModel { get; }

        public SalaryPlaceholderPage()
        {
            this.InitializeComponent();

            this.ViewModel = App.Services.GetRequiredService<SalaryComputationViewModel>();
            this.DataContext = this.ViewModel;
        }
    }
}
