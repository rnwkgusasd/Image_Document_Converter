using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace IDC.Converter;

public class BoolToGridLengthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isVisible = (bool)value;
        if (!isVisible) return new GridLength(0); // 숨겨질 때 너비 0

        string paramStr = parameter?.ToString() ?? "*";
        if (paramStr == "*")
        {
            return new GridLength(1, GridUnitType.Star);
        }
        else if (double.TryParse(paramStr, out double val))
        {
            return new GridLength(val);
        }
        return new GridLength(1, GridUnitType.Star);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
