namespace UBB_SE_2026_923_2.Views
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml.Controls;
    using UBB_SE_2026_923_2.ViewModels.Doctor;

    public sealed partial class MySchedulePage : Page
    {
        public MyScheduleViewModel ViewModel { get; }

        public MySchedulePage()
        {
            InitializeComponent();

            this.ViewModel = App.Services.GetRequiredService<MyScheduleViewModel>();
            this.DataContext = this.ViewModel;
        }
    }
}
