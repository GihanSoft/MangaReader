using System.Windows;
using ControlzEx;

using GihanSoft.MangaSources.Local;
using GihanSoft.Navigation;

using MangaReader.Controllers;
using MangaReader.Data;
using MangaReader.Data.Models;
using MangaReader.PagesViewer;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Controls.Primitives;

namespace MangaReader.Views.Pages
{
    /// <summary>
    /// Interaction logic for PgViewer.xaml
    /// </summary>
    [CLSCompliant(false)]
    public partial class PgViewer
    {
        /// <summary>Identifies the <see cref="CurrentChapter"/> dependency property.</summary>
        public static readonly DependencyProperty CurrentChapterProperty = DependencyProperty.Register(
            nameof(CurrentChapter),
            typeof(FileSystemInfo),
            typeof(PgViewer),
            new PropertyMetadata(default(FileSystemInfo), (d, e) =>
            {
                if (d is not PgViewer pgViewer || e.NewValue is not FileSystemInfo currentChapter)
                {
                    return;
                }
                //TODO: chane chapter
                if (pgViewer.currentPagesProvider is not null)
                {
                    pgViewer.currentPagesProvider.Dispose();
                }
                if (currentChapter is DirectoryInfo directory)
                {
                    FileInfo[] files = directory.GetFiles("*", SearchOption.AllDirectories);
                    if (files.Any(file => FileTypeList.CompressedType.Contains(file.Extension, StringComparer.OrdinalIgnoreCase)))
                    {
                        pgViewer.currentPagesProvider = new CompressedPageProvider(files.First(x => FileTypeList.CompressedType.Contains(x.Extension, StringComparer.InvariantCultureIgnoreCase)).FullName);
                    }
                    else
                    {
                        pgViewer.currentPagesProvider = new LocalPagesProvider(currentChapter.FullName);
                    }
                }
                else
                {
                    pgViewer.currentPagesProvider = new CompressedPageProvider(currentChapter.FullName);
                }
                //PagesCount.Text = pagesProvider.Count.ToString("/#", CultureInfo.InvariantCulture);
                //ZoomPersent.Text = (manga.Zoom * 100).ToString();

                Components.PagesViewer viewer = new Components.PageSingle();
                viewer.SetBinding(PageProperty, new Binding()
                {
                    Source = pgViewer,
                    Path = new(nameof(Page), null),
                    Mode = BindingMode.TwoWay
                });
                // Components.PagesViewer.GetPagesViewer(ViewMode.PageSingle);
                //viewer.IsTabStop = true;
                //viewer.Focusable = true;
                viewer.View(pgViewer.currentPagesProvider, 0);
                pgViewer.Content = viewer;
                KeyboardNavigationEx.Focus(viewer);
            }));

        /// <summary>Identifies the <see cref="Page"/> dependency property.</summary>
        public static readonly DependencyProperty PageProperty = DependencyProperty.Register(
            nameof(Page),
            typeof(int),
            typeof(PgViewer),
            new PropertyMetadata(default(int), (d, e) =>
            {
            }));

        private static readonly DependencyPropertyKey ChaptersPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Chapters),
            typeof(ObservableCollection<FileSystemInfo>),
            typeof(PgViewer),
            new PropertyMetadata(default(ObservableCollection<FileSystemInfo>)));
        /// <summary>Identifies the <see cref="Chapters"/> dependency property.</summary>
        public static readonly DependencyProperty ChaptersProperty = ChaptersPropertyKey.DependencyProperty;

        private static IEnumerable<FileSystemInfo> GetChapterList(Manga manga)
        {
            if (manga.Path is null)
            {
                return Enumerable.Empty<FileSystemInfo>();
            }

            DirectoryInfo dir = new(manga.Path);

            return dir.EnumerateDirectories().Cast<FileSystemInfo>()
                    .Concat(dir.EnumerateFiles().Where(f => FileTypeList.CompressedType.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase)))
                    .OrderBy(f => f.Name, NaturalStringComparer.Default);
        }

        private PagesProvider? currentPagesProvider;
        private readonly DataDb dataDb;

        public PgViewer(DataDb dataDb)
        {
            this.dataDb = dataDb;

            InitializeComponent();
            SetValue(ChaptersPropertyKey, new ObservableCollection<FileSystemInfo>());
            CboChapters.SetBinding(ItemsControl.ItemsSourceProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath(nameof(Chapters), null)
            });
            CboChapters.SetBinding(Selector.SelectedItemProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath(nameof(CurrentChapter), null),
                Mode = BindingMode.TwoWay
            });
            //TxtPage.SetBinding(TextBox.TextProperty, new Binding
            //{
            //    Source = this,
            //    Path = new PropertyPath(nameof(Page), null),
            //    Mode = BindingMode.TwoWay
            //});
        }

        public ObservableCollection<FileSystemInfo>? Chapters
        {
            get => (ObservableCollection<FileSystemInfo>?)GetValue(ChaptersProperty);
        }

        public FileSystemInfo? CurrentChapter
        {
            get => (FileSystemInfo?)GetValue(CurrentChapterProperty);
            set => SetValue(CurrentChapterProperty, value);
        }

        public int Page
        {
            get => (int)GetValue(PageProperty);
            set => SetValue(PageProperty, value);
        }

        public void View(string path)
        {
            if (path.StartsWith("manga://", StringComparison.OrdinalIgnoreCase))
            {
                Manga manga = dataDb.Mangas.FindById(int.Parse(path.Split('/').Last(), CultureInfo.InvariantCulture));
                Chapters!.Clear();
                foreach (var chapter in GetChapterList(manga))
                {
                    Chapters.Add(chapter);
                }
                SetCurrentValue(CurrentChapterProperty, Chapters[manga.CurrentChapter]);



            }
        }

        private void ChapterListCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CmdNextPage_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            (Content as Components.PagesViewer).Page++;
        }
    }
}
