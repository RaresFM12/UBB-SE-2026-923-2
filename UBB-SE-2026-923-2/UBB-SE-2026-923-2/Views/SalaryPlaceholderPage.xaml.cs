using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using UBB_SE_2026_923_2.ViewModels;

namespace UBB_SE_2026_923_2.Views
{
    public sealed partial class SalaryPlaceholderPage : Page
    {
        public SalaryComputationViewModel ViewModel { get; }

        public SalaryPlaceholderPage()
        {
            this.InitializeComponent();

            ViewModel = App.Services.GetRequiredService<SalaryComputationViewModel>();
            this.DataContext = ViewModel;
        }
    }
}
