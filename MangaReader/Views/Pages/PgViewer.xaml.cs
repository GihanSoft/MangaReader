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

using GihanSoft.MangaSources.Local;

using MangaReader.Controllers;
using MangaReader.Data;
using MangaReader.Data.Models;
using MangaReader.PagesViewer;
using MangaReader.Views.Components;
using System.Threading.Tasks;
using System.Windows.Input;
using MangaReader.Views.XamlConverters;
using System.Timers;
using GihanSoft.Navigation;

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
                if (pgViewer.CurrentPagesProvider is not null)
                {
                    pgViewer.CurrentPagesProvider.Dispose();
                }
                if (currentChapter is DirectoryInfo directory)
                {
                    FileInfo[] files = directory.GetFiles("*", SearchOption.AllDirectories);
                    if (files.Any(file => FileTypeList.CompressedType.Contains(file.Extension, StringComparer.OrdinalIgnoreCase)))
                    {
                        pgViewer.CurrentPagesProvider = new CompressedPageProvider(files.First(x => FileTypeList.CompressedType.Contains(x.Extension, StringComparer.InvariantCultureIgnoreCase)).FullName);
                    }
                    else
                    {
                        pgViewer.CurrentPagesProvider = new LocalPagesProvider(currentChapter.FullName);
                    }
                }
                else
                {
                    pgViewer.CurrentPagesProvider = new CompressedPageProvider(currentChapter.FullName);
                }
                pgViewer.TbPagesCount.SetCurrentValue(TextBlock.TextProperty, pgViewer.CurrentPagesProvider.Count.ToString("/#", CultureInfo.InvariantCulture));

                while (pgViewer.PagesViewer is null)
                {
                    Task.Delay(10);
                }
                int page = 0;
                if (e.OldValue is not null)
                {
                    pgViewer.Save();
                }
                else
                {
                    int currentPage = pgViewer.manga!.CurrentPage;
                    page = currentPage < pgViewer.CurrentPagesProvider.Count ?
                        currentPage :
                        pgViewer.CurrentPagesProvider.Count - 1;
                }
                pgViewer.PagesViewer.View(pgViewer.CurrentPagesProvider, page);
            }));

        /// <summary>Identifies the <see cref="PagesViewer"/> dependency property.</summary>
        public static readonly DependencyPropertyKey PagesViewerPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(PagesViewer),
            typeof(Components.PagesViewer),
            typeof(PgViewer),
            new PropertyMetadata(default(Components.PagesViewer)));

        /// <summary>Identifies the <see cref="PagesViewer"/> dependency property.</summary>
        public static readonly DependencyProperty PagesViewerProperty = PagesViewerPropertyKey.DependencyProperty;

        /// <summary>Identifies the <see cref="Time"/> dependency property.</summary>
        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
            nameof(Time),
            typeof(DateTime),
            typeof(PgViewer),
            new PropertyMetadata(default(DateTime)));

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

        private readonly IValueConverter zaribConverter100;
        private readonly IValueConverter incrementalConverter;
        private readonly DataDb dataDb;
        private readonly PageNavigator pageNavigator;
        private readonly Timer timer;
        private Manga? manga;

        public PgViewer(
            DataDb dataDb,
            PageNavigator pageNavigator)
        {
            zaribConverter100 = new ZaribConverter() { Zarib = 100 };
            incrementalConverter = new IncrementalConverter() { Increment = 1 };
            this.dataDb = dataDb;
            this.pageNavigator = pageNavigator;
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
                Converter = incrementalConverter
            });
            TxtZoom.SetBinding(TextBox.TextProperty, new Binding
            {
                Source = PagesViewer,
                Path = new PropertyPath(nameof(PagesViewer.Zoom), null),
                Mode = BindingMode.TwoWay,
                Converter = zaribConverter100
            });

            timer = new()
            {
                AutoReset = true,
                Interval = 1000,
            };
            timer.Elapsed += (sender, e) => Dispatcher.Invoke(() => SetCurrentValue(TimeProperty, DateTime.Now));
            timer.Start();
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

        public DateTime Time
        {
            get => (DateTime)GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }
        public PagesProvider? CurrentPagesProvider { get; set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                timer.Dispose();
                Save();
            }
        }

        private void Save()
        {
            manga!.CurrentChapter = Chapters!.IndexOf(CurrentChapter!);
            manga.CurrentPage = PagesViewer!.Page;
            manga.Zoom = PagesViewer.Zoom;

            dataDb.Mangas.Update(manga);
        }

        public void View(string path)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.StartsWith("manga://", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(path.Split('/').Last(), NumberStyles.Any, CultureInfo.InvariantCulture, out int mangaId))
                {
                    throw new ArgumentException("Manga Id is not valid.", nameof(path));
                }
                manga = dataDb.Mangas.FindAll().FirstOrDefault(m => m.Id == mangaId);
                if (manga is null)
                {
                    throw new ArgumentException("Manga not found.", nameof(path));
                }
                Chapters!.Clear();
                foreach (FileSystemInfo chapter in GetChapterList(manga))
                {
                    Chapters.Add(chapter);
                }
                SetCurrentValue(CurrentChapterProperty, Chapters[manga.CurrentChapter]);
                PagesViewer!.SetCurrentValue(Components.PagesViewer.ZoomProperty, manga.Zoom);
            }
            ControlzEx.KeyboardNavigationEx.Focus(PagesViewer);

            pageNavigator!.Navigated += (sender, e) =>
            {
                if (e.Previous == this)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Save();
                    });
                }
            };
        }

        private void CmdNextChapter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int chapter = CboChapters.SelectedIndex + 1;
            if (chapter >= Chapters!.Count)
            {
                return;
            }
            SetCurrentValue(CurrentChapterProperty, Chapters[chapter]);
        }

        private void CmdPreviousChapter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int chapter = CboChapters.SelectedIndex - 1;
            if (chapter < 0)
            {
                return;
            }
            SetCurrentValue(CurrentChapterProperty, Chapters![chapter]);
        }

        private void CmdZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PagesViewer!.SetCurrentValue(Components.PagesViewer.ZoomProperty, PagesViewer.Zoom + 0.1);
        }

        private void CmdZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PagesViewer!.SetCurrentValue(Components.PagesViewer.ZoomProperty, PagesViewer.Zoom - 0.1);
        }
    }
}
