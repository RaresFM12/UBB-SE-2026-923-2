using UBB_SE_2026_923_2.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.Views
{
    public sealed partial class RoleSelectionPage : Page
    {
        private readonly ICurrentUserService currentUser;

        public RoleSelectionPage()
        {
            InitializeComponent();

            currentUser = App.Services.GetRequiredService<ICurrentUserService>();
        }

        private void Admin_Click(object sender, RoutedEventArgs e)
        {
            currentUser.RoleType = UserRole.Admin;
            Frame.Navigate(typeof(RoleDashboardPage));
        }

        private void Doctor_Click(object sender, RoutedEventArgs e)
        {
            currentUser.RoleType = UserRole.Doctor;
            Frame.Navigate(typeof(RoleDashboardPage));
        }

        private void Pharmacist_Click(object sender, RoutedEventArgs e)
        {
            currentUser.RoleType = UserRole.Pharmacist;
            Frame.Navigate(typeof(RoleDashboardPage));
        }
    }
}
