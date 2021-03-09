using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MangaReader.Views.XamlConverters
{
    [ValueConversion(typeof(double), typeof(double))]
    public class ZaribConverter : DependencyObject, IValueConverter
    {
        /// <summary>Identifies the <see cref="Zarib"/> dependency property.</summary>
        public static readonly DependencyProperty ZaribProperty = DependencyProperty.Register(
            nameof(Zarib),
            typeof(double),
            typeof(ZaribConverter),
            new PropertyMetadata(1d));

        public double Zarib
        {
            get => (double)GetValue(ZaribProperty);
            set => SetValue(ZaribProperty, value);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Math.Round((double)value * Zarib, 2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Math.Round((double)value / Zarib, 2);
        }
    }
}
