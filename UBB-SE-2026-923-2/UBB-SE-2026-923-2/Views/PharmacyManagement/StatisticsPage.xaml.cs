namespace UBB_SE_2026_923_2.Views.PharmacyManagement;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using UBB_SE_2026_923_2.Services;

public sealed partial class StatisticsPage : Page
{
    public List<Tuple<int, string, int>> TopItems { get; set; }

    public List<KeyValuePair<string, int>> TopSubstances { get; set; }

    private readonly IAdminService adminService;

    public StatisticsPage()
    {
        this.adminService = new AdminService();
        this.TopItems = this.adminService.GetTop30Items();
        this.TopSubstances =[.. this.adminService.GetTop30Substances()];

        InitializeComponent();

        ItemsGrid.Visibility = Visibility.Visible;
        SubstancesGrid.Visibility = Visibility.Collapsed;
    }

    private void GoToEditPageClick(object sender, RoutedEventArgs e)
    {
        this.Frame.Navigate(typeof(EditPage));
    }

    private void OnItemsClick(object sender, RoutedEventArgs e)
    {
        ItemsGrid.Visibility = Visibility.Visible;
        SubstancesGrid.Visibility = Visibility.Collapsed;
    }

    private void OnSubstancesClick(object sender, RoutedEventArgs e)
    {
        ItemsGrid.Visibility = Visibility.Collapsed;
        SubstancesGrid.Visibility = Visibility.Visible;
    }
}
