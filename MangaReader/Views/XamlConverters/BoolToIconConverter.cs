using MahApps.Metro.IconPacks;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace AnimePlayer.Views.XamlConverters
{
    public class BoolToIconConverter : IValueConverter,
        ICollection<BoolToIconConverterItem>, ICollection,
        IEnumerable<BoolToIconConverterItem>, IEnumerable,
        IList<BoolToIconConverterItem>, IList
    {
        private readonly List<BoolToIconConverterItem> items = new();

        public BoolToIconConverterItem this[int index] { get => ((IList<BoolToIconConverterItem>)items)[index]; set => ((IList<BoolToIconConverterItem>)items)[index] = value; }
        object IList.this[int index] { get => ((IList)items)[index]; set => ((IList)items)[index] = value; }

        public int Count => ((ICollection<BoolToIconConverterItem>)items).Count;

        public bool IsReadOnly => ((ICollection<BoolToIconConverterItem>)items).IsReadOnly;

        public bool IsSynchronized => ((ICollection)items).IsSynchronized;

        public object SyncRoot => ((ICollection)items).SyncRoot;

        public bool IsFixedSize => ((IList)items).IsFixedSize;

        public void Add(BoolToIconConverterItem item)
        {
            ((ICollection<BoolToIconConverterItem>)items).Add(item);
        }

        public int Add(object value)
        {
            return ((IList)items).Add(value);
        }

        public void Clear()
        {
            ((ICollection<BoolToIconConverterItem>)items).Clear();
        }

        public bool Contains(BoolToIconConverterItem item)
        {
            return ((ICollection<BoolToIconConverterItem>)items).Contains(item);
        }

        public bool Contains(object value)
        {
            return ((IList)items).Contains(value);
        }

        #region IValueConverter
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return items.LastOrDefault(i => i.Value.Equals(value))?.Kind;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not PackIconMaterialKind) return null;
            return items.LastOrDefault(i => i.Kind == (PackIconMaterialKind)value)?.Value;
        }
        #endregion

        public void CopyTo(BoolToIconConverterItem[] array, int arrayIndex)
        {
            ((ICollection<BoolToIconConverterItem>)items).CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)items).CopyTo(array, index);
        }

        public IEnumerator<BoolToIconConverterItem> GetEnumerator()
        {
            return ((IEnumerable<BoolToIconConverterItem>)items).GetEnumerator();
        }

        public int IndexOf(BoolToIconConverterItem item)
        {
            return ((IList<BoolToIconConverterItem>)items).IndexOf(item);
        }

        public int IndexOf(object value)
        {
            return ((IList)items).IndexOf(value);
        }

        public void Insert(int index, BoolToIconConverterItem item)
        {
            ((IList<BoolToIconConverterItem>)items).Insert(index, item);
        }

        public void Insert(int index, object value)
        {
            ((IList)items).Insert(index, value);
        }

        public bool Remove(BoolToIconConverterItem item)
        {
            return ((ICollection<BoolToIconConverterItem>)items).Remove(item);
        }

        public void Remove(object value)
        {
            ((IList)items).Remove(value);
        }

        public void RemoveAt(int index)
        {
            ((IList<BoolToIconConverterItem>)items).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)items).GetEnumerator();
        }
    }

    public class BoolToIconConverterItem
    {
        public PackIconMaterialKind Kind { get; set; }
        public bool Value { get; set; }
    }
}
