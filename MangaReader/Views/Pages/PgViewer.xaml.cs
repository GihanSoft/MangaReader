using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Controls.Primitives;

using ControlzEx;

using GihanSoft.MangaSources.Local;

using MangaReader.Controllers;
using MangaReader.Data;
using MangaReader.Data.Models;
using MangaReader.PagesViewer;
using MangaReader.Views.Components;
using System.Threading.Tasks;
using System.Windows.Input;

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

                // Components.PagesViewer.GetPagesViewer(ViewMode.PageSingle);
                //viewer.IsTabStop = true;
                //viewer.Focusable = true;
                while (pgViewer.PagesViewer is null)
                {
                    Task.Delay(10);
                }
                pgViewer.PagesViewer.View(pgViewer.currentPagesProvider, 0);
                KeyboardNavigationEx.Focus(pgViewer.PagesViewer);
            }));

        /// <summary>Identifies the <see cref="PagesViewer"/> dependency property.</summary>
        public static readonly DependencyPropertyKey PagesViewerPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(PagesViewer),
            typeof(Components.PagesViewer),
            typeof(PgViewer),
            new PropertyMetadata(default(Components.PagesViewer)));

        /// <summary>Identifies the <see cref="PagesViewer"/> dependency property.</summary>
        public static readonly DependencyProperty PagesViewerProperty = PagesViewerPropertyKey.DependencyProperty;

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
            SetValue(PagesViewerPropertyKey, new PageSingle());
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
            TxtPage.SetBinding(TextBox.TextProperty, new Binding
            {
                Source = PagesViewer,
                Path = new PropertyPath(nameof(PagesViewer.Page), null),
                Mode = BindingMode.TwoWay,
            });
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

        public Components.PagesViewer? PagesViewer
        {
            get => (Components.PagesViewer?)GetValue(PagesViewerProperty);
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

        private void CmdNextChapter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CboChapters.SetCurrentValue(Selector.SelectedIndexProperty, CboChapters.SelectedIndex + 1);
        }
    }
}
