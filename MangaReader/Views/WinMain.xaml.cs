using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Gihan.Manga.Reader.Controllers;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;

using MangaReader.Controllers;
using MangaReader.Data.Models;

namespace MangaReader.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WinMain : MetroWindow
    {
        //public static RoutedCommand NextChapterCmd = new RoutedCommand();
        //public static RoutedCommand PreviousChapterCmd = new RoutedCommand();
        //public static RoutedCommand ZoomInCmd = new RoutedCommand();
        //public static RoutedCommand ZoomOutCmd = new RoutedCommand();
        //public static RoutedCommand FullScreenCmd = new RoutedCommand();
        //public static RoutedCommand HomeCmd = new RoutedCommand();

        private List<string> _chapterList;
        private readonly Manga _currentManga;
        private List<BitmapImage> _imageList;

        private bool _firstLoad = true;

        //private ComboBox ChapterListCombo;
        //private TextBox CurrentPage;
        //private TextBox ZoomPersent;
        //private TextBox PagesCount;
        //private PackIconMaterial FullScreenIcon;

        public WinMain()
        {
            InitializeComponent();

            //ChapterListCombo = ((Resources["LeftToolBar"] as FrameworkElement).FindName("ChapterListCombo") as ComboBox);
            //CurrentPage = ((Resources["LeftToolBar"] as FrameworkElement).FindName("CurrentPage") as TextBox);
            //ZoomPersent = ((Resources["LeftToolBar"] as FrameworkElement).FindName("ZoomPersent") as TextBox);
            //PagesCount = ((Resources["LeftToolBar"] as FrameworkElement).FindName("PagesCount") as TextBox);
            //FullScreenIcon = ((Resources["LeftToolBar"] as FrameworkElement).FindName("FullScreenIcon") as PackIconMaterial);

            var clockTimer = new System.Timers.Timer(1000) { AutoReset = true };
            clockTimer.Elapsed += ClockTmr_Elapsed;
            clockTimer.Start();

            //PreviousBtn.ToolTip = "چپتر قبل\nhotkey: Ctrl + Left";
            //NextBtn.ToolTip = "چپتر بعد\nhotkey: Ctrl + Right";
            //FullScreenBtn.ToolTip = "حالت تمام صفحه\nhotkey: F11";
            //HomeMenuBtn.ToolTip = "صفحه اصلی\nhotkey: Ctrl + H";

            //BackgroundColorChooser.SelectedIndex = SettingApi.This.BackgroundColor;

            _autoScrollTimer = new System.Timers.Timer(33) { AutoReset = true };
            _autoScrollTimer.Elapsed += AutoScrollTimer_Elapsed;

            //NextChapterCmd.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));
            //PreviousChapterCmd.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
            //ZoomInCmd.InputGestures.Add(new KeyGesture(Key.Add, ModifierKeys.Control));
            //ZoomOutCmd.InputGestures.Add(new KeyGesture(Key.Subtract, ModifierKeys.Control));
            //FullScreenCmd.InputGestures.Add(new KeyGesture(Key.F11));
            //HomeCmd.InputGestures.Add(new KeyGesture(Key.H, ModifierKeys.Control));

            var args = Environment.GetCommandLineArgs().ToList();
            args.RemoveAt(0);
            if (args.Count == 0) return;
            if (args.Contains("-m"))
            {
                var i = args.IndexOf("-m");
                var mangaId = args.ElementAt(i + 1);
                var manga = SettingApi.This.MangaList[int.Parse(mangaId)];
                _currentManga = manga;
                _chapterList = GetChapterList(manga).ToList();
                return;
            }
            _chapterList = args;
            ChapterListCombo.ItemsSource = _chapterList.Select(ch => ch.Substring(ch.LastIndexOf('\\') + 1));
            _currentManga = new Manga()
            {
                Id = -1, // to not save on closing
                CurrentChapter = 0
            };
        }

        public WinMain(Manga manga) : this()
        {
            _currentManga = manga;
            _chapterList = GetChapterList(manga).ToList();
        }

        private void ClockTmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var strongSender = sender as System.Timers.Timer;
            // if(strongSender != null) return;
            var now = DateTime.Now.ToString("HH:mm");
            try
            {
                Dispatcher.Invoke(() =>
                {
                    if (ClockViewer.Text != now)
                        if (strongSender != null)
                            strongSender.Interval = 60 * 1000;
                    ClockViewer.Text = now;
                });
            }
            catch
            {
                // ignored
            }
        }

        public async void MetroWindow_Loaded(object? sender, RoutedEventArgs? e)
        {
            while (ChapterListCombo is null)
            {
                await Task.Delay(10);
            }
            ChapterListCombo.ItemsSource = null;
            ChapterListCombo.ItemsSource = _chapterList.Select(chapter => chapter.Substring(chapter.LastIndexOf('\\') + 1));

            ChapterListCombo.SelectedIndex = _currentManga.CurrentChapter;
            CurrentPage.Text = "1";

            PagesScroll.ScrollToVerticalOffset(_currentManga.Offset);
            ZoomPersent.Text = string.IsNullOrWhiteSpace((_currentManga.Zoom * 100).ToString()) ? "100" : (_currentManga.Zoom * 100).ToString();
            PagesScroll.Focus();
        }

        private void LoadImage(string chapterUri)
        {
            if (FileTypeList.CompressedType.Any(t => chapterUri.ToLower().EndsWith(t)))
                LoadImageCompressed(chapterUri);
            else
                LoadImageNormal(chapterUri);
        }
        private void LoadImageCompressed(string uri)
        {
            var path = CompressApi.OpenArchive(uri);
            LoadImageNormal(path);
        }
        private void LoadImageNormal(string uri)
        {
            List<string> list;
            try
            {
                list = Directory.EnumerateFiles(uri, "*.*", SearchOption.AllDirectories).ToList();
            }
            catch
            {
                MessageBox.Show("خطا در بارگذاری مانگا", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.RightAlign);
                return;
            }
            list = list.Where(file =>
                FileTypeList.ImageTypes.Any(t => file.ToLower().EndsWith(t))).ToList();
            list.Sort(NaturalStringComparer.Default.Compare);

            _imageList = new List<BitmapImage>();
            foreach (var item in list)
            {
                try
                {
                    var imgUri = new Uri(item, UriKind.Relative);
                    BitmapImage bitmap = null;
                    Dispatcher.Invoke(() =>
                    {
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = File.OpenRead(imgUri.OriginalString);
                        bitmap.EndInit();
                    });
                    _imageList.Add(bitmap);
                    Thread.Sleep(1);
                }
                catch (Exception err)
                {
                    MessageBox.Show("خطا در بارگذاری تصویر : " + item + "\n\n" + err.Message, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.RightAlign);
                }
            }
        }

        private void AddImage()
        {
            Dispatcher.Invoke(() =>
            {
                Pages.Children.Clear();
                PagesCount.Text = '/' + _imageList.Count.ToString();
            });
            foreach (var image in _imageList)
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        var imageCtrl = new Image
                        {
                            Source = image,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Width = SystemParameters.FullPrimaryScreenWidth * 0.55,
                            MaxWidth = SystemParameters.FullPrimaryScreenWidth - 20,
                            Margin = new Thickness(0, 10, 0, 10)
                        };

                        if (image.Width > image.Height)
                        {
                            imageCtrl.Width = double.NaN;
                            imageCtrl.Height = SystemParameters.FullPrimaryScreenHeight - 10;
                        }
                        Pages.Children.Add(imageCtrl);
                    });
                    Thread.Sleep(2);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void HomeMenu_Click(object sender, RoutedEventArgs e)
        {
            //new WinMangaChooser().Show();
            var h = new WinMangaChooser();
            h.Show();
            Close();
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (_currentManga.CurrentChapter != 0)
            {
                ChapterListCombo.SelectedIndex = --_currentManga.CurrentChapter;
            }
            else
            {
                this.ShowMessageAsync("خطا", "این چپتر اول است.", MessageDialogStyle.Affirmative);
            }
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (_currentManga.CurrentChapter + 1 != _chapterList.Count)
            {
                ChapterListCombo.SelectedIndex = ++_currentManga.CurrentChapter;
            }
            else
            {
                var nList = GetChapterList(_currentManga);
                if (nList.Count() != _chapterList.Count)
                {
                    _chapterList = nList.ToList();
                    ChapterListCombo.ItemsSource = null;
                    ChapterListCombo.ItemsSource = _chapterList.Select(chapter => chapter.Substring(chapter.LastIndexOf('\\') + 1));

                    ChapterListCombo.SelectedIndex = ++_currentManga.CurrentChapter;
                    return;
                }
                this.ShowMessageAsync("خطا", "این چپتر آخر است.", MessageDialogStyle.Affirmative);
            }
        }

        //need work
        private async void ChapterListCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChapterListCombo.SelectedIndex == -1) return;
            _currentManga.CurrentChapter = ChapterListCombo.SelectedIndex;
            RleaseImages();
            await Task.Run(() => LoadImage(_chapterList[_currentManga.CurrentChapter]));
            if (!_firstLoad)
            {
                PagesScroll.ScrollToVerticalOffset(0);
                _currentManga.Offset = 0;
                CurrentPage.Text = "1";
            }
            else
                _firstLoad = false;
            await Task.Run(() => AddImage());
            ZoomPersent_TextChanged(null, null);
            PagesScroll.Focus();
        }


        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RleaseImages();
            if (_currentManga.Id == -1) return;
            //SettingApi.This.MangaList[_currentManga.Id].CurrentPlace =
            //    PagesScroll.VerticalOffset * (100 / double.Parse(ZoomPersent.Text));
            //SettingApi.This.MangaList[_currentManga.Id].Zoom = double.Parse(ZoomPersent.Text) / 100;
        }

        private void RleaseImages()
        {
            if (_imageList == null) return;
            foreach (BitmapImage item in _imageList)
            {
                item.StreamSource.Close();
            }
            CompressApi.CleanExtractPath();
        }

        WindowState _previousState = WindowState.Maximized;

        private void FullScreenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (FullScreenIcon.Kind == MahApps.Metro.IconPacks.PackIconMaterialKind.Fullscreen)
            {
                IgnoreTaskbarOnMaximize = true;
                FullScreenIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.FullscreenExit;
                _previousState = WindowState;
                WindowState = WindowState.Maximized;
            }
            else
            {
                IgnoreTaskbarOnMaximize = false;
                FullScreenIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Fullscreen;
                MyToolBar.Visibility = Visibility.Visible;
                WindowState = _previousState;
            }
            ShowTitleBar = !ShowTitleBar;
        }

        private void ZoomInBtn_Click(object sender, RoutedEventArgs e)
        {
            ZoomPersent.Text = (double.Parse(ZoomPersent.Text) + 10).ToString(CultureInfo.InvariantCulture);
        }
        private void ZoomOutBtn_Click(object sender, RoutedEventArgs e)
        {
            ZoomPersent.Text = (double.Parse(ZoomPersent.Text) - 10).ToString(CultureInfo.InvariantCulture);
        }

        private string _oldText = "100";
        private async void ZoomPersent_TextChanged(object sender, TextChangedEventArgs e)
        {
            while (ZoomPersent is null)
            {
                await Task.Delay(10);
            }
            if (Pages is null)
                return;
            if (ZoomPersent.Text?.Length == 0)
            {
                return;
            }

            if (double.Parse(ZoomPersent.Text) > 300)
            {
                ZoomPersent.Text = "300";
                return;
            }
            if (double.Parse(ZoomPersent.Text) < 5)
            {
                ZoomPersent.Text = "5";
                return;
            }

            try
            {
                var zoom = double.Parse(ZoomPersent.Text) / 100;
                var oldZoom = double.Parse(_oldText);
                foreach (var item in Pages.Children)
                {
                    var i = (item as Image);
                    i.Margin = new Thickness(0, 10 * zoom, 0, 10 * zoom);
                    if (double.IsNaN(i.Width))
                    {
                        i.Height = (SystemParameters.FullPrimaryScreenHeight - 10) * zoom;
                        i.MaxWidth = (SystemParameters.FullPrimaryScreenWidth - 20) * zoom;
                    }
                    else
                    {
                        i.Width = SystemParameters.FullPrimaryScreenWidth * 0.55 * zoom;
                    }
                }
                var zn = (double.Parse(ZoomPersent.Text) / oldZoom);
                var offset = PagesScroll.VerticalOffset == 0 ?
                    _currentManga.Offset * zn
                    : PagesScroll.VerticalOffset * zn;
                PagesScroll.ScrollToVerticalOffset(offset);
                var hoffset = ((SystemParameters.FullPrimaryScreenWidth - 20) * (zn - 1));
            }
            catch (Exception)
            {
                // ignored
            }

            _oldText = ZoomPersent.Text;
        }

        private void MetroWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Delta > 0)
                    ZoomPersent.Text = (double.Parse(ZoomPersent.Text) + 5).ToString();
                if (e.Delta < 0)
                    ZoomPersent.Text = (double.Parse(ZoomPersent.Text) - 5).ToString();
            }
        }

        private void NextChapterCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Next_Click(this, new RoutedEventArgs());
        }
        private void PreviousChapterCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Previous_Click(this, new RoutedEventArgs());
        }
        private void ZoomInCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomInBtn_Click(this, new RoutedEventArgs());
        }
        private void ZoomOutCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomOutBtn_Click(this, new RoutedEventArgs());
        }
        private void FullScreenCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FullScreenBtn_Click(this, new RoutedEventArgs());
        }
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HomeMenu_Click(this, new RoutedEventArgs());
        }

        private void PagesScroll_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.Right:
                        Next_Click(this, new RoutedEventArgs());
                        break;
                    case Key.Left:
                        Previous_Click(this, new RoutedEventArgs());
                        break;
                }
            }
        }

        private void BackgroundColorChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        readonly System.Timers.Timer _autoScrollTimer;
        Point _firstPoint;
        bool _scrollActive;
        double _vOff;
        private void PagesScroll_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && !_scrollActive)
            {
                _autoScrollTimer.Start();
                _firstPoint = e.GetPosition(PagesScroll);
                _vOff = PagesScroll.VerticalOffset;
                PagesScroll.CaptureMouse();
                PagesScroll.Cursor = Cursors.ScrollNS;
                _scrollActive = true;
            }
            else
            {
                _autoScrollTimer.Stop();
                PagesScroll.ReleaseMouseCapture();
                PagesScroll.Cursor = null;
                _scrollActive = false;
            }
        }

        private void AutoScrollTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (PagesScroll.IsMouseCaptured)
                {
                    if (Mouse.PrimaryDevice.GetPosition(PagesScroll).Y > _firstPoint.Y + 5)
                        PagesScroll.Cursor = Cursors.ScrollS;
                    else if (Mouse.PrimaryDevice.GetPosition(PagesScroll).Y + 5 < _firstPoint.Y)
                        PagesScroll.Cursor = Cursors.ScrollN;
                    else
                    {
                        PagesScroll.Cursor = Cursors.ScrollNS;
                        return;
                    }
                    PagesScroll.ScrollToVerticalOffset(PagesScroll.VerticalOffset + Mouse.PrimaryDevice.GetPosition(PagesScroll).Y - _firstPoint.Y);
                }
            });
        }

        private void PagesScroll_MouseMove(object sender, MouseEventArgs e)
        {
            if (!ShowTitleBar)
            {
                MyToolBar.Visibility = e.GetPosition(PagesScroll).Y <= 20 ?
                    Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void MetroWindow_StateChanged(object sender, EventArgs e)
        {
            _previousState = this.WindowState;
            if (FullScreenIcon.Kind == MahApps.Metro.IconPacks.PackIconMaterialKind.FullscreenExit)
                FullScreenBtn_Click(this, new RoutedEventArgs());
        }

        private async void PagesScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            while (CurrentPage is null)
            {
                await Task.Delay(10);
            }
            if (CurrentPage.Text == "")
                return;
            int end;
            int step;
            if (e != null)
                if (e.VerticalChange > 0)
                {
                    end = Pages.Children.Count;
                    step = 1;
                }
                else
                {
                    end = -1;
                    step = -1;
                }
            else
            {
                end = Pages.Children.Count;
                step = 1;
            }
            try
            {
                for (int i = int.Parse(CurrentPage.Text) - 1; i != end; i += step)
                {
                    var offset = (Pages.Children[i] as Image).TransformToAncestor(PagesScroll).Transform(new Point(0, 0)).Y;
                    if (offset < (PagesScroll.ActualHeight / 2) && (offset >= 0 || step == -1 || i + 1 > int.Parse(CurrentPage.Text)))
                    {
                        CurrentPage.Text = (i + 1).ToString();
                        if (step == -1)
                            break;
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        private IEnumerable<string> GetChapterList(Manga manga)
        {
            var dir = new DirectoryInfo(manga.path);
            var chapters = dir.EnumerateFileSystemInfos()
                .Where(item => !(item is FileInfo file) ||
                FileTypeList.CompressedType.Any(t => file.Name.EndsWith(t)));
            chapters = chapters.OrderBy(item => item is FileInfo file ?
                Path.GetFileNameWithoutExtension(file.Name) : item.Name,
                    NaturalStringComparer.Default);
            return chapters.Select(ch => ch.FullName);
        }
    }
}
