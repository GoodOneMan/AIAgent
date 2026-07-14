using System;
using System.Windows.Data;
using System.Windows.Media;

namespace AIAgent.UI
{
    public class UserBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
            (value is bool isUser && isUser) ? new SolidColorBrush(Colors.LightBlue) : new SolidColorBrush(Colors.LightGray);
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
            throw new NotImplementedException();
    }
}