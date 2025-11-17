using Microsoft.UI.Xaml.Data;
using System;

namespace EducationInstitutionsRB.Converters;

public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Enum enumValue)
        {
            return ConvertEnumToString(enumValue);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    private string ConvertEnumToString(Enum value)
    {
        return value switch
        {
            ReportType.ByRegion => "По регионам",
            ReportType.ByDistrict => "По районам",
            ReportType.ByType => "По типам учреждений",
            ReportType.Statistics => "Статистика",
            ReportType.Infrastructure => "Инфраструктура",
            ReportType.Financial => "Финансовые показатели",
            ReportScale.Country => "Страна",
            ReportScale.Region => "Область",
            ReportScale.District => "Район",
            _ => value.ToString()
        };
    }
}

// Временные enum для конвертера
public enum ReportType
{
    ByRegion,
    ByDistrict,
    ByType,
    Statistics,
    Infrastructure,
    Financial
}

public enum ReportScale
{
    Country,
    Region,
    District
}