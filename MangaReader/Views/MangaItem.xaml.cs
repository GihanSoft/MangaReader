using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MangaReader.Views
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
                if (!string.IsNullOrEmpty(value) && System.IO.File.Exists(value))
                {
                    coverSource = value;
                    Cover.Source = new BitmapImage(new Uri(value, UriKind.RelativeOrAbsolute));
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

        public Models.MangaInfo Manga { get; set; }

        public MangaItem()
        {
            InitializeComponent();
        }

        public MangaItem(Models.MangaInfo manga) : this()
        {
            MangaTitle = manga.Name;

            if (manga.CoverAddress == null)
            {
                try
                {
                    var chapters = System.IO.Directory.EnumerateDirectories(manga.Address).ToList();
                    chapters.Sort(NaturalStringComparer.Default.Compare);
                    var pages = System.IO.Directory.EnumerateFiles(chapters[0], "*.*", System.IO.SearchOption.AllDirectories).ToList();
                    pages.Sort(NaturalStringComparer.Default.Compare);
                    CoverSource = pages.Find(file =>
                        FileTypeList.ImageTypes.Any(t => file.ToLower().EndsWith(t)));

                    SettingApi.This.MangaList[manga.ID].CoverAddress = CoverSource;
                }
                catch { }
            }
            else
                CoverSource = manga.CoverAddress;
            Manga = manga;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Click(this, e);
        }

        private void EditNameBtn_Click(object sender, RoutedEventArgs e)
        {
            NameEntryBorder.Visibility = Visibility.Visible;
            NameEntry.Text = Manga.Name;
        }

        private void EditCoverBtn_Click(object sender, RoutedEventArgs e)
        {
            string filter = "Image files (";
            foreach (string format in FileTypeList.ImageTypes)
            {
                filter += $"*.{format}, ";
            }
            filter = filter.Substring(0, filter.Length - 2);
            filter += ")|";
            foreach (string format in FileTypeList.ImageTypes)
            {
                filter += $"*.{format};";
            }
            filter = filter.Substring(0, filter.Length - 1);

            var fileChooser = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = filter,
                InitialDirectory = Manga.Address
            };
            var r = fileChooser.ShowDialog();
            if (r == true)
                SettingApi.This.MangaList[Manga.ID].CoverAddress = CoverSource = Manga.CoverAddress = fileChooser.FileName;
        }

        private void NameEditOK_Click(object sender, RoutedEventArgs e)
        {
            SettingApi.This.MangaList[Manga.ID].Name = MangaTitle = Manga.Name = NameEntry.Text;
            NameEntryBorder.Visibility = Visibility.Collapsed;
        }
    }
}
