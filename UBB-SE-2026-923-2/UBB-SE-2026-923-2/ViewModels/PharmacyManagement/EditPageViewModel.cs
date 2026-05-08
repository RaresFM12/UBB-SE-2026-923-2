namespace UBB_SE_2026_923_2.ViewModels.PharmacyManagement
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Microsoft.UI.Xaml;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public class EditPageViewModel : INotifyPropertyChanged
    {
        private readonly IAdminService adminService;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Item> Items { get; } = new ObservableCollection<Item>();

        public ObservableCollection<Substance> Substances { get; } = new ObservableCollection<Substance>();

        private Visibility itemListButtonsVisibility = Visibility.Visible;

        public Visibility ItemListButtonsVisibility
        {
            get => this.itemListButtonsVisibility;
            private set
            {
                this.itemListButtonsVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility itemBottomButtonsVisibility = Visibility.Visible;

        public Visibility ItemBottomButtonsVisibility
        {
            get => this.itemBottomButtonsVisibility;
            private set
            {
                this.itemBottomButtonsVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility showExpiredItemsToggleVisibility = Visibility.Visible;

        public Visibility ShowExpiredItemsToggleVisibility
        {
            get => this.showExpiredItemsToggleVisibility;
            private set
            {
                this.showExpiredItemsToggleVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility substanceListButtonsVisibility = Visibility.Collapsed;

        public Visibility SubstanceListButtonsVisibility
        {
            get => this.substanceListButtonsVisibility;
            private set
            {
                this.substanceListButtonsVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility substanceBottomButtonsVisibility = Visibility.Collapsed;

        public Visibility SubstanceBottomButtonsVisibility
        {
            get => this.substanceBottomButtonsVisibility;
            private set
            {
                this.substanceBottomButtonsVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility addSubstanceGridVisibility = Visibility.Collapsed;

        public Visibility AddSubstanceGridVisibility
        {
            get => this.addSubstanceGridVisibility;
            set
            {
                this.addSubstanceGridVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility updateSubstanceGridVisibility = Visibility.Collapsed;

        public Visibility UpdateSubstanceGridVisibility
        {
            get => this.updateSubstanceGridVisibility;
            set
            {
                this.updateSubstanceGridVisibility = value;
                this.OnPropertyChanged();
            }
        }

        public EditPageViewModel()
            : this(new AdminService())
        {
        }

        public EditPageViewModel(IAdminService adminService)
        {
            this.adminService = adminService;
            this.RefreshItems();
            this.RefreshSubstances();
        }

        public void RefreshItems()
        {
            this.Items.Clear();
            foreach (Item item in this.adminService.GetAllItems())
            {
                this.Items.Add(item);
            }
        }

        public void RefreshSubstances()
        {
            this.Substances.Clear();
            foreach (Substance substance in this.adminService.GetAllSubstances())
            {
                this.Substances.Add(substance);
            }
        }

        public void SearchItems(string query)
        {
            this.Items.Clear();
            foreach (Item item in this.adminService.SearchItemsByName(query))
            {
                this.Items.Add(item);
            }
        }

        public void ShowExpiredItems()
        {
            this.Items.Clear();
            foreach (Item item in this.adminService.GetExpiredItems())
            {
                this.Items.Add(item);
            }
        }

        public Item GetItemById(int itemId) => this.adminService.GetItemById(itemId);

        public Substance GetSubstanceByName(string name) => this.adminService.GetSubstanceByName(name);

        public bool SubstanceExists(string name) => this.adminService.SubstanceExists(name);

        public void AddItemWithQuantity(Item item) => this.adminService.AddItemWithQuantity(item);

        public void UpdateItemById(int itemId, Item item) => this.adminService.UpdateItemById(itemId, item);

        public void RemoveItemById(int itemId) => this.adminService.RemoveItemById(itemId);

        public void AddSubstance(Substance substance) => this.adminService.AddSubstance(substance);

        public void UpdateSubstanceByName(string name, Substance substance) => this.adminService.UpdateSubstanceByName(name, substance);

        public void RemoveSubstanceByName(Substance substance) => this.adminService.RemoveSubstanceByName(substance);

        public void ActivateItemsSection()
        {
            this.ItemListButtonsVisibility = Visibility.Visible;
            this.ItemBottomButtonsVisibility = Visibility.Visible;
            this.ShowExpiredItemsToggleVisibility = Visibility.Visible;

            this.SubstanceListButtonsVisibility = Visibility.Collapsed;
            this.SubstanceBottomButtonsVisibility = Visibility.Collapsed;
            this.AddSubstanceGridVisibility = Visibility.Collapsed;
            this.UpdateSubstanceGridVisibility = Visibility.Collapsed;
        }

        public void ActivateSubstancesSection()
        {
            this.ItemListButtonsVisibility = Visibility.Collapsed;
            this.ItemBottomButtonsVisibility = Visibility.Collapsed;
            this.ShowExpiredItemsToggleVisibility = Visibility.Collapsed;

            this.SubstanceListButtonsVisibility = Visibility.Visible;
            this.SubstanceBottomButtonsVisibility = Visibility.Visible;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
