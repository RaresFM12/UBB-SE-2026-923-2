namespace UBB_SE_2026_923_2.Converters
{
    using System;
    using Microsoft.UI;
    using Microsoft.UI.Xaml.Data;
    using Microsoft.UI.Xaml.Media;
    using UBB_SE_2026_923_2.ViewModels.ProductsCatalogue;

    public partial class StockLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is StockLevel level)
            {
                return level switch
                {
                    StockLevel.OutOfStock => new SolidColorBrush(Colors.Red),
                    StockLevel.LowStock => new SolidColorBrush(Colors.Orange),
                    StockLevel.InStock => new SolidColorBrush(Colors.Green),
                    var stock => new SolidColorBrush(Colors.Gray),
                };
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}