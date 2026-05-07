namespace UBB_SE_2026_923_2.Views
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public sealed partial class RoleSelectionPage : Page
    {
        private readonly ICurrentUserService currentUser;

        public RoleSelectionPage()
        {
            InitializeComponent();

            this.currentUser = App.Services.GetRequiredService<ICurrentUserService>();
        }

        private void Admin_Click(object sender, RoutedEventArgs e)
        {
            this.currentUser.RoleType = UserRole.Admin;
            this.Frame.Navigate(typeof(RoleDashboardPage));
        }

        private void Doctor_Click(object sender, RoutedEventArgs e)
        {
            this.currentUser.RoleType = UserRole.Doctor;
            this.Frame.Navigate(typeof(RoleDashboardPage));
        }

        private void Pharmacist_Click(object sender, RoutedEventArgs e)
        {
            this.currentUser.RoleType = UserRole.Pharmacist;
            this.Frame.Navigate(typeof(RoleDashboardPage));
        }
    }
}
