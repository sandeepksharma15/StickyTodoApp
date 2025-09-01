using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using StickyTodoApp.Models;

namespace StickyTodoApp.Converters;

public class PriorityToFontWeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Priority p)
            return p == Priority.Urgent ? FontWeights.Bold : FontWeights.Normal;
        return FontWeights.Normal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
