using GihanSoft.Navigation;
using GihanSoft.Views.AttachedProperties;

using MangaReader.Data;
using MangaReader.Views.Pages;

using System;
using System.ComponentModel;
using System.Windows;

using PageHost = GihanSoft.Navigation.PageHost;

using static Microsoft.Extensions.DependencyInjection.ActivatorUtilities;

namespace MangaReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [CLSCompliant(false)]
    public partial class MainWindow
    {
        private readonly SettingsManager settingsManager;

        public MainWindow()
        {
            InitializeComponent();
            settingsManager = GetServiceOrCreateInstance<SettingsManager>(App.Current.ServiceProvider);
            MainOptions mainOptions = settingsManager.GetMainOptions();

            Top = mainOptions.Appearance.WindowPosition.Top;
            Left = mainOptions.Appearance.WindowPosition.Left;
            Width = mainOptions.Appearance.WindowPosition.Width;
            Height = mainOptions.Appearance.WindowPosition.Height;
            WindowState = (WindowState)mainOptions.Appearance.WindowPosition.WindowsState;

            PageHost.PageNavigator = new PageNavigator(App.Current.ServiceProvider);

            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length > 1)
            {
                PageHost.PageNavigator.GoTo<PgViewer>();
                ((PgViewer)PageHost.PageNavigator.Current).View(commandLineArgs[1]); //TO DO: what?
            }
            else
            {
                PageHost.PageNavigator.GoTo<PgLibrary>();
            }
        }

        private void FlyoutCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyout.IsOpen = false;
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            PageHost.PageNavigator!.GoTo<PgSettings>();
            FlyoutCancelBtn_Click(sender, e);
        }

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            PageHost.PageNavigator!.GoTo<PgHelp>();
        }

        private void CmdToggleFullScreen_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            this.SetFullScreen(!this.GetFullScreen());
        }

        private void This_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!this.GetFullScreen())
            {
                return;
            }

            double y = e.GetPosition(this).Y;
            if (y < 30)
            {
                ShowTitleBar = true;
            }
            else if (y > 60 && !ToolBar.IsMouseOver)
            {
                ShowTitleBar = false;
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            MainOptions mainOptions = settingsManager.GetMainOptions();
            Rect restoreBounds;
            WindowState windowState;
            if (this.GetFullScreen())
            {
                restoreBounds = this.GetRealRestoreBounds();
                windowState = WindowState.Maximized;
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
            if (PageHost.PageNavigator!.CanGoBack)
            {
                PageHost.PageNavigator.GoBack();
            }
        }

        private void CmdGoForward_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            PageHost.PageNavigator!.GoFroward();
        }

        private void CmdOpenMenu_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            MenuFlyout.IsOpen = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PageHost.PageNavigator!.GoTo<PgLibrary>();
            MenuFlyout.IsOpen = false;
        }
    }
}
