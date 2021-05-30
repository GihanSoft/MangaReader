using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using ControlzEx;

using MangaReader.Data.Models;

namespace MangaReader.Views.Components
{
    /// <summary>
    /// Interaction logic for MangaItem.xaml
    /// </summary>
    public partial class MangaItem
    {
        /// <summary>Identifies the <see cref="IsCheckActive"/> dependency property.</summary>
        public static readonly DependencyProperty IsCheckActiveProperty = DependencyProperty.Register(
            nameof(IsCheckActive),
            typeof(bool),
            typeof(MangaItem),
            new PropertyMetadata(default(bool), (d, _) =>
                d.SetCurrentValue(IsCheckedProperty, false)));

        /// <summary>Identifies the <see cref="IsChecked"/> dependency property.</summary>
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            nameof(IsChecked),
            typeof(bool),
            typeof(MangaItem),
            new PropertyMetadata(default(bool)));

        /// <summary>Identifies the <see cref="Manga"/> dependency property.</summary>
        public static readonly DependencyProperty MangaProperty = DependencyProperty.Register(
            nameof(Manga),
            typeof(Manga),
            typeof(MangaItem),
            new PropertyMetadata(default(Manga?), (d, e) =>
            {
                if (d is not MangaItem mangaItem || e.NewValue is not Manga manga)
                {
                    return;
                }
                mangaItem.SetCover(manga.Cover);
            }));

        public event EventHandler<RoutedEventArgs>? Click;

        public bool IsCheckActive
        {
            get => (bool)GetValue(IsCheckActiveProperty);
            set => SetValue(IsCheckActiveProperty, value);
        }

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        private void SetCover(string? cover)
        {
            if (cover is null)
            {
                Cover.SetCurrentValue(Image.SourceProperty, null);
                return;
            }
            BitmapSource imageSource;
            if (cover.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var dataStr = cover.Split(new[] { ',' }, 2)[1];
                var data = Convert.FromBase64String(dataStr);
                MemoryStream? memStreamMain = new(data)
                {
                    Position = 0
                };
                imageSource = BitmapFrame.Create(memStreamMain);
            }
            else
            {
                imageSource = BitmapFrame.Create(new Uri(cover));
            }
            Cover.SetCurrentValue(Image.SourceProperty, imageSource);
        }

        public bool IsEditActive
        {
            get => EditBorder.Visibility != Visibility.Collapsed;
            set => EditBorder.SetValue(VisibilityProperty, value ? Visibility.Visible : Visibility.Collapsed);
        }

        public Manga? Manga
        {
            get => (Manga?)GetValue(MangaProperty);
            set => SetValue(MangaProperty, value);
        }

        public MangaItem()
        {
            InitializeComponent();
        }

        public void WorkingFocus()
        {
            KeyboardNavigationEx.Focus(Btn);
        }

        private void EditNameBtn_Click(object sender, RoutedEventArgs e)
        {
            NameEntryBorder.SetCurrentValue(VisibilityProperty, Visibility.Visible);
            NameEntry.SetCurrentValue(TextBox.TextProperty, Manga?.Name);
        }

        private void EditCoverBtn_Click(object sender, RoutedEventArgs e)
        {
            //
        }

        private void NameEditOK_Click(object sender, RoutedEventArgs e)
        {
            //
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsCheckActive)
            {
                SetCurrentValue(IsCheckedProperty, !IsChecked);
            }
            else
            {
                Click?.Invoke(this, e);
            }
        }
    }
}
