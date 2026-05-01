using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using UBB_SE_2026_923_2.ViewModels.Doctor;

namespace UBB_SE_2026_923_2.Views
{
    public sealed partial class MySchedulePage : Page
    {
        public MyScheduleViewModel ViewModel { get; }

        public MySchedulePage()
        {
            InitializeComponent();

            ViewModel = App.Services.GetRequiredService<MyScheduleViewModel>();
            DataContext = ViewModel;
        }
    }
}
