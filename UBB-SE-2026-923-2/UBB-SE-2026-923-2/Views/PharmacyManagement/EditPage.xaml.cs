// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace UBB_SE_2026_923_2.Views.PharmacyManagement
{
    using ABI.System.Collections.Generic;
    using Microsoft.UI;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
    using Microsoft.UI.Xaml.Navigation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.ViewModels.PharmacyManagement;

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
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            this.ViewModel = new EditPageViewModel();
            this.DataContext = this.ViewModel;
            this.ApplyUiStateFromViewModel();
            this.ResetUiValidationState();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ResetUiValidationState();
        }

        private void ApplyUiStateFromViewModel()
        {
            ItemListButtons.Visibility = this.ViewModel.ItemListButtonsVisibility;
            ItemBottomButtons.Visibility = this.ViewModel.ItemBottomButtonsVisibility;
            ShowExpiredItemsToggle.Visibility = this.ViewModel.ShowExpiredItemsToggleVisibility;

            SubstanceListButtons.Visibility = this.ViewModel.SubstanceListButtonsVisibility;
            SubstanceBottomButtons.Visibility = this.ViewModel.SubstanceBottomButtonsVisibility;
            AddSubstanceGrid.Visibility = this.ViewModel.AddSubstanceGridVisibility;
            UpdateSubstanceGrid.Visibility = this.ViewModel.UpdateSubstanceGridVisibility;
        }

        private void ResetUiValidationState()
        {
            this.isGetItemDataClicked = false;

            this.ViewModel.ActivateItemsSection();
            this.ViewModel.AddSubstanceGridVisibility = Visibility.Collapsed;
            this.ViewModel.UpdateSubstanceGridVisibility = Visibility.Collapsed;
            this.ApplyUiStateFromViewModel();

            AddItemGrid.Visibility = Visibility.Collapsed;
            UpdateItemGrid.Visibility = Visibility.Collapsed;

            SearchBox.Text = string.Empty;
            ShowExpiredToggle.IsOn = false;

            this.ClearItemAddBoxes();
            this.ClearItemUpdateBoxes();
            this.ClearSubstanceBoxes();
            this.ClearSubstanceUpdateBoxes();

            this.ActiveSubstancesDict.Clear();
            this.BatchesDict.Clear();
            this.RefreshActiveSubstancesList();
            this.RefreshActiveSubstancesListUpdate();
            this.RefreshBatchesList();
            this.RefreshBatchesListUpdate();

            ItemList.SelectedItem = null;
            SubstanceList.SelectedItem = null;

            RemoveItemError.Visibility = Visibility.Collapsed;
            RemoveSubstanceError.Visibility = Visibility.Collapsed;

            this.ResetAddItemErrors();
            this.ResetUpdateItemErrors();
            this.ResetSubstanceErrors();
            this.ResetActiveSubstanceErrors();
            this.ResetAddBatchErrors();
        }

        private void GoToStatisticsClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(StatisticsPage));
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ViewModel.SearchItems(SearchBox.Text);
            ItemList.ItemsSource = this.ViewModel.Items;
        }

        private void OnItemClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ActivateItemsSection();
            this.ApplyUiStateFromViewModel();
            this.ResetSubstanceErrors();
        }

        private void OnSubstancesClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ActivateSubstancesSection();
            this.ApplyUiStateFromViewModel();

            AddItemGrid.Visibility = Visibility.Collapsed;
            UpdateItemGrid.Visibility = Visibility.Collapsed;
            RemoveItemError.Visibility = Visibility.Collapsed;
        }

        private void OnOrdersClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Views.Orders.OrderManagementPage), new OrderService());
        }

        private void ShowExpiredToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (ShowExpiredToggle.IsOn)
            {
                this.ViewModel.ShowExpiredItems();
                ItemList.ItemsSource = this.ViewModel.Items;
            }
            else
            {
                this.ViewModel.RefreshItems();
                ItemList.ItemsSource = this.ViewModel.Items;
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
                this.ResetAddItemErrors();
                this.ResetActiveSubstanceErrors();
                this.ResetAddBatchErrors();
            }
        }

        private void OnAddItemClick(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateAddItem())
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

            for (int i = 0; i < this.BatchesDict.Count; i++)
            {
                newItem.AddNewBatchToItem(this.BatchesDict.ElementAt(i).Key, this.BatchesDict.ElementAt(i).Value);
                System.Diagnostics.Debug.WriteLine("Added batch: " + this.BatchesDict.ElementAt(i).Key + " " + this.BatchesDict.ElementAt(i).Value);
            }

            for (int i = 0; i < this.ActiveSubstancesDict.Count; i++)
            {
                newItem.AddActiveSubstanceToItem(this.ActiveSubstancesDict.ElementAt(i).Key, this.ActiveSubstancesDict.ElementAt(i).Value);
                System.Diagnostics.Debug.WriteLine("Added active substance: " + this.ActiveSubstancesDict.ElementAt(i).Key + " " + this.ActiveSubstancesDict.ElementAt(i).Value);
            }

            try
            {
                this.ViewModel.AddItemWithQuantity(newItem);
            }
            catch (ArgumentException exception)
            {
                AddItemFormatError.Text = exception.Message;
                AddItemFormatError.Visibility = Visibility.Visible;
                return;
            }

            this.ViewModel.RefreshItems();
            ItemList.ItemsSource = this.ViewModel.Items;

            System.Diagnostics.Debug.WriteLine("Added item");

            this.ClearItemAddBoxes();
            this.ActiveSubstancesDict.Clear();
            this.RefreshActiveSubstancesList();
            this.BatchesDict.Clear();
            this.RefreshBatchesList();
            this.ResetAddItemErrors();
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

            if (this.ActiveSubstancesDict.Count == 0)
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
            var list = this.ActiveSubstancesDict
                .Select(keyValuePair => new ActiveSubstance
                {
                    Name = keyValuePair.Key,
                    Concentration = keyValuePair.Value,
                })
                .ToList();

            ActiveSubstancesList.ItemsSource = list;
        }

        private void AddSubstance_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateAddItemSubstance())
            {
                return;
            }

            string name = SubstanceNameBox.Text;
            float concentration = float.Parse(ConcentrationBox.Text);

            this.ActiveSubstancesDict[name] = concentration;

            this.RefreshActiveSubstancesList();

            SubstanceNameBox.Text = string.Empty;
            ConcentrationBox.Text = string.Empty;
            this.ResetActiveSubstanceErrors();
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

            if (this.ActiveSubstancesDict.ContainsKey(SubstanceNameBox.Text))
            {
                SubstanceNameBox.Background = new SolidColorBrush(Colors.LightPink);
                SubstanceNameBox.Text = string.Empty;
                AddActiveSubstanceToItemInvalidError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!this.ViewModel.SubstanceExists(SubstanceNameBox.Text))
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
                Substance substance = this.ViewModel.GetSubstanceByName(SubstanceNameBox.Text);

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

            if (this.ActiveSubstancesDict.ContainsKey(name))
            {
                this.ActiveSubstancesDict.Remove(name);
            }
            else
            {
                RemoveUpdateActiveSubstanceError.Visibility = Visibility.Visible;
                return;
            }

            this.RefreshActiveSubstancesListUpdate();

            SubstanceNameBox.Text = string.Empty;
            ConcentrationBox.Text = string.Empty;
            this.ResetActiveSubstanceErrors();
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
            var list = this.BatchesDict
                .Select(keyValuePair => new BatchItem
                {
                    Date = keyValuePair.Key,
                    Packs = keyValuePair.Value,
                })
                .OrderBy(item => item.Date)
                .ToList();

            BatchesList.ItemsSource = list;
        }

        private void AddBatch_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateAddBatch())
            {
                return;
            }

            int packs = int.Parse(PacksBox.Text);
            DateOnly date = DateOnly.FromDateTime(BatchDatePicker.Date.Date);

            this.BatchesDict[date] = packs;

            this.RefreshBatchesList();

            PacksBox.Text = string.Empty;
            this.ResetAddBatchErrors();
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
                if (this.BatchesDict.ContainsKey(selectedBatch.Date))
                {
                    this.BatchesDict.Remove(selectedBatch.Date);

                    this.RefreshBatchesList();
                }
            }
            else
            {
                RemoveBatchFromItemError.Visibility = Visibility.Visible;
            }
        }

        private void OnItemCancelClick(object sender, RoutedEventArgs e)
        {
            this.ClearItemAddBoxes();
            this.ClearItemUpdateBoxes();
            this.ActiveSubstancesDict.Clear();
            this.BatchesDict.Clear();
            this.RefreshActiveSubstancesList();
            this.RefreshActiveSubstancesListUpdate();
            this.RefreshBatchesList();
            this.RefreshBatchesListUpdate();
            this.isGetItemDataClicked = false;

            this.ResetAddItemErrors();
            this.ResetUpdateItemErrors();
            this.ResetActiveSubstanceErrors();
            this.ResetAddBatchErrors();

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

            int itemId = selectedItem.Id;

            this.ViewModel.RemoveItemById(itemId);
            this.ViewModel.RefreshItems();
            ItemList.ItemsSource = this.ViewModel.Items;
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
                this.ResetUpdateItemErrors();
            }
        }

        private void OnGetItemDataClick(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateGetItemData())
            {
                return;
            }

            Item item;
            try
            {
                item = this.ViewModel.GetItemById(int.Parse(IdBox.Text));
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

            this.ActiveSubstancesDict = item.ActiveSubstances;
            this.RefreshActiveSubstancesListUpdate();
            this.BatchesDict = item.Batches;
            this.RefreshBatchesListUpdate();

            this.ResetUpdateItemErrors();
            this.isGetItemDataClicked = true;
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
                if (this.ViewModel.GetItemById(id) is null)
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
            if (!this.ValidateUpdateItem())
            {
                return;
            }

            int itemId = int.Parse(IdBox.Text);

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

            for (int i = 0; i < this.BatchesDict.Count; i++)
            {
                quantity += this.BatchesDict.ElementAt(i).Value;
            }

            this.ViewModel.UpdateItemById(itemId, new Item(name, producer, category, price, numberOfPills, this.ActiveSubstancesDict, this.BatchesDict, quantity, label, description, imagePath, discount));

            this.ViewModel.RefreshItems();
            ItemList.ItemsSource = this.ViewModel.Items;
            this.ClearItemUpdateBoxes();
            this.ActiveSubstancesDict.Clear();
            this.RefreshActiveSubstancesListUpdate();
            this.BatchesDict.Clear();
            this.RefreshBatchesListUpdate();
            this.ResetUpdateItemErrors();
        }

        private bool ValidateUpdateItem()
        {
            bool isValid = true;

            if (!this.isGetItemDataClicked)
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

            if (this.ActiveSubstancesDict.Count == 0)
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
            if (this.ViewModel.AddSubstanceGridVisibility == Visibility.Visible)
            {
                this.ViewModel.AddSubstanceGridVisibility = Visibility.Collapsed;
            }
            else
            {
                this.ViewModel.AddSubstanceGridVisibility = Visibility.Visible;
                this.ViewModel.UpdateSubstanceGridVisibility = Visibility.Collapsed;
                this.ResetSubstanceErrors();
            }

            this.ApplyUiStateFromViewModel();
        }

        private void OnSubstanceUpdateClick(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.UpdateSubstanceGridVisibility == Visibility.Visible)
            {
                this.ViewModel.UpdateSubstanceGridVisibility = Visibility.Collapsed;
            }
            else
            {
                this.ViewModel.UpdateSubstanceGridVisibility = Visibility.Visible;
                this.ViewModel.AddSubstanceGridVisibility = Visibility.Collapsed;
                this.ResetSubstanceErrors();
            }

            this.ApplyUiStateFromViewModel();
        }

        private void RefreshActiveSubstancesListUpdate()
        {
            var list = this.ActiveSubstancesDict
                .Select(keyValuePair => new ActiveSubstance
                {
                    Name = keyValuePair.Key,
                    Concentration = keyValuePair.Value,
                })
                .ToList();

            ActiveSubstancesListUpdate.ItemsSource = list;
        }

        private void AddSubstanceUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateUpdateItemSubstance())
            {
                return;
            }

            string name = SubstanceNameBoxUpdate.Text;
            float concentration = float.Parse(ConcentrationBoxUpdate.Text);

            this.ActiveSubstancesDict[name] = concentration;

            this.RefreshActiveSubstancesListUpdate();

            SubstanceNameBoxUpdate.Text = string.Empty;
            ConcentrationBoxUpdate.Text = string.Empty;
            this.ResetUpdateItemErrors();
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

            if (this.ActiveSubstancesDict.ContainsKey(SubstanceNameBoxUpdate.Text))
            {
                SubstanceNameBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                SubstanceNameBoxUpdate.Text = string.Empty;
                UpdateActiveSubstanceInvalidError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!this.ViewModel.SubstanceExists(SubstanceNameBoxUpdate.Text))
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
                Substance substance = this.ViewModel.GetSubstanceByName(SubstanceNameBoxUpdate.Text);

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

            if (this.ActiveSubstancesDict.ContainsKey(name))
            {
                this.ActiveSubstancesDict.Remove(name);
            }
            else
            {
                RemoveUpdateActiveSubstanceError.Visibility = Visibility.Visible;
                return;
            }

            this.RefreshActiveSubstancesListUpdate();

            SubstanceNameBoxUpdate.Text = string.Empty;
            ConcentrationBoxUpdate.Text = string.Empty;
            this.ResetUpdateItemErrors();
        }

        private void RefreshBatchesListUpdate()
        {
            var list = this.BatchesDict
                .Select(keyValuePair => new BatchItem
                {
                    Date = keyValuePair.Key,
                    Packs = keyValuePair.Value,
                })
                .OrderBy(item => item.Date)
                .ToList();

            BatchesListUpdate.ItemsSource = list;
        }

        private void AddBatchUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateUpdateBatch())
            {
                return;
            }

            int packs = int.Parse(PacksBoxUpdate.Text);
            DateOnly date = DateOnly.FromDateTime(BatchDatePickerUpdate.Date.Date);

            this.BatchesDict[date] = packs;

            this.RefreshBatchesListUpdate();

            PacksBoxUpdate.Text = string.Empty;
            this.ResetUpdateItemErrors();
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
                if (this.BatchesDict.ContainsKey(selectedBatch.Date))
                {
                    this.BatchesDict.Remove(selectedBatch.Date);

                    this.RefreshBatchesListUpdate();
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

            this.ViewModel.RemoveSubstanceByName(selectedItem);
            this.ViewModel.RefreshSubstances();
            SubstanceList.ItemsSource = this.ViewModel.Substances;
            this.ResetSubstanceErrors();
        }

        private void OnSubstanceCancelClick(object sender, RoutedEventArgs e)
        {
            this.ClearSubstanceBoxes();
            this.ClearSubstanceUpdateBoxes();
            this.ViewModel.AddSubstanceGridVisibility = Visibility.Collapsed;
            this.ViewModel.UpdateSubstanceGridVisibility = Visibility.Collapsed;
            this.ApplyUiStateFromViewModel();
            this.ResetSubstanceErrors();
        }

        private void OnAddSubstanceClick(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateAddSubstance())
            {
                return;
            }

            string name = NameBoxSubstance.Text;
            int lethalDose = int.Parse(LethalDoseBoxSubstance.Text);
            string description = DescriptionBoxSubstance.Text;

            Substance newSubstance = new Substance(name, lethalDose, description);

            try
            {
                this.ViewModel.AddSubstance(newSubstance);
            }
            catch (ArgumentException exception)
            {
                AddSubstanceFormatError.Text = exception.Message;
                AddSubstanceFormatError.Visibility = Visibility.Visible;
                return;
            }

            this.ClearSubstanceBoxes();
            this.ViewModel.RefreshSubstances();
            SubstanceList.ItemsSource = this.ViewModel.Substances;
            this.ResetSubstanceErrors();
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
            if (!this.ValidateUpdateSubstance())
            {
                return;
            }

            string name = NameBoxSubstanceUpdate.Text;
            int lethalDose = int.Parse(LethalDoseBoxSubstanceUpdate.Text);
            string description = DescriptionBoxSubstanceUpdate.Text;

            Substance updatedSubstance = new Substance(name, lethalDose, description);

            this.ViewModel.UpdateSubstanceByName(name, updatedSubstance);

            System.Diagnostics.Debug.WriteLine("Updated substance");

            this.ClearSubstanceUpdateBoxes();
            this.ViewModel.RefreshSubstances();
            SubstanceList.ItemsSource = this.ViewModel.Substances;
            this.ResetSubstanceErrors();
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

            if (!this.ViewModel.SubstanceExists(NameBoxSubstanceUpdate.Text))
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
