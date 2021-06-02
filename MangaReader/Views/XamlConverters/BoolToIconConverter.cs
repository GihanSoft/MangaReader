// -----------------------------------------------------------------------
// <copyright file="BoolToIconConverter.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

using MahApps.Metro.IconPacks;

namespace AnimePlayer.Views.XamlConverters
{
    [ValueConversion(typeof(object), typeof(PackIconMaterialKind))]
    [SuppressMessage("Performance", "CA1812:BoolToIconConverter is an internal class that is apparently never instantiated. If so, remove the code from the assembly. If this class is intended to contain only static members, make it static (Shared in Visual Basic.", Justification ="WPF object")]
    internal class BoolToIconConverter :
        DependencyObject,
        IValueConverter,
        IList,
        IList<BoolToIconConverterItem>
    {
        private readonly List<BoolToIconConverterItem> items = new();

        public BoolToIconConverterItem this[int index] { get => items[index]; set => items[index] = value; }
        object? IList.this[int index]
        {
            get => items[index];
            set => items[index] = value as BoolToIconConverterItem ??
                throw new InvalidOperationException("item type must be BoolToIconConverterItem");
        }

        public int Count => items.Count;

        public bool IsReadOnly => ((ICollection<BoolToIconConverterItem>)items).IsReadOnly;

        public bool IsSynchronized => ((ICollection)items).IsSynchronized;

        public object SyncRoot => ((ICollection)items).SyncRoot;

        public bool IsFixedSize => ((IList)items).IsFixedSize;

        public void Add(BoolToIconConverterItem item) => items.Add(item);

        public int Add(object? value) => ((IList)items).Add(value);

        public void Clear() => items.Clear();

        public bool Contains(BoolToIconConverterItem item) => items.Contains(item);

        public bool Contains(object? value) => ((IList)items).Contains(value);

        #region IValueConverter
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => items.LastOrDefault(i => i.Value.Equals(value))?.Kind;
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value is not PackIconMaterialKind ? null : (items.LastOrDefault(i => i.Kind == (PackIconMaterialKind)value)?.Value);
        #endregion

        public void CopyTo(BoolToIconConverterItem[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);

        public void CopyTo(Array array, int index) => ((ICollection)items).CopyTo(array, index);

        public IEnumerator<BoolToIconConverterItem> GetEnumerator() => items.GetEnumerator();

        public int IndexOf(BoolToIconConverterItem item) => items.IndexOf(item);

        public int IndexOf(object? value) => ((IList)items).IndexOf(value);

        public void Insert(int index, BoolToIconConverterItem item) => items.Insert(index, item);

        public void Insert(int index, object? value) => ((IList)items).Insert(index, value);

        public bool Remove(BoolToIconConverterItem item) => items.Remove(item);

        public void Remove(object? value) => ((IList)items).Remove(value);

        public void RemoveAt(int index) => items.RemoveAt(index);

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)items).GetEnumerator();
    }
}
