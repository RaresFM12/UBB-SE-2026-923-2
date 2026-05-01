using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.ViewModels.PharmacyManagement
{
    public class EditPageViewModel : INotifyPropertyChanged
    {
        private readonly IAdminService adminService;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Item> Items { get; } = new ObservableCollection<Item>();
        public ObservableCollection<Substance> Substances { get; } = new ObservableCollection<Substance>();

        private Visibility itemListButtonsVisibility = Visibility.Visible;
        public Visibility ItemListButtonsVisibility
        {
            get => itemListButtonsVisibility;
            private set
            {
                itemListButtonsVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility itemBottomButtonsVisibility = Visibility.Visible;
        public Visibility ItemBottomButtonsVisibility
        {
            get => itemBottomButtonsVisibility;
            private set
            {
                itemBottomButtonsVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility showExpiredItemsToggleVisibility = Visibility.Visible;
        public Visibility ShowExpiredItemsToggleVisibility
        {
            get => showExpiredItemsToggleVisibility;
            private set
            {
                showExpiredItemsToggleVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility substanceListButtonsVisibility = Visibility.Collapsed;
        public Visibility SubstanceListButtonsVisibility
        {
            get => substanceListButtonsVisibility;
            private set
            {
                substanceListButtonsVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility substanceBottomButtonsVisibility = Visibility.Collapsed;
        public Visibility SubstanceBottomButtonsVisibility
        {
            get => substanceBottomButtonsVisibility;
            private set
            {
                substanceBottomButtonsVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility addSubstanceGridVisibility = Visibility.Collapsed;
        public Visibility AddSubstanceGridVisibility
        {
            get => addSubstanceGridVisibility;
            set
            {
                addSubstanceGridVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility updateSubstanceGridVisibility = Visibility.Collapsed;
        public Visibility UpdateSubstanceGridVisibility
        {
            get => updateSubstanceGridVisibility;
            set
            {
                updateSubstanceGridVisibility = value;
                OnPropertyChanged();
            }
        }

        public EditPageViewModel()
            : this(new AdminService())
        {
        }

        public EditPageViewModel(IAdminService adminService)
        {
            this.adminService = adminService;
            RefreshItems();
            RefreshSubstances();
        }

        public void RefreshItems()
        {
            Items.Clear();
            foreach (Item item in adminService.GetAllItems())
            {
                Items.Add(item);
            }
        }

        public void RefreshSubstances()
        {
            Substances.Clear();
            foreach (Substance substance in adminService.GetAllSubstances())
            {
                Substances.Add(substance);
            }
        }

        public void SearchItems(string query)
        {
            Items.Clear();
            foreach (Item item in adminService.SearchItemsByName(query))
            {
                Items.Add(item);
            }
        }

        public void ShowExpiredItems()
        {
            Items.Clear();
            foreach (Item item in adminService.GetExpiredItems())
            {
                Items.Add(item);
            }
        }

        public Item GetItemById(int id) => adminService.GetItemById(id);
        public Substance GetSubstanceByName(string name) => adminService.GetSubstanceByName(name);
        public bool SubstanceExists(string name) => adminService.SubstanceExists(name);

        public void AddItemWithQuantity(Item item) => adminService.AddItemWithQuantity(item);
        public void UpdateItemById(int id, Item item) => adminService.UpdateItemById(id, item);
        public void RemoveItemById(int id) => adminService.RemoveItemById(id);

        public void AddSubstance(Substance substance) => adminService.AddSubstance(substance);
        public void UpdateSubstanceByName(string name, Substance substance) => adminService.UpdateSubstanceByName(name, substance);
        public void RemoveSubstanceByName(Substance substance) => adminService.RemoveSubstanceByName(substance);

        public void ActivateItemsSection()
        {
            ItemListButtonsVisibility = Visibility.Visible;
            ItemBottomButtonsVisibility = Visibility.Visible;
            ShowExpiredItemsToggleVisibility = Visibility.Visible;

            SubstanceListButtonsVisibility = Visibility.Collapsed;
            SubstanceBottomButtonsVisibility = Visibility.Collapsed;
            AddSubstanceGridVisibility = Visibility.Collapsed;
            UpdateSubstanceGridVisibility = Visibility.Collapsed;
        }

        public void ActivateSubstancesSection()
        {
            ItemListButtonsVisibility = Visibility.Collapsed;
            ItemBottomButtonsVisibility = Visibility.Collapsed;
            ShowExpiredItemsToggleVisibility = Visibility.Collapsed;

            SubstanceListButtonsVisibility = Visibility.Visible;
            SubstanceBottomButtonsVisibility = Visibility.Visible;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
