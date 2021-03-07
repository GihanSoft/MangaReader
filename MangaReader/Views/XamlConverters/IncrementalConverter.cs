using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MangaReader.Views.XamlConverters
{
    [ValueConversion(typeof(double), typeof(double))]
    public class IncrementalConverter : DependencyObject, IValueConverter
    {
        /// <summary>Identifies the <see cref="Increment"/> dependency property.</summary>
        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(
            nameof(Increment),
            typeof(double),
            typeof(IncrementalConverter),
            new PropertyMetadata(1d));

        public double Increment
        {
            get => (double)GetValue(IncrementProperty);
            set => SetValue(IncrementProperty, value);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value, culture) + Increment;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value, culture) - Increment;
        }
    }
}
