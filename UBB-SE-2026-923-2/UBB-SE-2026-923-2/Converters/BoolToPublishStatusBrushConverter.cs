using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace UBB_SE_2026_923_2.Converters
{
    public sealed class BoolToPublishStatusBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isPublishable = value is bool isPublished && isPublished;
            return new SolidColorBrush(isPublishable ? Colors.Green : Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotImplementedException();
    }
}
