using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Windows;
using System.Windows.Controls;
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

        public event EventHandler<RoutedEventArgs> Click;

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
        //public bool? IsChecked
        //{
        //    get { return Checker.IsChecked; }
        //    set { Checker.IsChecked = value; }
        //}

        private string mangaTitle;
        public string MangaTitle
        {
            get { return mangaTitle; }
            set
            {
                mangaTitle = value;
                Title.Text = value;
            }
        }

        public void SetCoverSource(string? cover)
        {
            if (cover is null)
            {
                Cover.Source = null;
                return;
            }
            NetVips.Image image;
            if (cover.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var dataStr = cover.Split(new[] { ',' }, 2)[1];
                var data = Convert.FromBase64String(dataStr);
                image = NetVips.Image.NewFromBuffer(data);
            }
            else
            {
                image = NetVips.Image.NewFromFile(cover);
            }
            cover = null;
            NetVips.Image thumbImage = image.ThumbnailImage((int)MaxWidth * 3);
            image.Dispose();
            MemoryStream memoryStream = new();
            thumbImage.PngsaveStream(memoryStream, 0);
            thumbImage.Dispose();
            memoryStream.Position = 0;
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.StreamSource = memoryStream;
            bitmap.EndInit();
            Cover.Source = bitmap;
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

        public Manga Manga { get; set; }

        public MangaItem()
        {
            InitializeComponent();
        }

        public MangaItem(Manga manga) : this()
        {
            MangaTitle = manga.Name;
            SetCoverSource(manga.Cover);

            //if (manga.CoverAddress == null || !File.Exists(manga.CoverAddress))
            //{
            //    try
            //    {
            //        var chapters = Directory.EnumerateDirectories(manga.Address).ToList();
            //        if (chapters.Count > 0)
            //        {
            //            chapters.Sort(NaturalStringComparer.Default.Compare);
            //            var pages = Directory.EnumerateFiles(chapters[0], "*.*", SearchOption.AllDirectories).ToList();
            //            pages.Sort(NaturalStringComparer.Default.Compare);
            //            CoverSource = pages.Find(file =>
            //                FileTypeList.ImageTypes.Any(t => file.ToLower().EndsWith(t)));
            //        }
            //        else if (Directory.EnumerateFiles(manga.Address).
            //            FirstOrDefault(f => FileTypeList.ImageTypes.Any(t => f.EndsWith(t))) != null)
            //        {
            //            CoverSource = Directory.EnumerateFiles(manga.Address).
            //                FirstOrDefault(f => FileTypeList.ImageTypes.Any(t => f.EndsWith(t)));
            //        }
            //        {
            //            chapters = Directory.EnumerateFiles(manga.Address).
            //                Where(ch => FileTypeList.CompressedType.Any(t => ch.EndsWith(t))).ToList();
            //            chapters.Sort(NaturalStringComparer.Default.Compare);
            //            var exPath = CompressApi.OpenArchive(chapters[0]);
            //            var pages = Directory.EnumerateFiles(exPath, "*.*", SearchOption.AllDirectories).ToList();
            //            pages.Sort(NaturalStringComparer.Default.Compare);
            //            CoverSource = pages.Find(file =>
            //                FileTypeList.ImageTypes.Any(t => file.ToLower().EndsWith(t)));
            //        }
            //        SettingApi.This.MangaList[manga.Id].CoverAddress = CoverSource;
            //        CoverMaker.CoverConvert(manga);
            //        CoverSource = SettingApi.This.MangaList[manga.Id].CoverAddress;
            //        CompressApi.CleanExtractPath();
            //    }
            //    catch (Exception err)
            //    {
            //        var x = err.Message;
            //    }
            //}
            //else
            //{
            //    if (!manga.CoverAddress.StartsWith(CoverMaker.AbsoluteCoversPath))
            //        CoverMaker.CoverConvert(manga);
            //    CoverSource = manga.CoverAddress;
            //}
            Manga = manga;
        }

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
