namespace UBB_SE_2026_923_2
{
    using Microsoft.UI.Xaml;
    using UBB_SE_2026_923_2.Views.Accounts;

    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RootFrame.Navigate(typeof(LoginView));
        }
    }
}
