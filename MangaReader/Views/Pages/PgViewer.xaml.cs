using Gihan.Manga.Views.Custom;

using GihanSoft.MangaSources.Local;
using GihanSoft.Navigation;

using MangaReader.Controllers;
using MangaReader.Data;
using MangaReader.Data.Models;

using OtakuLib.MangaBase;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace MangaReader.Views.Pages
{
    /// <summary>
    /// Interaction logic for PgViewer.xaml
    /// </summary>
    public partial class PgViewer
    {
        private readonly DataDb dataDb;

        public PgViewer(
            PageNavigator navigator,
            DataDb dataDb) : base(navigator)
        {
            InitializeComponent();
            this.dataDb = dataDb;
        }

        private IEnumerable<FileSystemInfo> GetChapterList(Manga manga)
        {
            var dir = new DirectoryInfo(manga.path);
            var chapters = dir.EnumerateFileSystemInfos()
                .Where(item => item is not FileInfo file ||
                FileTypeList.CompressedType.Any(t => file.Name.EndsWith(t)));
            chapters = chapters.OrderBy(item => item is FileInfo file ?
                Path.GetFileNameWithoutExtension(file.Name) : item.Name,
                    NaturalStringComparer.Default);
            return chapters;
        }

        public void View(string path)
        {
            if (char.IsNumber(path[0]))
            {
                var manga = dataDb.Mangas.FindById(int.Parse(path));
                //var win = new WinMain(manga);
                //win.MetroWindow_Loaded(null, null);
                ////win.Show();
                //var content = win.Content;
                //win.Content = null;
                //win.Close();
                //Content = content;
                var chapters = GetChapterList(manga).ToArray();
                FileSystemInfo chapter = chapters[manga.CurrentChapter];

                PagesProvider pagesProvider;
                if (chapter is DirectoryInfo directory)
                {
                    var x = directory.GetFiles("*", SearchOption.AllDirectories);
                    if (x.Any(x => FileTypeList.CompressedType.Contains(x.Extension, StringComparer.InvariantCultureIgnoreCase)))
                    {
                        pagesProvider = new CompressedPageProvider(x.First(x => FileTypeList.CompressedType.Contains(x.Extension, StringComparer.InvariantCultureIgnoreCase)).FullName);
                    }
                    else
                    {
                        pagesProvider = new LocalPagesProvider(chapter.FullName);
                    }
                }
                else
                {
                    pagesProvider = new CompressedPageProvider(chapter.FullName);
                }
                (LeftToolBar.Children[1] as ComboBox).ItemsSource = chapters;
                (LeftToolBar.Children[1] as ComboBox).SelectedIndex = manga.CurrentChapter;
                (LeftToolBar.Children[3] as TextBox).Text = manga.CurrentPage.ToString();
                (LeftToolBar.Children[4] as TextBlock).Text = pagesProvider.Count.ToString();
                (LeftToolBar.Children[6] as TextBox).Text = (manga.Zoom * 100).ToString();

                var viewer = PagesViewerFactory.GetPagesViewer(Gihan.Manga.ViewMode.PageSingle);
                viewer.IsTabStop = true;
                viewer.Focusable = true;
                viewer.SetSourceStreams(pagesProvider, 1);
                Content = viewer;
                viewer.Focus();
            }
        }

        public override StackPanel? LeftToolBar => (StackPanel)Resources["LeftToolBar"];

        protected override void Dispose(bool disposing)
        {
        }

        private void HomeMenu_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void Previous_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void ChapterListCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ZoomInBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void ZoomOutBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void ZoomPersent_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void FullScreenBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void Next_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void CmdNextPage_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            (Content as PagesViewer).Page++;
        }
    }
}
