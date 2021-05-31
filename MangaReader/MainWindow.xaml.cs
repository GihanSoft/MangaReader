using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using GihanSoft.Navigation;
using GihanSoft.Views.AttachedProperties;

using MahApps.Metro.Controls;

using MangaReader.Data;
using MangaReader.Views.Pages;

namespace MangaReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    internal partial class MainWindow
    {
        private static readonly DependencyPropertyKey PageNavigatorPropertyKey = DependencyProperty.RegisterReadOnly(
                    nameof(PageNavigator),
                    typeof(PageNavigator),
                    typeof(MainWindow),
                    new PropertyMetadata(default(PageNavigator)));

        /// <summary>Identifies the <see cref="PageNavigator"/> dependency property.</summary>
        public static readonly DependencyProperty PageNavigatorProperty = PageNavigatorPropertyKey.DependencyProperty;

        private readonly SettingsManager settingsManager;

        public MainWindow(
            SettingsManager settingsManager,
            PageNavigator pageNavigator)
        {
            InitializeComponent();
            this.settingsManager = settingsManager;
            var mainOptions = settingsManager.GetMainOptions();

            Top = mainOptions.Appearance.WindowPosition.Top;
            Left = mainOptions.Appearance.WindowPosition.Left;
            Width = mainOptions.Appearance.WindowPosition.Width;
            Height = mainOptions.Appearance.WindowPosition.Height;
            WindowState = (WindowState)mainOptions.Appearance.WindowPosition.WindowsState;

            PageNavigator = pageNavigator;

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (PageNavigator is null)
            {
                return;
            }
            if (App.Current.Properties.Contains(App.ArgumentsKey))
            {
                await PageNavigator.GoToAsync<PgViewer>().ConfigureAwait(false);
                if (PageNavigator.CurrentPage is PgViewer pgViewer &&
                    App.Current.Properties[App.ArgumentsKey] is string[] args)
                {
                    pgViewer.View(args[0]);
                }
            }
            else
            {
                try
                {
                    await PageNavigator.GoToAsync<PgLibrary>().ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    //
                }
            }
        }

        /// <summary>
        /// Gets Page Navigator.
        /// </summary>
        public PageNavigator? PageNavigator
        {
            get => (PageNavigator?)GetValue(PageNavigatorProperty);
            private set => SetValue(PageNavigatorPropertyKey, value);
        }

        private void FlyoutCancelBtn_Click(object sender, RoutedEventArgs e) => MenuFlyout.SetCurrentValue(Flyout.IsOpenProperty, false);

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.GoToAsync<PgSettings>();
            FlyoutCancelBtn_Click(sender, e);
        }

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.GoToAsync<PgHelp>();
            FlyoutCancelBtn_Click(sender, e);
        }

        private void CmdToggleFullScreen_Executed(object sender, ExecutedRoutedEventArgs e) => this.SetFullScreen(!this.GetFullScreen());

        private void This_MouseMove(object sender, MouseEventArgs e)
        {
            if (!this.GetFullScreen())
            {
                return;
            }

            var y = e.GetPosition(this).Y;
            if (y is < 30)
            {
                SetCurrentValue(ShowTitleBarProperty, true);
            }
            else if (y is > 60 && !ToolBar.IsMouseOver)
            {
                SetCurrentValue(ShowTitleBarProperty, false);
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            var mainOptions = settingsManager.GetMainOptions();
            Rect restoreBounds;
            WindowState windowState;
            if (this.GetFullScreen())
            {
                restoreBounds = this.GetRealRestoreBounds();
                windowState = this.GetPreWindowsState();
            }
            else
            {
                restoreBounds = RestoreBounds;
                windowState = WindowState;
            }

            mainOptions.Appearance.WindowPosition.Top = restoreBounds.Top;
            mainOptions.Appearance.WindowPosition.Left = restoreBounds.Left;
            mainOptions.Appearance.WindowPosition.Width = restoreBounds.Width;
            mainOptions.Appearance.WindowPosition.Height = restoreBounds.Height;
            mainOptions.Appearance.WindowPosition.WindowsState = (byte)windowState;

            settingsManager.Save(MainOptions.Key, mainOptions);
        }

        private void CmdGoBack_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (PageNavigator.CanGoBack)
            {
                PageNavigator.GoBackAsync();
            }
        }

        private void CmdGoForward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (PageNavigator.CanGoForward)
            {
                PageNavigator.GoFrowardAsync();
            }
        }

        private void CmdOpenMenu_Executed(object sender, ExecutedRoutedEventArgs e) => MenuFlyout.SetCurrentValue(Flyout.IsOpenProperty, true);

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.GoToAsync<PgLibrary>();
            MenuFlyout.SetCurrentValue(Flyout.IsOpenProperty, false);
        }
    }
}
