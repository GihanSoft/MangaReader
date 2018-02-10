using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MangaReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static RoutedCommand nextChapterCmd = new RoutedCommand();
        public static RoutedCommand previousChapterCmd = new RoutedCommand();
        public static RoutedCommand zoomInCmd = new RoutedCommand();
        public static RoutedCommand zoomOutCmd = new RoutedCommand();
        public static RoutedCommand fullScreenCmd = new RoutedCommand();
        public static RoutedCommand homeCmd = new RoutedCommand();

        List<String> ChapterList;
        //int CurrentChapter;
        Models.MangaInfo CurrentManga;
        List<BitmapImage> imageList;
        System.Timers.Timer clockTimer;

        bool firstLoad = true;

        private MainWindow()
        {
            InitializeComponent();

            clockTimer = new System.Timers.Timer(1000) { AutoReset = true };
            clockTimer.Elapsed += ClockTmr_Elapsed;
            clockTimer.Start();

            PreviousBtn.ToolTip = "چپتر قبل\nhotkey: Ctrl + Left";
            NextBtn.ToolTip = "چپتر بعد\nhotkey: Ctrl + Right";
            FullScreenBtn.ToolTip = "حالت تمام صفحه\nhotkey: F11";
            HomeMenuBtn.ToolTip = "صفحه اصلی\nhotkey: Ctrl + H";

            BackgroundColorChooser.SelectedIndex = SettingApi.This.BackgroundColor;

            nextChapterCmd.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));
            previousChapterCmd.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
            zoomInCmd.InputGestures.Add(new KeyGesture(Key.Add, ModifierKeys.Control));
            zoomOutCmd.InputGestures.Add(new KeyGesture(Key.Subtract, ModifierKeys.Control));
            fullScreenCmd.InputGestures.Add(new KeyGesture(Key.F11));
            homeCmd.InputGestures.Add(new KeyGesture(Key.H, ModifierKeys.Control));
        }

        public MainWindow(Models.MangaInfo manga) : this()
        {
            CurrentManga = manga;

            ChapterList = new List<string>(Directory.EnumerateDirectories(manga.Address));
            ChapterList.Sort(NaturalStringComparer.Default.Compare);

            this.Loaded += MetroWindow_Loaded;
        }
        //need work
        public MainWindow(bool isSingleChapter) : this()
        {
            var chS = Environment.GetCommandLineArgs().ToList();
            chS.RemoveAt(0);
            foreach (var item in chS)
            {
                ChapterListCombo.Items.Add(new TextBlock()
                {
                    Text = item.Substring(item.LastIndexOf('\\') + 1)
                });
            }
            ChapterListCombo.SelectionChanged -= ChapterListCombo_SelectionChanged;
            Closing -= MetroWindow_Closing;
            ChapterListCombo.SelectedIndex = 0;
        }

        private void ClockTmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var This = sender as System.Timers.Timer;
            var now = DateTime.Now.ToString("HH:mm");
            try
            {
                Dispatcher.Invoke(() =>
                {
                    if (ClockViewer.Text != now)
                        This.Interval = 60 * 1000;
                    ClockViewer.Text = now;
                });
            }
            catch { }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ChapterListCombo.ItemsSource = null;
            ChapterListCombo.ItemsSource = ChapterList.Select(chapter => chapter.Substring(chapter.LastIndexOf('\\') + 1));

            ChapterListCombo.SelectedIndex = CurrentManga.CurrentChapter;
            CurrentPage.Text = "1";

            PagesScroll.ScrollToVerticalOffset(CurrentManga.CurrentPlace);
            PagesScroll.Focus();
        }

        private void LoadImage(string ChapterUri)
        {
            List<string> list;
            try
            {
                list = Directory.EnumerateFiles(ChapterUri, "*.*", SearchOption.AllDirectories).ToList();
            }
            catch
            {
                MessageBox.Show("خطا در بارگذاری مانگا", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.RightAlign);
                return;
            }
            list = list.Where(file =>
            file.ToLower().EndsWith("jpg") || file.ToLower().EndsWith("png") ||
            file.ToLower().EndsWith("bmp") || file.ToLower().EndsWith("gif") ||
            file.ToLower().EndsWith("jpeg")).ToList();
            list.Sort(NaturalStringComparer.Default.Compare);

            imageList = new List<BitmapImage>();
            foreach (var item in list)
            {
                try
                {
                    Uri uri = new Uri(item);
                    BitmapImage bitmap = null;
                    Dispatcher.Invoke(() => bitmap = new BitmapImage(uri));
                    imageList.Add(bitmap);
                    Thread.Sleep(1);
                }
                catch
                {
                    MessageBox.Show("خطا در بارگذاری تصویر : " + item, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.RightAlign);
                }
            }
        }

        private void AddImage()
        {
            Dispatcher.Invoke(() =>
            {
                Pages.Children.Clear();
                PagesCount.Text = '/' + imageList.Count.ToString();
            });
            foreach (BitmapImage image in imageList)
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        var ImageCtrl = new Image
                        {
                            Source = image,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Width = SystemParameters.FullPrimaryScreenWidth * 0.55,
                            MaxWidth = SystemParameters.FullPrimaryScreenWidth - 20,
                            Margin = new Thickness(0, 10, 0, 10)
                        };

                        if (image.Width > image.Height)
                        {
                            ImageCtrl.Width = double.NaN;
                            ImageCtrl.Height = SystemParameters.FullPrimaryScreenHeight - 10;
                        }
                        Pages.Children.Add(ImageCtrl);
                    });
                    Thread.Sleep(1);
                }
                catch { }
            }
        }

        private void HomeMenu_Click(object sender, RoutedEventArgs e)
        {
            SettingApi.This.MangaList[CurrentManga.ID].CurrentPlace = PagesScroll.VerticalOffset * (100 / double.Parse(ZoomPersent.Text));
            var mangaChWin = new MangaChooser();
            mangaChWin.Show();
            Close();
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentManga.CurrentChapter != 0)
            {
                ChapterListCombo.SelectedIndex = --CurrentManga.CurrentChapter;
            }
            else
            {
                MessageBox.Show("این چپتر اول است.", "خطا", MessageBoxButton.OK, MessageBoxImage.Warning,
                    MessageBoxResult.OK, MessageBoxOptions.RtlReading);
            }
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentManga.CurrentChapter + 1 != ChapterList.Count)
            {
                ChapterListCombo.SelectedIndex = ++CurrentManga.CurrentChapter;
            }
            else
            {
                var nList = Directory.EnumerateDirectories(CurrentManga.Address).ToList();
                nList.Sort(NaturalStringComparer.Default.Compare);
                if (nList.Count != ChapterList.Count)
                {
                    ChapterList = nList;
                    ChapterListCombo.ItemsSource = null;
                    ChapterListCombo.ItemsSource = ChapterList.Select(chapter => chapter.Substring(chapter.LastIndexOf('\\') + 1));

                    ChapterListCombo.SelectedIndex = ++CurrentManga.CurrentChapter;
                    //Next_Click(sender, e);
                    return;
                }
                MessageBox.Show("این چپتر آخر است.", "خطا", MessageBoxButton.OK, MessageBoxImage.Warning,
                    MessageBoxResult.OK, MessageBoxOptions.RtlReading);
            }
        }

        //need work
        private async void ChapterListCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChapterListCombo.SelectedIndex == -1) return;
            CurrentManga.CurrentChapter = ChapterListCombo.SelectedIndex;
            var zoom = ZoomPersent.Text;
            await Task.Run(() => LoadImage(ChapterList[CurrentManga.CurrentChapter]));
            if (!firstLoad)
            {
                PagesScroll.ScrollToVerticalOffset(0);
                CurrentPage.Text = "1";
            }
            else
                firstLoad = false;
            await Task.Run(() => AddImage());
            ZoomPersent.Text = "100";
            ZoomPersent.Text = zoom;
            PagesScroll.Focus();
        }


        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SettingApi.This.MangaList[CurrentManga.ID].CurrentPlace = PagesScroll.VerticalOffset * (100 / double.Parse(ZoomPersent.Text));
        }

        WindowState previousState = WindowState.Maximized;

        private void FullScreenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (FullScreenIcon.Kind == MahApps.Metro.IconPacks.PackIconMaterialKind.Fullscreen)
            {
                FullScreenIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.FullscreenExit;
                previousState = this.WindowState;
                WindowState = WindowState.Maximized;
                IgnoreTaskbarOnMaximize = true;
            }
            else
            {
                FullScreenIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Fullscreen;
                MyToolBar.Visibility = Visibility.Visible;
                WindowState = previousState;
                IgnoreTaskbarOnMaximize = false;
            }
            ShowTitleBar = !ShowTitleBar;
        }

        private void ZoomInBtn_Click(object sender, RoutedEventArgs e)
        {
            ZoomPersent.Text = (double.Parse(ZoomPersent.Text) + 10).ToString();
        }
        private void ZoomOutBtn_Click(object sender, RoutedEventArgs e)
        {
            ZoomPersent.Text = (double.Parse(ZoomPersent.Text) - 10).ToString();
        }

        string oldText = "100";
        private void ZoomPersent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ZoomPersent.Text == "") return;
            if (double.Parse(ZoomPersent.Text) > 300) ZoomPersent.Text = "300";
            if (double.Parse(ZoomPersent.Text) < 5) ZoomPersent.Text = "5";

            try
            {
                var zoom = double.Parse(ZoomPersent.Text) / 100;
                var oldZoom = double.Parse(oldText);
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
                        i.Width = SystemParameters.FullPrimaryScreenWidth * 0.55 * zoom;
                }
                var zn = (double.Parse(ZoomPersent.Text) / oldZoom);
                var offset = PagesScroll.VerticalOffset * zn;
                PagesScroll.ScrollToVerticalOffset(offset);
                var hoffset = ((SystemParameters.FullPrimaryScreenWidth - 20) * (zn - 1));
            }
            catch (Exception) { }
            oldText = ZoomPersent.Text;
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
            if (BackgroundColorChooser.SelectedIndex == 0)
            {
                Background = new SolidColorBrush(Colors.Black);
                ClockViewer.Foreground = new SolidColorBrush(Color.FromRgb(250, 250, 250));
                SettingApi.This.BackgroundColor = 0;
            }
            else
            {
                Background = new SolidColorBrush(Color.FromRgb(250, 250, 250));
                ClockViewer.Foreground = new SolidColorBrush(Colors.Black);
                SettingApi.This.BackgroundColor = 1;
            }
            Properties.Settings.Default.Save();
        }

        System.Timers.Timer tmr;
        Point firstPoint = new Point();
        bool scrollActive = false;
        double vOff = 1;
        private void PagesScroll_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                if (!scrollActive)
                {
                    tmr = new System.Timers.Timer(33) { AutoReset = true };
                    tmr.Elapsed += Tmr_Elapsed;
                    tmr.Start();
                    firstPoint = e.GetPosition(PagesScroll);
                    vOff = PagesScroll.VerticalOffset;
                    PagesScroll.CaptureMouse();
                    PagesScroll.Cursor = Cursors.ScrollNS;
                }
                else
                {
                    tmr.Stop();
                    PagesScroll.ReleaseMouseCapture();
                    PagesScroll.Cursor = null;
                }
                scrollActive = !scrollActive;
            }
        }
        private void Tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
           {
               if (PagesScroll.IsMouseCaptured)
               {
                   if (Mouse.PrimaryDevice.GetPosition(PagesScroll).Y > firstPoint.Y + 5)
                       PagesScroll.Cursor = Cursors.ScrollS;
                   else if (Mouse.PrimaryDevice.GetPosition(PagesScroll).Y + 5 < firstPoint.Y)
                       PagesScroll.Cursor = Cursors.ScrollN;
                   else
                   {
                       PagesScroll.Cursor = Cursors.ScrollNS;
                       return;
                   }
                   PagesScroll.ScrollToVerticalOffset(PagesScroll.VerticalOffset + Mouse.PrimaryDevice.GetPosition(PagesScroll).Y - firstPoint.Y);
               }
           }
            );
        }

        private void PagesScroll_MouseMove(object sender, MouseEventArgs e)
        {
            if (!ShowTitleBar)
            {
                if (e.GetPosition(PagesScroll).Y <= 20)
                    MyToolBar.Visibility = Visibility.Visible;
                else
                    MyToolBar.Visibility = Visibility.Collapsed;
            }
        }

        private void MetroWindow_StateChanged(object sender, EventArgs e)
        {
            previousState = this.WindowState;
            if (FullScreenIcon.Kind == MahApps.Metro.IconPacks.PackIconMaterialKind.FullscreenExit)
                FullScreenBtn_Click(this, new RoutedEventArgs());
        }

        private void PagesScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
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
            catch { }
        }
    }
}
