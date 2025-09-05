using System.Globalization;
using System.Windows.Data;

namespace StickyTodoApp.Converters;

public class BoolToArchiveTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (bool)value ? "▲ Hide Archived" : "▼ View Archived";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}