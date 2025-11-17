    // EducationInstitutionsRB/Converters/StringToDecimalConverter.cs
using Microsoft.UI.Xaml.Data;
using System;

namespace EducationInstitutionsRB.Converters;

public class StringToDecimalConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal decimalValue)
        {
            return decimalValue.ToString();
        }
        return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is string stringValue && decimal.TryParse(stringValue, out decimal result))
        {
            return result;
        }
        return 0m;
    }
}