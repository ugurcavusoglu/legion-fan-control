using System.Globalization;
using System.Windows.Data;
using LegionFanControl.Models;

namespace LegionFanControl.Converters;

public class ThermalModeToActiveStyleConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is ThermalMode current && parameter is string target
            && Enum.TryParse<ThermalMode>(target, out var targetMode))
        {
            return current == targetMode
                ? System.Windows.Application.Current.Resources["ThermalBtnActive"]!
                : System.Windows.Application.Current.Resources["ThermalBtn"]!;
        }
        return System.Windows.Application.Current.Resources["ThermalBtn"]!;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class TempToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double temp)
        {
            if (temp >= 90) return System.Windows.Media.Brushes.Red;
            if (temp >= 75) return new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0xFF, 0x6D, 0x00));
            return System.Windows.Media.Brushes.White;
        }
        return System.Windows.Media.Brushes.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
