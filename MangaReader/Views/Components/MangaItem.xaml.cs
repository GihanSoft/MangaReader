using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using MangaReader.Data.Models;

namespace MangaReader.Views.Components
{
    /// <summary>
    /// Interaction logic for MangaItem.xaml
    /// </summary>
    public partial class MangaItem : UserControl
    {
        public event RoutedEventHandler Click;

        public bool IsCheckActive
        {
            get
            {
                if (CheckerBorder.Visibility == Visibility.Collapsed)
                    return false;
                else
                    return true;
            }
            set
            {
                if (value == true)
                    CheckerBorder.Visibility = Visibility.Visible;
                else
                    CheckerBorder.Visibility = Visibility.Collapsed;
            }
        }
        public bool? IsChecked
        {
            get { return Checker.IsChecked; }
            set { Checker.IsChecked = value; }
        }

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

        private string coverSource;
        public string CoverSource
        {
            get { return coverSource; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    coverSource = value;
                    BitmapImage bitmap;
                    if (value.StartsWith("data:"))
                    {
                        var dataStr = value.Split(new[] { ',' }, 2)[1];
                        var data = Convert.FromBase64String(dataStr);
                        MemoryStream memStream = new(data)
                        {
                            Position = 0
                        };
                        bitmap = new();
                        bitmap.BeginInit();
                        bitmap.StreamSource = memStream;
                        bitmap.EndInit();
                    }
                    else
                    {
                        bitmap = new(new Uri(value));
                    }
                    Cover.Source = bitmap;
                }
            }
        }

        public Button MangaButton
        {
            get
            {
                return SelectButton;
            }
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
            CoverSource = manga.Cover;

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
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
    }
}
