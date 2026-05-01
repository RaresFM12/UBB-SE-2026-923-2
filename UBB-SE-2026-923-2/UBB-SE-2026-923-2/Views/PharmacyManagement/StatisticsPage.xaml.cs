using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UBB_SE_2026_923_2.Services;


namespace UBB_SE_2026_923_2.Views.PharmacyManagement;

public sealed partial class StatisticsPage : Page
{
    public List<Tuple<int, string, int>> TopItems { get; set; }
    public Dictionary<string, int> TopSubstances { get; set; }
    public string ItemsWarning { get; set; } = string.Empty;
    public string SubstancesWarning { get; set; } = string.Empty;
    private IAdminService adminService;
    public StatisticsPage()
    {
        InitializeComponent();
        adminService = new AdminService();
        TopItems = adminService.GetTop30Items();
        TopSubstances = adminService.GetTop30Substances();

        if (TopItems.Count < 30)
        {
            ItemsWarning = $"Only {TopItems.Count} products were bought last month (fewer than 30).";
        }

        if (TopSubstances.Count < 30)
        {
            SubstancesWarning = $"Only {TopSubstances.Count} active substances found last month (fewer than 30).";
        }

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
