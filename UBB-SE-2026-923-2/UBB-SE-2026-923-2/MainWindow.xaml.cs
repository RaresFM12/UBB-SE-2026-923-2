using Microsoft.UI.Xaml;
using UBB_SE_2026_923_2.Views.Accounts;

namespace UBB_SE_2026_923_2
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RootFrame.Navigate(typeof(LoginView));
        }
    }
}
