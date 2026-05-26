namespace UBB_SE_2026_923_2.Views
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.Versioning;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media.Animation;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.PeriodTracker;
    using UBB_SE_2026_923_2.Views.Accounts;
    using UBB_SE_2026_923_2.Views.Admin;
    using UBB_SE_2026_923_2.Views.Doctor;
    using UBB_SE_2026_923_2.Views.Orders;
    using UBB_SE_2026_923_2.Views.PeriodTracker;
    using UBB_SE_2026_923_2.Views.Pharmacy;
    using UBB_SE_2026_923_2.Views.PharmacyManagement;
    using UBB_SE_2026_923_2.Views.ProductsCatalogue;

    [SupportedOSPlatform("windows10.0.17763.0")]
    public sealed partial class RoleDashboardPage : Page
    {
        private readonly ICurrentUserService currentUser;
        private readonly ObservableCollection<MenuEntry> items = new ObservableCollection<MenuEntry>();
        private readonly Dictionary<string, Type> routes = new Dictionary<string, Type>();
        private readonly Dictionary<string, object?> routeParameters = new Dictionary<string, object?>();
        private readonly IOrderService? orderService;
        private readonly PeriodTrackerViewModel? periodTrackerViewModel;

        public RoleDashboardPage()
        {
            InitializeComponent();

            this.currentUser = App.Services.GetRequiredService<ICurrentUserService>();
            this.orderService = new OrderService();

            // Repositories now come from DI as HTTP-backed implementations; the
            // desktop no longer talks to EF Core directly.
            var usersRepository = App.Services.GetRequiredService<IUsersRepository>();
            var itemsRepository = App.Services.GetRequiredService<IItemsRepository>();
            var ptFactory = new PeriodTrackerServiceFactory(
                usersRepository,
                itemsRepository,
                App.Services.GetRequiredService<RaresICurrentUserService>(),
                this.orderService);
            this.periodTrackerViewModel = new PeriodTrackerViewModel(
                ptFactory.CreatePeriodTrackerService(),
                ptFactory.CreateWellnessItemsService(),
                ptFactory.CreateBasketService());
            var productCatalogueService = new ProductCatalogueService(itemsRepository);
            var user = ServiceWrapper.UserAccountService.CurrentUser;
            var catalogParameter = ((IProductCatalogueService)productCatalogueService, user, this.orderService);

            MenuList.ItemsSource = this.items;
            this.BuildForRole(catalogParameter);
        }

        private void BuildForRole((IProductCatalogueService, User, IOrderService) catalogParameter)
        {
            this.items.Clear();
            this.routes.Clear();
            this.routeParameters.Clear();

            RoleText.Text = $"Role: {this.currentUser.RoleType}";

            switch (this.currentUser.RoleType)
            {
                case UserRole.Admin:
                    this.Add("See Doctor Schedule", "admin-doctor-schedule", typeof(DoctorSchedulePage));
                    this.Add("See Pharmacy Schedule", "admin-pharmacy-schedule", typeof(PharmacySchedulePage));
                    this.Add("Appointments", "admin-appointments", typeof(AppointmentsPage));
                    this.Add("Shift Management", "admin-shift-management", typeof(AdminSchedulePage));
                    this.Add("Auto-Audit", "admin-auto-audit", typeof(FatigueAuditPage));
                    this.Add("ER Dispatch", "admin-er-dispatch", typeof(ERDispatchPage));
                    this.Add("Accounts Management", "admin-accounts", typeof(AdminAccountsManagementView));
                    this.Add("Pharmacy Inventory Update", "admin-pharmacy-inventory", typeof(EditPage));
                    break;

                case UserRole.Pharmacist:
                    this.Add("See Schedule", "pharmacist-schedule", typeof(PharmacySchedulePage));
                    this.Add("Vacation Window", "pharmacist-vacation", typeof(PharmacistVacationPage));
                    this.Add("Salary", "pharmacist-salary", typeof(UBB_SE_2026_923_2.Views.SalaryPlaceholderPage));
                    break;

                case UserRole.Doctor:
                    this.Add("Shift Swap Request", "doctor-shift-swap-request", typeof(MySchedulePage));
                    this.Add("Incoming Swap Requests", "doctor-shift-swap-incoming", typeof(IncomingSwapRequestsPage));
                    this.Add("See Schedule", "doctor-schedule", typeof(DoctorSchedulePage));
                    this.Add("Salary", "doctor-salary", typeof(UBB_SE_2026_923_2.Views.SalaryPlaceholderPage));
                    this.Add("Hang Out", "doctor-hangout", typeof(HangOutPlaceholderPage));
                    break;

                case UserRole.Client:
                    this.Add("Product Catalogue", "client-catalogue", typeof(CatalogPage), catalogParameter);
                    this.Add("Shopping Cart", "client-cart", typeof(BasketPage), this.orderService);
                    this.Add("Order History", "client-order-history", typeof(OrderHistoryPage), this.orderService);
                    this.Add("Period Tracker", "client-period-tracker", typeof(PeriodTrackerPage), this.periodTrackerViewModel);
                    this.Add("Notifications", "client-notifications", typeof(Notifications), catalogParameter);
                    break;
            }

            this.Add("Profile", "profile-management", typeof(ProfileManagementView));

            var first = this.items.FirstOrDefault();
            if (first != null)
            {
                MenuList.SelectedItem = first;
                this.NavigateToKey(first.Key);
            }
        }

        private void Add(string title, string key, Type pageType, object? parameter = null)
        {
            if (!typeof(Page).IsAssignableFrom(pageType))
            {
                throw new InvalidOperationException($"{pageType.FullName} is not a Page.");
            }

            this.items.Add(new MenuEntry { Key = key, Title = title });
            this.routes[key] = pageType;
            this.routeParameters[key] = parameter;
        }

        private void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuList.SelectedItem is not MenuEntry entry)
            {
                return;
            }

            this.NavigateToKey(entry.Key);
        }

        private void NavigateToKey(string key)
        {
            if (!this.routes.TryGetValue(key, out var pageType))
            {
                pageType = typeof(NotImplementedPlaceholderPage);
            }

            this.routeParameters.TryGetValue(key, out var parameter);
            ContentFrame.Navigate(pageType, parameter, new SuppressNavigationTransitionInfo());
        }

        private void ChangeRole_Click(object sender, RoutedEventArgs e)
        {
            ServiceWrapper.UserAccountService.Logout();
            this.Frame.Navigate(typeof(LoginView));
        }

        private sealed class MenuEntry
        {
            public string Key { get; set; } = string.Empty;

            public string Title { get; set; } = string.Empty;
        }
    }
}
