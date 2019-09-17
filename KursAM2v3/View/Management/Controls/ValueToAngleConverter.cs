using System;
using System.Globalization;
using System.Windows.Data;
using static System.Math;

namespace KursAM2.View.Management.Controls
{
    [ValueConversion(typeof(double), typeof(double))]
    public class ValueToAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ReSharper disable once PossibleNullReferenceException
            return (double) value * 0.01 * 360;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ReSharper disable once PossibleNullReferenceException
            return Floor((double) value / 360 * 100);
        }
    }
}