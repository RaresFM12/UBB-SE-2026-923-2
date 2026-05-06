using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.ViewModels.PharmacyManagement;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace UBB_SE_2026_923_2.Views.PharmacyManagement
{
    public sealed partial class EditPage : Page
    {
        public EditPageViewModel ViewModel { get; }
        private bool isGetItemDataClicked = false;

        public class ActiveSubstance
        {
            public string Name { get; set; }
            public float Concentration { get; set; }
        }
        public Dictionary<string, float> ActiveSubstancesDict { get; set; } = new Dictionary<string, float>();

        public class BatchItem
        {
            public DateOnly Date { get; set; }
            public int Packs { get; set; }
        }
        public Dictionary<DateOnly, int> BatchesDict { get; set; } = new Dictionary<DateOnly, int>();
        public EditPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;
            ViewModel = new EditPageViewModel();
            DataContext = ViewModel;
            ApplyUiStateFromViewModel();
            ResetUiValidationState();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ResetUiValidationState();
        }

        private void ApplyUiStateFromViewModel()
        {
            ItemListButtons.Visibility = ViewModel.ItemListButtonsVisibility;
            ItemBottomButtons.Visibility = ViewModel.ItemBottomButtonsVisibility;
            ShowExpiredItemsToggle.Visibility = ViewModel.ShowExpiredItemsToggleVisibility;

            SubstanceListButtons.Visibility = ViewModel.SubstanceListButtonsVisibility;
            SubstanceBottomButtons.Visibility = ViewModel.SubstanceBottomButtonsVisibility;
            AddSubstanceGrid.Visibility = ViewModel.AddSubstanceGridVisibility;
            UpdateSubstanceGrid.Visibility = ViewModel.UpdateSubstanceGridVisibility;
        }
        private void ResetUiValidationState()
        {
            isGetItemDataClicked = false;

            ViewModel.ActivateItemsSection();
            ViewModel.AddSubstanceGridVisibility = Visibility.Collapsed;
            ViewModel.UpdateSubstanceGridVisibility = Visibility.Collapsed;
            ApplyUiStateFromViewModel();

            AddItemGrid.Visibility = Visibility.Collapsed;
            UpdateItemGrid.Visibility = Visibility.Collapsed;

            SearchBox.Text = string.Empty;
            ShowExpiredToggle.IsOn = false;

            ClearItemAddBoxes();
            ClearItemUpdateBoxes();
            ClearSubstanceBoxes();
            ClearSubstanceUpdateBoxes();

            ActiveSubstancesDict.Clear();
            BatchesDict.Clear();
            RefreshActiveSubstancesList();
            RefreshActiveSubstancesListUpdate();
            RefreshBatchesList();
            RefreshBatchesListUpdate();

            ItemList.SelectedItem = null;
            SubstanceList.SelectedItem = null;

            RemoveItemError.Visibility = Visibility.Collapsed;
            RemoveSubstanceError.Visibility = Visibility.Collapsed;

            ResetAddItemErrors();
            ResetUpdateItemErrors();
            ResetSubstanceErrors();
            ResetActiveSubstanceErrors();
            ResetAddBatchErrors();
        }

        private void GoToStatisticsClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(StatisticsPage));
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.SearchItems(SearchBox.Text);
            ItemList.ItemsSource = ViewModel.Items;
        }

        private void OnItemClick(object sender, RoutedEventArgs e)
        {
            ViewModel.ActivateItemsSection();
            ApplyUiStateFromViewModel();
            ResetSubstanceErrors();
        }

        private void OnSubstancesClick(object sender, RoutedEventArgs e)
        {
            ViewModel.ActivateSubstancesSection();
            ApplyUiStateFromViewModel();

            AddItemGrid.Visibility = Visibility.Collapsed;
            UpdateItemGrid.Visibility = Visibility.Collapsed;
            RemoveItemError.Visibility = Visibility.Collapsed;
        }

        private void OnOrdersClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Views.Orders.OrderManagementPage), new OrderService());
        }

        private void ShowExpiredToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (ShowExpiredToggle.IsOn)
            {
                ViewModel.ShowExpiredItems();
                ItemList.ItemsSource = ViewModel.Items;
            }
            else
            {
                ViewModel.RefreshItems();
                ItemList.ItemsSource = ViewModel.Items;
            }
        }

        private void ClearItemAddBoxes()
        {
            NameBox.Text = string.Empty;
            ProducerBox.Text = string.Empty;
            PriceBox.Text = string.Empty;
            CategoryBox.Text = string.Empty;
            ImagePathBox.Text = string.Empty;
            NumberOfPillsBox.Text = string.Empty;
            LabelBox.Text = string.Empty;
            DescriptionBox.Text = string.Empty;
            DiscountBox.Text = string.Empty;
            SubstanceNameBox.Text = string.Empty;
            PacksBox.Text = string.Empty;
        }

        private void ClearItemUpdateBoxes()
        {
            IdBox.Text = string.Empty;
            NameBoxUpdate.Text = string.Empty;
            ProducerBoxUpdate.Text = string.Empty;
            PriceBoxUpdate.Text = string.Empty;
            CategoryBoxUpdate.Text = string.Empty;
            ImagePathBoxUpdate.Text = string.Empty;
            NumberOfPillsBoxUpdate.Text = string.Empty;
            LabelBoxUpdate.Text = string.Empty;
            DescriptionBoxUpdate.Text = string.Empty;
            DiscountBoxUpdate.Text = string.Empty;
            SubstanceNameBoxUpdate.Text = string.Empty;
            PacksBoxUpdate.Text = string.Empty;
        }

        private void ClearSubstanceBoxes()
        {
            NameBoxSubstance.Text = string.Empty;
            LethalDoseBoxSubstance.Text = string.Empty;
            DescriptionBoxSubstance.Text = string.Empty;
        }

        private void ClearSubstanceUpdateBoxes()
        {
            NameBoxSubstanceUpdate.Text = string.Empty;
            LethalDoseBoxSubstanceUpdate.Text = string.Empty;
            DescriptionBoxSubstanceUpdate.Text = string.Empty;
        }

        private void OnItemAddClick(object sender, RoutedEventArgs e)
        {
            RemoveUpdateActiveSubstanceError.Visibility = Visibility.Collapsed;

            if (AddItemGrid.Visibility == Visibility.Visible)
            {
                AddItemGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                AddItemGrid.Visibility = Visibility.Visible;
                UpdateItemGrid.Visibility = Visibility.Collapsed;
                ResetAddItemErrors();
                ResetActiveSubstanceErrors();
                ResetAddBatchErrors();
            }
        }
        private void OnAddItemClick(object sender, RoutedEventArgs e)
        {
            if (!ValidateAddItem())
            {
                return;
            }
            float discount = 0f;
            int quantity = 0;

            string name = NameBox.Text;
            string producer = ProducerBox.Text;
            string category = CategoryBox.Text;
            string imagePath = ImagePathBox.Text;
            float price = float.Parse(PriceBox.Text);
            int numberOfPills = int.Parse(NumberOfPillsBox.Text);
            if (DiscountBox.Text != string.Empty)
            {
                discount = float.Parse(DiscountBox.Text);
            }

            string label = LabelBox.Text;
            string description = DescriptionBox.Text;

            Item newItem = new Item(name, producer, category, price, numberOfPills, quantity, label, description, imagePath, discount);

            for (int i = 0; i < BatchesDict.Count; i++)
            {
                newItem.AddNewBatchToItem(BatchesDict.ElementAt(i).Key, BatchesDict.ElementAt(i).Value);
                System.Diagnostics.Debug.WriteLine("Added batch: " + BatchesDict.ElementAt(i).Key + " " + BatchesDict.ElementAt(i).Value);
            }

            for (int i = 0; i < ActiveSubstancesDict.Count; i++)
            {
                newItem.AddActiveSubstanceToItem(ActiveSubstancesDict.ElementAt(i).Key, ActiveSubstancesDict.ElementAt(i).Value);
                System.Diagnostics.Debug.WriteLine("Added active substance: " + ActiveSubstancesDict.ElementAt(i).Key + " " + ActiveSubstancesDict.ElementAt(i).Value);
            }

            try
            {
                ViewModel.AddItemWithQuantity(newItem);
            }
            catch (ArgumentException ex)
            {
                AddItemFormatError.Text = ex.Message;
                AddItemFormatError.Visibility = Visibility.Visible;
                return;
            }

            ViewModel.RefreshItems();
            ItemList.ItemsSource = ViewModel.Items;

            System.Diagnostics.Debug.WriteLine("Added item");

            ClearItemAddBoxes();
            ActiveSubstancesDict.Clear();
            RefreshActiveSubstancesList();
            BatchesDict.Clear();
            RefreshBatchesList();
            ResetAddItemErrors();
        }

        private bool ValidateAddItem()
        {
            bool isValid = true;
            if (NameBox.Text == string.Empty)
            {
                NameBox.Background = new SolidColorBrush(Colors.LightPink);
                NameBox.Text = string.Empty;
                AddItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (ProducerBox.Text == string.Empty)
            {
                ProducerBox.Background = new SolidColorBrush(Colors.LightPink);
                ProducerBox.Text = string.Empty;
                AddItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }
            if (CategoryBox.Text == string.Empty)
            {
                CategoryBox.Background = new SolidColorBrush(Colors.LightPink);
                CategoryBox.Text = string.Empty;
                AddItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }
            if (PriceBox.Text == string.Empty)
            {
                PriceBox.Background = new SolidColorBrush(Colors.LightPink);
                PriceBox.Text = string.Empty;
                AddItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }
            if (NumberOfPillsBox.Text == string.Empty)
            {
                NumberOfPillsBox.Background = new SolidColorBrush(Colors.LightPink);
                NumberOfPillsBox.Text = string.Empty;
                AddItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!float.TryParse(PriceBox.Text, out float price))
            {
                PriceBox.Background = new SolidColorBrush(Colors.LightPink);
                PriceBox.Text = string.Empty;
                AddItemFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!int.TryParse(NumberOfPillsBox.Text, out int numberOfPills))
            {
                NumberOfPillsBox.Background = new SolidColorBrush(Colors.LightPink);
                NumberOfPillsBox.Text = string.Empty;
                AddItemFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!float.TryParse(DiscountBox.Text, out float discount) && DiscountBox.Text != string.Empty)
            {
                DiscountBox.Background = new SolidColorBrush(Colors.LightPink);
                DiscountBox.Text = string.Empty;
                AddItemFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (ActiveSubstancesDict.Count == 0)
            {
                SubstanceNameBox.Background = new SolidColorBrush(Colors.LightPink);
                SubstanceNameBox.Text = string.Empty;
                ConcentrationBox.Background = new SolidColorBrush(Colors.LightPink);
                ConcentrationBox.Text = string.Empty;
                AddItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            return isValid;
        }

        private void ResetAddItemErrors()
        {
            NameBox.Background = new SolidColorBrush(Colors.White);
            ProducerBox.Background = new SolidColorBrush(Colors.White);
            CategoryBox.Background = new SolidColorBrush(Colors.White);
            PriceBox.Background = new SolidColorBrush(Colors.White);
            NumberOfPillsBox.Background = new SolidColorBrush(Colors.White);
            DiscountBox.Background = new SolidColorBrush(Colors.White);
            SubstanceNameBox.Background = new SolidColorBrush(Colors.White);
            ConcentrationBox.Background = new SolidColorBrush(Colors.White);
            AddItemMandatoryError.Visibility = Visibility.Collapsed;
            AddItemFormatError.Text = "Wrong Format!";
            AddItemFormatError.Visibility = Visibility.Collapsed;
        }

        private void RefreshActiveSubstancesList()
        {
            var list = ActiveSubstancesDict
                .Select(kvp => new ActiveSubstance
                {
                    Name = kvp.Key,
                    Concentration = kvp.Value
                })
                .ToList();

            ActiveSubstancesList.ItemsSource = list;
        }

        private void AddSubstance_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateAddItemSubstance())
            {
                return;
            }

            string name = SubstanceNameBox.Text;
            float concentration = float.Parse(ConcentrationBox.Text);

            ActiveSubstancesDict[name] = concentration;

            RefreshActiveSubstancesList();

            SubstanceNameBox.Text = string.Empty;
            ConcentrationBox.Text = string.Empty;
            ResetActiveSubstanceErrors();
        }

        private bool ValidateAddItemSubstance()
        {
            bool isValid = true;
            AddActiveSubstanceToItemInvalidError.Text = "Invalid input!";
            if (SubstanceNameBox.Text == string.Empty)
            {
                SubstanceNameBox.Background = new SolidColorBrush(Colors.LightPink);
                SubstanceNameBox.Text = string.Empty;
                AddActiveSubstanceToItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (ConcentrationBox.Text == string.Empty)
            {
                ConcentrationBox.Background = new SolidColorBrush(Colors.LightPink);
                ConcentrationBox.Text = string.Empty;
                AddActiveSubstanceToItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!float.TryParse(ConcentrationBox.Text, out float concentration))
            {
                ConcentrationBox.Background = new SolidColorBrush(Colors.LightPink);
                ConcentrationBox.Text = string.Empty;
                AddActiveSubstanceToItemFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (ActiveSubstancesDict.ContainsKey(SubstanceNameBox.Text))
            {
                SubstanceNameBox.Background = new SolidColorBrush(Colors.LightPink);
                SubstanceNameBox.Text = string.Empty;
                AddActiveSubstanceToItemInvalidError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!ViewModel.SubstanceExists(SubstanceNameBox.Text))
            {
                SubstanceNameBox.Background = new SolidColorBrush(Colors.LightPink);
                SubstanceNameBox.Text = string.Empty;
                ConcentrationBox.Text = string.Empty;
                AddActiveSubstanceToItemInvalidError.Text = "Substance must exist in database";
                AddActiveSubstanceToItemInvalidError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (isValid)
            {
                Substance substance = ViewModel.GetSubstanceByName(SubstanceNameBox.Text);

                if (concentration >= substance.LethalDose)
                {
                    ConcentrationBox.Background = new SolidColorBrush(Colors.LightPink);
                    AddActiveSubstanceToItemInvalidError.Text =
                        $"Concentration must be lower than lethal dosage ({substance.LethalDose})";
                    AddActiveSubstanceToItemInvalidError.Visibility = Visibility.Visible;
                    isValid = false;
                }
            }

            return isValid;
        }

        private void RemoveSubstance_Click(object sender, RoutedEventArgs e)
        {
            string name = SubstanceNameBox.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                SubstanceNameBox.Background = new SolidColorBrush(Colors.LightPink);
                RemoveUpdateActiveSubstanceError.Visibility = Visibility.Visible;
                return;
            }

            if (ActiveSubstancesDict.ContainsKey(name))
            {
                ActiveSubstancesDict.Remove(name);
            }
            else
            {
                RemoveUpdateActiveSubstanceError.Visibility = Visibility.Visible;
                return;
            }

            RefreshActiveSubstancesListUpdate();

            SubstanceNameBox.Text = string.Empty;
            ConcentrationBox.Text = string.Empty;
            ResetActiveSubstanceErrors();
        }

        private void ResetActiveSubstanceErrors()
        {
            SubstanceNameBox.Background = new SolidColorBrush(Colors.White);
            ConcentrationBox.Background = new SolidColorBrush(Colors.White);
            AddActiveSubstanceToItemMandatoryError.Visibility = Visibility.Collapsed;
            AddActiveSubstanceToItemFormatError.Visibility = Visibility.Collapsed;
            RemoveActiveSubstanceFromItemError.Visibility = Visibility.Collapsed;
            AddActiveSubstanceToItemInvalidError.Visibility = Visibility.Collapsed;
        }
        private void RefreshBatchesList()
        {
            var list = BatchesDict
                .Select(kvp => new BatchItem
                {
                    Date = kvp.Key,
                    Packs = kvp.Value
                })
                .OrderBy(x => x.Date)
                .ToList();

            BatchesList.ItemsSource = list;
        }

        private void AddBatch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateAddBatch())
            {
                return;
            }

            int packs = int.Parse(PacksBox.Text);
            DateOnly date = DateOnly.FromDateTime(BatchDatePicker.Date.Date);

            BatchesDict[date] = packs;

            RefreshBatchesList();

            PacksBox.Text = string.Empty;
            ResetAddBatchErrors();
        }

        private bool ValidateAddBatch()
        {
            bool isValid = true;
            AddBatchFormatError.Text = "Wrong Format!";

            DateOnly selectedDate = DateOnly.FromDateTime(BatchDatePicker.Date.Date);
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);

            if (selectedDate <= currentDate)
            {
                AddBatchFormatError.Text = "expiration date must be later than current date";
                AddBatchFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (PacksBox.Text == string.Empty)
            {
                PacksBox.Background = new SolidColorBrush(Colors.LightPink);
                AddBatchMandatoryError.Visibility = Visibility.Visible;
                PacksBox.Text = string.Empty;
                isValid = false;
            }

            if (!int.TryParse(PacksBox.Text, out int packs))
            {
                PacksBox.Background = new SolidColorBrush(Colors.LightPink);
                AddBatchFormatError.Text = "invalid quantity";
                AddBatchFormatError.Visibility = Visibility.Visible;
                PacksBox.Text = string.Empty;
                isValid = false;
            }
            return isValid;
        }

        public void ResetAddBatchErrors()
        {
            BatchDatePicker.Background = new SolidColorBrush(Colors.White);
            PacksBox.Background = new SolidColorBrush(Colors.White);
            AddBatchMandatoryError.Visibility = Visibility.Collapsed;
            AddBatchFormatError.Visibility = Visibility.Collapsed;
            RemoveBatchFromItemError.Visibility = Visibility.Collapsed;
        }

        private void RemoveBatchFromItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedBatch = BatchesList.SelectedItem as BatchItem;

            if (selectedBatch != null)
            {
                if (BatchesDict.ContainsKey(selectedBatch.Date))
                {
                    BatchesDict.Remove(selectedBatch.Date);

                    RefreshBatchesList();
                }
            }
            else
            {
                RemoveBatchFromItemError.Visibility = Visibility.Visible;
            }
        }

        private void OnItemCancelClick(object sender, RoutedEventArgs e)
        {
            ClearItemAddBoxes();
            ClearItemUpdateBoxes();
            ActiveSubstancesDict.Clear();
            BatchesDict.Clear();
            RefreshActiveSubstancesList();
            RefreshActiveSubstancesListUpdate();
            RefreshBatchesList();
            RefreshBatchesListUpdate();
            isGetItemDataClicked = false;

            ResetAddItemErrors();
            ResetUpdateItemErrors();
            ResetActiveSubstanceErrors();
            ResetAddBatchErrors();

            AddItemGrid.Visibility = Visibility.Collapsed;
            UpdateItemGrid.Visibility = Visibility.Collapsed;
        }

        private void OnItemRemoveClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = ItemList.SelectedItem as Item;
            if (selectedItem == null)
            {
                RemoveItemError.Visibility = Visibility.Visible;
                return;
            }

            int id = selectedItem.Id;

            ViewModel.RemoveItemById(id);
            ViewModel.RefreshItems();
            ItemList.ItemsSource = ViewModel.Items;
            RemoveItemError.Visibility = Visibility.Collapsed;
        }

        private void OnItemUpdateClick(object sender, RoutedEventArgs e)
        {
            RemoveUpdateActiveSubstanceError.Visibility = Visibility.Collapsed;

            if (UpdateItemGrid.Visibility == Visibility.Visible)
            {
                UpdateItemGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                UpdateItemGrid.Visibility = Visibility.Visible;
                AddItemGrid.Visibility = Visibility.Collapsed;
                ResetUpdateItemErrors();
            }
        }

        private void OnGetItemDataClick(object sender, RoutedEventArgs e)
        {
            if (!ValidateGetItemData())
            {
                return;
            }

            Item item;
            try
            {
                item = ViewModel.GetItemById(int.Parse(IdBox.Text));
            }
            catch (Exception)
            {
                IdBox.Background = new SolidColorBrush(Colors.LightPink);
                IdBox.Text = string.Empty;
                UpdateInvalidIdError.Visibility = Visibility.Visible;
                return;
            }
            NameBoxUpdate.Text = item.Name;
            ProducerBoxUpdate.Text = item.Producer;
            PriceBoxUpdate.Text = item.Price.ToString();
            CategoryBoxUpdate.Text = item.Category;
            ImagePathBoxUpdate.Text = item.ImagePath;
            NumberOfPillsBoxUpdate.Text = item.NumberOfPills.ToString();
            LabelBoxUpdate.Text = item.Label;
            DescriptionBoxUpdate.Text = item.Description;
            DiscountBoxUpdate.Text = item.DiscountPercentage.ToString();

            ActiveSubstancesDict = item.ActiveSubstances;
            RefreshActiveSubstancesListUpdate();
            BatchesDict = item.Batches;
            RefreshBatchesListUpdate();

            ResetUpdateItemErrors();
            isGetItemDataClicked = true;
        }

        private bool ValidateGetItemData()
        {
            bool isValid = true;

            if (IdBox.Text == string.Empty)
            {
                IdBox.Background = new SolidColorBrush(Colors.LightPink);
                IdBox.Text = string.Empty;
                UpdateItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!int.TryParse(IdBox.Text, out int id))
            {
                IdBox.Background = new SolidColorBrush(Colors.LightPink);
                IdBox.Text = string.Empty;
                UpdateItemFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                if (ViewModel.GetItemById(id) is null)
                {
                    IdBox.Background = new SolidColorBrush(Colors.LightPink);
                    IdBox.Text = string.Empty;
                    UpdateInvalidIdError.Visibility = Visibility.Visible;
                    isValid = false;
                }
            }

            return isValid;
        }

        private void OnUpdateItemClick(object sender, RoutedEventArgs e)
        {
            if (!ValidateUpdateItem())
            {
                return;
            }

            int id = int.Parse(IdBox.Text);

            string name = NameBoxUpdate.Text;
            string producer = ProducerBoxUpdate.Text;
            string category = CategoryBoxUpdate.Text;
            string imagePath = ImagePathBoxUpdate.Text;
            string label = LabelBoxUpdate.Text;
            string description = DescriptionBoxUpdate.Text;
            float price = float.Parse(PriceBoxUpdate.Text);
            float discount = 0f;
            if (DiscountBoxUpdate.Text != string.Empty)
            {
                discount = float.Parse(DiscountBoxUpdate.Text);
            }
            int numberOfPills = int.Parse(NumberOfPillsBoxUpdate.Text);
            int quantity = 0;

            for (int i = 0; i < BatchesDict.Count; i++)
            {
                quantity += BatchesDict.ElementAt(i).Value;
            }

            ViewModel.UpdateItemById(id, new Item(name, producer, category, price, numberOfPills, ActiveSubstancesDict, BatchesDict, quantity, label, description, imagePath, discount));

            ViewModel.RefreshItems();
            ItemList.ItemsSource = ViewModel.Items;
            ClearItemUpdateBoxes();
            ActiveSubstancesDict.Clear();
            RefreshActiveSubstancesListUpdate();
            BatchesDict.Clear();
            RefreshBatchesListUpdate();
            ResetUpdateItemErrors();
        }
        private bool ValidateUpdateItem()
        {
            bool isValid = true;

            if (!isGetItemDataClicked)
            {
                UpdateItemGetDataError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (NameBoxUpdate.Text == string.Empty)
            {
                NameBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                NameBoxUpdate.Text = string.Empty;
                UpdateItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (ProducerBoxUpdate.Text == string.Empty)
            {
                ProducerBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                ProducerBoxUpdate.Text = string.Empty;
                UpdateItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }
            if (CategoryBoxUpdate.Text == string.Empty)
            {
                CategoryBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                CategoryBoxUpdate.Text = string.Empty;
                UpdateItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (IdBox.Text == string.Empty)
            {
                IdBox.Background = new SolidColorBrush(Colors.LightPink);
                IdBox.Text = string.Empty;
                UpdateItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (PriceBoxUpdate.Text == string.Empty)
            {
                PriceBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                UpdateItemMandatoryError.Visibility = Visibility.Visible;
            }

            if (!float.TryParse(PriceBoxUpdate.Text, out float price))
            {
                PriceBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                PriceBoxUpdate.Text = string.Empty;
                UpdateItemFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (NumberOfPillsBoxUpdate.Text == string.Empty)
            {
                NumberOfPillsBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                UpdateItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!int.TryParse(NumberOfPillsBoxUpdate.Text, out int numberOfPills))
            {
                NumberOfPillsBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                NumberOfPillsBoxUpdate.Text = string.Empty;
                UpdateItemFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!float.TryParse(DiscountBoxUpdate.Text, out float discount) && DiscountBoxUpdate.Text != string.Empty)
            {
                DiscountBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                DiscountBoxUpdate.Text = string.Empty;
                UpdateItemFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (ActiveSubstancesDict.Count == 0)
            {
                SubstanceNameBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                SubstanceNameBoxUpdate.Text = string.Empty;
                ConcentrationBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                ConcentrationBoxUpdate.Text = string.Empty;
                UpdateItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            return isValid;
        }

        private void ResetUpdateItemErrors()
        {
            IdBox.Background = new SolidColorBrush(Colors.White);
            NameBoxUpdate.Background = new SolidColorBrush(Colors.White);
            ProducerBoxUpdate.Background = new SolidColorBrush(Colors.White);
            CategoryBoxUpdate.Background = new SolidColorBrush(Colors.White);
            PriceBoxUpdate.Background = new SolidColorBrush(Colors.White);
            NumberOfPillsBoxUpdate.Background = new SolidColorBrush(Colors.White);
            DiscountBoxUpdate.Background = new SolidColorBrush(Colors.White);
            SubstanceNameBoxUpdate.Background = new SolidColorBrush(Colors.White);
            ConcentrationBoxUpdate.Background = new SolidColorBrush(Colors.White);
            UpdateItemMandatoryError.Visibility = Visibility.Collapsed;
            UpdateItemFormatError.Visibility = Visibility.Collapsed;
            UpdateInvalidIdError.Visibility = Visibility.Collapsed;
            UpdateItemGetDataError.Visibility = Visibility.Collapsed;
            RemoveUpdateActiveSubstanceError.Visibility = Visibility.Collapsed;
            UpdateActiveSubstanceMandatoryError.Visibility = Visibility.Collapsed;
            UpdateActiveSubstanceFormatError.Visibility = Visibility.Collapsed;
            UpdateActiveSubstanceInvalidError.Visibility = Visibility.Collapsed;
            BatchDatePickerUpdate.Background = new SolidColorBrush(Colors.White);
            PacksBoxUpdate.Background = new SolidColorBrush(Colors.White);
            UpdateBatchMandatoryError.Visibility = Visibility.Collapsed;
            UpdateBatchFormatError.Visibility = Visibility.Collapsed;
            RemoveUpdateBatchError.Visibility = Visibility.Collapsed;
        }

        private void OnSubstanceAddClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.AddSubstanceGridVisibility == Visibility.Visible)
            {
                ViewModel.AddSubstanceGridVisibility = Visibility.Collapsed;
            }
            else
            {
                ViewModel.AddSubstanceGridVisibility = Visibility.Visible;
                ViewModel.UpdateSubstanceGridVisibility = Visibility.Collapsed;
                ResetSubstanceErrors();
            }

            ApplyUiStateFromViewModel();
        }

        private void OnSubstanceUpdateClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.UpdateSubstanceGridVisibility == Visibility.Visible)
            {
                ViewModel.UpdateSubstanceGridVisibility = Visibility.Collapsed;
            }
            else
            {
                ViewModel.UpdateSubstanceGridVisibility = Visibility.Visible;
                ViewModel.AddSubstanceGridVisibility = Visibility.Collapsed;
                ResetSubstanceErrors();
            }

            ApplyUiStateFromViewModel();
        }

        private void RefreshActiveSubstancesListUpdate()
        {
            var list = ActiveSubstancesDict
                .Select(kvp => new ActiveSubstance
                {
                    Name = kvp.Key,
                    Concentration = kvp.Value
                })
                .ToList();

            ActiveSubstancesListUpdate.ItemsSource = list;
        }

        private void AddSubstanceUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateUpdateItemSubstance())
            {
                return;
            }

            string name = SubstanceNameBoxUpdate.Text;
            float concentration = float.Parse(ConcentrationBoxUpdate.Text);

            ActiveSubstancesDict[name] = concentration;

            RefreshActiveSubstancesListUpdate();

            SubstanceNameBoxUpdate.Text = string.Empty;
            ConcentrationBoxUpdate.Text = string.Empty;
            ResetUpdateItemErrors();
        }

        private bool ValidateUpdateItemSubstance()
        {
            bool isValid = true;
            UpdateActiveSubstanceInvalidError.Text = "Invalid input!";
            if (SubstanceNameBoxUpdate.Text == string.Empty)
            {
                SubstanceNameBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                SubstanceNameBoxUpdate.Text = string.Empty;
                UpdateActiveSubstanceMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (ConcentrationBoxUpdate.Text == string.Empty)
            {
                ConcentrationBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                ConcentrationBoxUpdate.Text = string.Empty;
                UpdateActiveSubstanceMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!float.TryParse(ConcentrationBoxUpdate.Text, out float concentration))
            {
                ConcentrationBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                ConcentrationBoxUpdate.Text = string.Empty;
                UpdateActiveSubstanceFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (ActiveSubstancesDict.ContainsKey(SubstanceNameBoxUpdate.Text))
            {
                SubstanceNameBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                SubstanceNameBoxUpdate.Text = string.Empty;
                UpdateActiveSubstanceInvalidError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!ViewModel.SubstanceExists(SubstanceNameBoxUpdate.Text))
            {
                SubstanceNameBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                SubstanceNameBoxUpdate.Text = string.Empty;
                ConcentrationBoxUpdate.Text = string.Empty;
                UpdateActiveSubstanceInvalidError.Text = "Substance must exist in database";
                UpdateActiveSubstanceInvalidError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (isValid)
            {
                Substance substance = ViewModel.GetSubstanceByName(SubstanceNameBoxUpdate.Text);

                if (concentration >= substance.LethalDose)
                {
                    ConcentrationBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                    UpdateActiveSubstanceInvalidError.Text =
                        $"Concentration must be lower than lethal dosage ({substance.LethalDose})";
                    UpdateActiveSubstanceInvalidError.Visibility = Visibility.Visible;
                    isValid = false;
                }
            }

            return isValid;
        }

        private void RemoveSubstanceUpdate_Click(object sender, RoutedEventArgs e)
        {
            string name = SubstanceNameBoxUpdate.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                SubstanceNameBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                RemoveUpdateActiveSubstanceError.Visibility = Visibility.Visible;
                return;
            }

            if (ActiveSubstancesDict.ContainsKey(name))
            {
                ActiveSubstancesDict.Remove(name);
            }
            else
            {
                RemoveUpdateActiveSubstanceError.Visibility = Visibility.Visible;
                return;
            }

            RefreshActiveSubstancesListUpdate();

            SubstanceNameBoxUpdate.Text = string.Empty;
            ConcentrationBoxUpdate.Text = string.Empty;
            ResetUpdateItemErrors();
        }

        private void RefreshBatchesListUpdate()
        {
            var list = BatchesDict
                .Select(kvp => new BatchItem
                {
                    Date = kvp.Key,
                    Packs = kvp.Value
                })
                .OrderBy(x => x.Date)
                .ToList();

            BatchesListUpdate.ItemsSource = list;
        }

        private void AddBatchUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateUpdateBatch())
            {
                return;
            }

            int packs = int.Parse(PacksBoxUpdate.Text);
            DateOnly date = DateOnly.FromDateTime(BatchDatePickerUpdate.Date.Date);

            BatchesDict[date] = packs;

            RefreshBatchesListUpdate();

            PacksBoxUpdate.Text = string.Empty;
            ResetUpdateItemErrors();
        }

        private bool ValidateUpdateBatch()
        {
            bool isValid = true;
            UpdateBatchFormatError.Text = "Wrong Format!";

            DateOnly selectedDate = DateOnly.FromDateTime(BatchDatePickerUpdate.Date.Date);
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);

            if (selectedDate <= currentDate)
            {
                UpdateBatchFormatError.Text = "expiration date must be later than current date";
                UpdateBatchFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (PacksBoxUpdate.Text == string.Empty)
            {
                PacksBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                UpdateBatchMandatoryError.Visibility = Visibility.Visible;
                PacksBoxUpdate.Text = string.Empty;
                isValid = false;
            }

            if (!int.TryParse(PacksBoxUpdate.Text, out int packs))
            {
                PacksBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                UpdateBatchFormatError.Text = "invalid quantity";
                UpdateBatchFormatError.Visibility = Visibility.Visible;
                PacksBoxUpdate.Text = string.Empty;
                isValid = false;
            }
            return isValid;
        }

        private void RemoveBatchFromItemUpdate_Click(object sender, RoutedEventArgs e)
        {
            var selectedBatch = BatchesListUpdate.SelectedItem as BatchItem;

            if (selectedBatch != null)
            {
                if (BatchesDict.ContainsKey(selectedBatch.Date))
                {
                    BatchesDict.Remove(selectedBatch.Date);

                    RefreshBatchesListUpdate();
                }
            }
            else
            {
                RemoveUpdateBatchError.Visibility = Visibility.Visible;
            }
        }

        private void ResetSubstanceErrors()
        {
            NameBoxSubstance.Background = new SolidColorBrush(Colors.White);
            LethalDoseBoxSubstance.Background = new SolidColorBrush(Colors.White);
            DescriptionBoxSubstance.Background = new SolidColorBrush(Colors.White);
            NameBoxSubstanceUpdate.Background = new SolidColorBrush(Colors.White);
            LethalDoseBoxSubstanceUpdate.Background = new SolidColorBrush(Colors.White);
            DescriptionBoxSubstanceUpdate.Background = new SolidColorBrush(Colors.White);
            RemoveSubstanceError.Visibility = Visibility.Collapsed;
            AddSubstanceFormatError.Visibility = Visibility.Collapsed;
            AddSubstanceMandatoryError.Visibility = Visibility.Collapsed;
            UpdateSubstanceFormatError.Visibility = Visibility.Collapsed;
            UpdateSubstanceMandatoryError.Visibility = Visibility.Collapsed;
            UpdateSubstanceInvalidError.Visibility = Visibility.Collapsed;
        }
        private void OnSubstanceRemoveClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = SubstanceList.SelectedItem as Substance;
            if (selectedItem == null)
            {
                RemoveSubstanceError.Visibility = Visibility.Visible;
                return;
            }

            ViewModel.RemoveSubstanceByName(selectedItem);
            ViewModel.RefreshSubstances();
            SubstanceList.ItemsSource = ViewModel.Substances;
            ResetSubstanceErrors();
        }

        private void OnSubstanceCancelClick(object sender, RoutedEventArgs e)
        {
            ClearSubstanceBoxes();
            ClearSubstanceUpdateBoxes();
            ViewModel.AddSubstanceGridVisibility = Visibility.Collapsed;
            ViewModel.UpdateSubstanceGridVisibility = Visibility.Collapsed;
            ApplyUiStateFromViewModel();
            ResetSubstanceErrors();
        }

        private void OnAddSubstanceClick(object sender, RoutedEventArgs e)
        {
            if (!ValidateAddSubstance())
            {
                return;
            }

            string name = NameBoxSubstance.Text;
            int lethalDose = int.Parse(LethalDoseBoxSubstance.Text);
            string description = DescriptionBoxSubstance.Text;

            Substance newSubstance = new Substance(name, lethalDose, description);

            try
            {
                ViewModel.AddSubstance(newSubstance);
            }
            catch (ArgumentException ex)
            {
                AddSubstanceFormatError.Text = ex.Message;
                AddSubstanceFormatError.Visibility = Visibility.Visible;
                return;
            }

            ClearSubstanceBoxes();
            ViewModel.RefreshSubstances();
            SubstanceList.ItemsSource = ViewModel.Substances;
            ResetSubstanceErrors();
        }

        private bool ValidateAddSubstance()
        {
            bool isValid = true;
            if (NameBoxSubstance.Text == string.Empty)
            {
                NameBoxSubstance.Background = new SolidColorBrush(Colors.LightPink);
                NameBoxSubstance.Text = string.Empty;
                AddSubstanceMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }
            if (LethalDoseBoxSubstance.Text == string.Empty)
            {
                LethalDoseBoxSubstance.Background = new SolidColorBrush(Colors.LightPink);
                LethalDoseBoxSubstance.Text = string.Empty;
                AddSubstanceMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (LethalDoseBoxSubstance.Text == string.Empty)
            {
                LethalDoseBoxSubstance.Background = new SolidColorBrush(Colors.LightPink);
                LethalDoseBoxSubstance.Text = string.Empty;
                AddSubstanceMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!int.TryParse(LethalDoseBoxSubstance.Text, out int lethalDose))
            {
                LethalDoseBoxSubstance.Background = new SolidColorBrush(Colors.LightPink);
                LethalDoseBoxSubstance.Text = string.Empty;
                AddSubstanceFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            return isValid;
        }

        private void OnUpdateSubstanceClick(object sender, RoutedEventArgs e)
        {
            if (!ValidateUpdateSubstance())
            {
                return;
            }
            string name = NameBoxSubstanceUpdate.Text;
            int lethalDose = int.Parse(LethalDoseBoxSubstanceUpdate.Text);
            string description = DescriptionBoxSubstanceUpdate.Text;

            Substance updatedSubstance = new Substance(name, lethalDose, description);

            ViewModel.UpdateSubstanceByName(name, updatedSubstance);

            System.Diagnostics.Debug.WriteLine("Updated substance");

            ClearSubstanceUpdateBoxes();
            ViewModel.RefreshSubstances();
            SubstanceList.ItemsSource = ViewModel.Substances;
            ResetSubstanceErrors();
        }

        private bool ValidateUpdateSubstance()
        {
            bool isValid = true;
            if (NameBoxSubstanceUpdate.Text == string.Empty)
            {
                NameBoxSubstanceUpdate.Background = new SolidColorBrush(Colors.LightPink);
                NameBoxSubstanceUpdate.Text = string.Empty;
                UpdateSubstanceMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!ViewModel.SubstanceExists(NameBoxSubstanceUpdate.Text))
            {
                NameBoxSubstanceUpdate.Background = new SolidColorBrush(Colors.LightPink);
                NameBoxSubstanceUpdate.Text = string.Empty;
                UpdateSubstanceInvalidError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (LethalDoseBoxSubstanceUpdate.Text == string.Empty)
            {
                LethalDoseBoxSubstanceUpdate.Background = new SolidColorBrush(Colors.LightPink);
                LethalDoseBoxSubstanceUpdate.Text = string.Empty;
                UpdateSubstanceMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }
            if (!int.TryParse(LethalDoseBoxSubstanceUpdate.Text, out int lethalDose))
            {
                LethalDoseBoxSubstanceUpdate.Background = new SolidColorBrush(Colors.LightPink);
                LethalDoseBoxSubstanceUpdate.Text = string.Empty;
                UpdateSubstanceFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }
            return isValid;
        }
    }
}


