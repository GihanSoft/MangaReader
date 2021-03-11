using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

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
            {
                d.SetCurrentValue(IsCheckedProperty, false);
            }));

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
                Cover.Source = null;
                return;
            }
            Bitmap bitmap;
            //NetVips.Image image;
            if (cover.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var dataStr = cover.Split(new[] { ',' }, 2)[1];
                var data = Convert.FromBase64String(dataStr);
                MemoryStream? memStreamMain = new(data)
                {
                    Position = 0
                };
                bitmap = new Bitmap(memStreamMain);
                //image = NetVips.Image.NewFromBuffer(data);
            }
            else
            {
                bitmap = new Bitmap(cover);
                //image = NetVips.Image.NewFromFile(cover);
            }
            cover = null;
            var nesbat = bitmap.Height / (double)bitmap.Width;
            var thumbImage = bitmap.GetThumbnailImage(
                (int)Width * 3,
                (int)(Width * 3 * nesbat),
                null,
                IntPtr.Zero);
            //NetVips.Image thumbImage = image.ThumbnailImage((int)MaxWidth * 3);

            bitmap.Dispose();
            //image.Dispose();
            MemoryStream memoryStream = new();
            thumbImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            //thumbImage.PngsaveStream(memoryStream, 0);
            thumbImage.Dispose();
            memoryStream.Position = 0;
            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            Cover.Source = bitmapImage;
            GC.Collect();
        }

        public bool IsEditActive
        {
            get
            {
                if (EditBorder.Visibility == Visibility.Collapsed)
                    return false;
                else
                    return true;
            }
            set
            {
                if (value == true)
                    EditBorder.Visibility = Visibility.Visible;
                else
                    EditBorder.Visibility = Visibility.Collapsed;
            }
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

        //public MangaItem(Manga manga) : this()
        //{
        //    MangaTitle = manga.Name;
        //    SetCoverSource(manga.Cover);
        //    Manga = manga;
        //}

        private void EditNameBtn_Click(object sender, RoutedEventArgs e)
        {
            NameEntryBorder.Visibility = Visibility.Visible;
            NameEntry.Text = Manga.Name;
        }

        private void EditCoverBtn_Click(object sender, RoutedEventArgs e)
        {
            //string filter = "Image files (";
            //foreach (string format in FileTypeList.ImageTypes)
            //{
            //    filter += $"*.{format}, ";
            //}
            //filter = filter.Substring(0, filter.Length - 2);
            //filter += ")|";
            //foreach (string format in FileTypeList.ImageTypes)
            //{
            //    filter += $"*.{format};";
            //}
            //filter = filter.Substring(0, filter.Length - 1);

            //var fileChooser = new Microsoft.Win32.OpenFileDialog()
            //{
            //    Filter = filter,
            //    InitialDirectory = Manga.Address
            //};
            //var r = fileChooser.ShowDialog();
            //if (r == true)
            //    SettingApi.This.MangaList[Manga.Id].CoverAddress = CoverSource = Manga.CoverAddress = fileChooser.FileName;
        }

        private void NameEditOK_Click(object sender, RoutedEventArgs e)
        {
            //SettingApi.This.MangaList[Manga.Id].Name = MangaTitle = Manga.Name = NameEntry.Text;
            //NameEntryBorder.Visibility = Visibility.Collapsed;
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
