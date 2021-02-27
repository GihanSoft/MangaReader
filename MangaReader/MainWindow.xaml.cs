using GihanSoft.Navigation;
using GihanSoft.Views.AttachedProperties;

using MangaReader.Data;
using MangaReader.Views.Pages;

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

using Frame = GihanSoft.Navigation.Frame;

using static Microsoft.Extensions.DependencyInjection.ActivatorUtilities;
using GihanSoft.Navigation.Events;

namespace MangaReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly SettingsManager settingsManager;
        private readonly Frame NavFrame;

        public MainWindow()
        {
            settingsManager = GetServiceOrCreateInstance<SettingsManager>(App.Current.ServiceProvider);
            NavFrame = CreateInstance<Frame>(App.Current.ServiceProvider);
            InitializeComponent();

            NavBorder.Child = NavFrame;

            Binding canGoBackBinding = new Binding
            {
                Source = NavFrame.Navigator,
                Path = new PropertyPath(nameof(PageNavigator.CanGoBack))
            };
            BtnBack.SetBinding(IsEnabledProperty, canGoBackBinding);

            Binding canGoForwardBinding = new Binding
            {
                Source = NavFrame.Navigator,
                Path = new PropertyPath(nameof(PageNavigator.CanGoForward))
            };
            BtnForward.SetBinding(IsEnabledProperty, canGoForwardBinding);

            NavFrame.Navigator.Navigated += Navigator_Navigated;

            MainOptions mainOptions = settingsManager.GetMainOptions();
            Top = mainOptions.Appearance.WindowPosition.Top;
            Left = mainOptions.Appearance.WindowPosition.Left;
            Width = mainOptions.Appearance.WindowPosition.Width;
            Height = mainOptions.Appearance.WindowPosition.Height;
            WindowState = (WindowState)mainOptions.Appearance.WindowPosition.WindowsState;

            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length > 1)
            {
                NavFrame.Navigator.GoTo<PgViewer>();
                ((PgViewer)NavFrame.Navigator.Current).View(commandLineArgs[1]); //TODO: what?
            }
            else
            {
                NavFrame.Navigator.GoTo<PgLibrary>();
            }
        }

        private void Navigator_Navigated(object sender, NavigatedEventArgs e)
        {
            PageToolBar.Child = e.Future.LeftToolBar;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavFrame.Navigator.GoBack();
        }

        private void BtnForward_Click(object sender, RoutedEventArgs e)
        {
            NavFrame.Navigator.GoFroward();
        }

        private void FlyoutCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyout.IsOpen = false;
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            NavFrame.Navigator.GoTo<PgSettings>();
            FlyoutCancelBtn_Click(sender, e);
        }

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyout.IsOpen = true;
        }

        private void CmdToggleFullScreen_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            this.SetFullScreen(!this.GetFullScreen());
        }

        private void This_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            double y = e.GetPosition(this).Y;
            if (y < 30)
            {
                ShowTitleBar = true;
            }
            else if (y > 60 && this.GetFullScreen())
            {
                ShowTitleBar = false;
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            MainOptions mainOptions = settingsManager.Get<MainOptions>(MainOptions.Key);
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
            NavFrame.Navigator.GoBack();
        }

        private void CmdGoForward_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            NavFrame.Navigator.GoFroward();
        }

        private void CmdOpenMenu_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            MenuFlyout.IsOpen = true;
        }
    }
}
