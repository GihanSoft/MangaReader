// -----------------------------------------------------------------------
// <copyright file="ZaribConverter.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MangaReader.Views.XamlConverters
{
    /// <summary>
    /// multiple a double by <see cref="Zarib"/>.
    /// </summary>
    [SuppressMessage("WpfAnalyzers.IValueConverter", "WPF0072:ValueConversion must use correct types.", Justification = "its really double to double")]
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

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string valStr)
            {
                value = double.Parse(valStr, culture);
            }

            return Math.Round((double)value * Zarib, 2);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string valStr)
            {
                value = double.Parse(valStr, culture);
            }

            return Math.Round((double)value / Zarib, 2);
        }
    }
}
