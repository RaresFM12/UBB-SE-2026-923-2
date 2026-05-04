using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Versioning;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Views.Admin;
using UBB_SE_2026_923_2.Views.Doctor;
using UBB_SE_2026_923_2.Views.Pharmacy;
using UBB_SE_2026_923_2.Views.Accounts;
using UBB_SE_2026_923_2.Views.Orders;
using UBB_SE_2026_923_2.Views.ProductsCatalogue;
using UBB_SE_2026_923_2.Views.PharmacyManagement;
using UBB_SE_2026_923_2.Views.PeriodTracker;
using UBB_SE_2026_923_2.ViewModels.PeriodTracker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.Views
{
    [SupportedOSPlatform("windows10.0.17763.0")]
    public sealed partial class RoleDashboardPage : Page
    {
        private readonly ICurrentUserService currentUser;
        private readonly ObservableCollection<MenuEntry> items = new ObservableCollection<MenuEntry>();
        private readonly Dictionary<string, Type> routes = new Dictionary<string, Type>();
        private readonly Dictionary<string, object?> routeParameters = new Dictionary<string, object?>();
        private IOrderService? orderService;
        private PeriodTrackerViewModel? periodTrackerViewModel;

        public RoleDashboardPage()
        {
            InitializeComponent();

            currentUser = App.Services.GetRequiredService<ICurrentUserService>();
            orderService = new OrderService();
            var dbContextFactory = App.Services.GetRequiredService<Microsoft.EntityFrameworkCore.IDbContextFactory<UBB_SE_2026_923_2.Data.AppDbContext>>();
            var ptFactory = new PeriodTrackerServiceFactory(
                new SQLUsersRepository(dbContextFactory),
                new SQLItemsRepository(dbContextFactory),
                App.Services.GetRequiredService<RaresICurrentUserService>(),
                orderService);
            periodTrackerViewModel = new PeriodTrackerViewModel(
                ptFactory.CreatePeriodTrackerService(),
                ptFactory.CreateWellnessItemsService(),
                ptFactory.CreateBasketService());
            var productCatalogueService = new ProductCatalogueService(new SQLItemsRepository(dbContextFactory));
            var user = ServiceWrapper.UserAccountService.CurrentUser;
            var catalogParameter = ((IProductCatalogueService)productCatalogueService, user, orderService);

            MenuList.ItemsSource = items;
            BuildForRole(catalogParameter);
        }

        private void BuildForRole((IProductCatalogueService, User, IOrderService) catalogParameter)
        {
            items.Clear();
            routes.Clear();
            routeParameters.Clear();

            RoleText.Text = $"Role: {currentUser.RoleType}";

            switch (currentUser.RoleType)
            {
                case UserRole.Admin:
                    Add("See Doctor Schedule", "admin-doctor-schedule", typeof(DoctorSchedulePage));
                    Add("See Pharmacy Schedule", "admin-pharmacy-schedule", typeof(PharmacySchedulePage));
                    Add("Appointments", "admin-appointments", typeof(AppointmentsPage));
                    Add("Create Shift", "admin-create-shift", typeof(AdminShiftView));
                    Add("Auto-Audit", "admin-auto-audit", typeof(FatigueAuditPage));
                    Add("ER Dispatch", "admin-er-dispatch", typeof(ERDispatchPage));
                    Add("Accounts Management", "admin-accounts", typeof(AdminAccountsManagementView));
                    break;

                case UserRole.Pharmacist:
                    Add("See Schedule", "pharmacist-schedule", typeof(PharmacySchedulePage));
                    Add("Vacation Window", "pharmacist-vacation", typeof(PharmacistVacationPage));
                    Add("Salary", "pharmacist-salary", typeof(UBB_SE_2026_923_2.Views.SalaryPlaceholderPage));
                    Add("Product Catalogue", "pharmacist-catalogue", typeof(CatalogPage), catalogParameter);
                    Add("Order Management", "pharmacist-orders", typeof(OrderManagementPage), orderService);
                    Add("Edit Inventory", "pharmacist-edit", typeof(EditPage));
                    Add("Statistics", "pharmacist-statistics", typeof(StatisticsPage));
                    Add("Notifications", "pharmacist-notifications", typeof(Notifications));
                    break;

                case UserRole.Doctor:
                    Add("Medical Evaluation", "doctor-medical", typeof(UBB_SE_2026_923_2.Views.MedicalEvaluationView));
                    Add("Shift Swap Request", "doctor-shift-swap-request", typeof(MySchedulePage));
                    Add("Incoming Swap Requests", "doctor-shift-swap-incoming", typeof(IncomingSwapRequestsPage));
                    Add("See Schedule", "doctor-schedule", typeof(DoctorSchedulePage));
                    Add("Salary", "doctor-salary", typeof(UBB_SE_2026_923_2.Views.SalaryPlaceholderPage));
                    Add("Hang Out", "doctor-hangout", typeof(HangOutPlaceholderPage));
                    Add("Product Catalogue", "doctor-catalogue", typeof(CatalogPage), catalogParameter);
                    Add("Order History", "doctor-orders", typeof(OrderHistoryPage), orderService);
                    Add("Period Tracker", "doctor-period-tracker", typeof(PeriodTrackerPage), periodTrackerViewModel);
                    break;

                case UserRole.Client:
                    Add("Product Catalogue", "client-catalogue", typeof(CatalogPage), catalogParameter);
                    Add("Shopping Cart", "client-cart", typeof(BasketPage), orderService);
                    Add("Order History", "client-orders", typeof(OrderHistoryPage), orderService);
                    Add("Period Tracker", "client-period-tracker", typeof(PeriodTrackerPage), periodTrackerViewModel);
                    Add("Notifications", "client-notifications", typeof(Notifications));
                    break;
            }

            var first = items.FirstOrDefault();
            if (first != null)
            {
                MenuList.SelectedItem = first;
                NavigateToKey(first.Key);
            }
        }

        private void Add(string title, string key, Type pageType, object? parameter = null)
        {
            if (!typeof(Page).IsAssignableFrom(pageType))
            {
                throw new InvalidOperationException($"{pageType.FullName} is not a Page.");
            }

            items.Add(new MenuEntry { Key = key, Title = title });
            routes[key] = pageType;
            routeParameters[key] = parameter;
        }

        private void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuList.SelectedItem is not MenuEntry entry)
            {
                return;
            }

            NavigateToKey(entry.Key);
        }

        private void NavigateToKey(string key)
        {
            if (!routes.TryGetValue(key, out var pageType))
            {
                pageType = typeof(NotImplementedPlaceholderPage);
            }

            routeParameters.TryGetValue(key, out var parameter);
            ContentFrame.Navigate(pageType, parameter, new SuppressNavigationTransitionInfo());
        }

        private void ChangeRole_Click(object sender, RoutedEventArgs e)
        {
            ServiceWrapper.UserAccountService.Logout();
            Frame.Navigate(typeof(LoginView));
        }

        private sealed class MenuEntry
        {
            public string Key { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
        }
    }
}
