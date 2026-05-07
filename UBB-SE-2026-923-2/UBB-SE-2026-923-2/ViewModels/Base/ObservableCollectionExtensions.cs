namespace UBB_SE_2026_923_2.ViewModels.Base
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public static class ObservableCollectionExtensions
    {
        public static void ReplaceWith<T>(this ObservableCollection<T> collection, IEnumerable<T> source)
        {
            collection.Clear();
            foreach (var item in source)
            {
                collection.Add(item);
            }
        }
    }
}
