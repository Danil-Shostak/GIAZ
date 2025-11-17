using Microsoft.UI.Xaml.Data;
using System;

namespace EducationInstitutionsRB.Converters;

public class BooleanToYesNoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "Да" : "Нет";
        }
        return "Нет";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is string stringValue)
        {
            return stringValue == "Да";
        }
        return false;
    }
}