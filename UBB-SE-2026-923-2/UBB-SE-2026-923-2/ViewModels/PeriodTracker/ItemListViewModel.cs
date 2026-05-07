namespace UBB_SE_2026_923_2.ViewModels.PeriodTracker
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ItemListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        private ObservableCollection<ItemViewModel> items;

        public ObservableCollection<ItemViewModel> Items
        {
            get => this.items;
            set
            {
                if (this.items == value)
                {
                    return;
                }

                this.items = value;
                this.OnPropertyChanged();
            }
        }

        public ItemListViewModel()
        {
            this.Items = new ObservableCollection<ItemViewModel>();
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}